using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using Coplt.Analyzers.Generators.Templates;
using Coplt.Union.Analyzers.Generators.Templates;
using Coplt.Union.Analyzers.Resources;
using Coplt.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coplt.Union.Analyzers.Generators;

[Generator]
public class UnionGenerator : IIncrementalGenerator
{
    public const string Id = "Union";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Coplt.Union.UnionAttribute",
            static (syntax, _) => syntax is StructDeclarationSyntax or ClassDeclarationSyntax,
            static (ctx, _) =>
            {
                var diagnostics = new List<Diagnostic>();
                var compilation = ctx.SemanticModel.Compilation;
                var attr = ctx.Attributes.First();
                var union_attr = UnionAttr.FromData(attr, diagnostics);
                var syntax = (TypeDeclarationSyntax)ctx.TargetNode;
                var semantic_model = ctx.SemanticModel;
                var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                var GenBase = Utils.BuildGenBase(syntax, symbol, compilation);
                var IsReadOnly = symbol.IsReadOnly;
                var IsClass = syntax is ClassDeclarationSyntax;
                var HasToString = symbol.GetMembers("ToString")
                    .Where(s => s is IMethodSymbol)
                    .Cast<IMethodSymbol>()
                    .Any(s => s.TypeParameters.Length == 0 && s.Parameters.Length == 0);
                var NamedArguments = attr.NamedArguments.ToDictionary(a => a.Key, a => a.Value);
                var GenToString = NamedArguments.TryGetValue("GenerateToString", out var _GenerateToString)
                    ? (bool)_GenerateToString.Value!
                    : !HasToString;
                var GenEquals = !NamedArguments.TryGetValue("GenerateEquals", out var _GenerateEquals) ||
                                (bool)_GenerateEquals.Value!;
                var GenCompareTo = !NamedArguments.TryGetValue("GenerateCompareTo", out var _GenerateCompareTo) ||
                                   (bool)_GenerateCompareTo.Value!;
                var GenMethods = new UnionGenerateMethod(GenToString, GenEquals, GenCompareTo);
                var Templates = syntax.Members
                    .Where(static t => t is InterfaceDeclarationSyntax)
                    .Cast<InterfaceDeclarationSyntax>()
                    .Select(t =>
                    {
                        var symbol = semantic_model.GetDeclaredSymbol(t);
                        return (t, symbol);
                    })
                    .Where(i =>
                    {
                        var (_, symbol) = i;
                        return symbol!.GetAttributes().Any(a =>
                            a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionTemplateAttribute");
                    })
                    .ToList();
                if (Templates.Count > 1)
                {
                    var desc = Utils.MakeWarning(Id, Strings.Get("Generator.Union.Error.MultiTemplate"));
                    foreach (var t in Templates)
                    {
                        diagnostics.Add(Diagnostic.Create(desc, t.t.Identifier.GetLocation()));
                    }
                }
                var cases = ImmutableArray.CreateBuilder<UnionCase>();
                var AnyGeneric = false;
                foreach (var (template, _) in Templates)
                {
                    foreach (var (member, i) in template.Members.Select((a, b) => (a, b)))
                    {
                        if (member is MethodDeclarationSyntax mds)
                        {
                            var case_name = mds.Identifier.ToString();
                            var ret_type = mds.ReturnType.ToString();
                            var kind = UnionCaseTypeKind.None;
                            var member_symbol = (IMethodSymbol)semantic_model.GetDeclaredSymbol(mds)!;
                            var tag_attr = member_symbol.GetAttributes().FirstOrDefault(a =>
                                a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionTagAttribute");
                            string? tag = null;
                            if (tag_attr != null)
                            {
                                tag = tag_attr.ConstructorArguments.First().Value?.ToString();
                            }
                            if (i == 0 && tag == null && ret_type != "void")
                            {
                                tag = "1";
                            }
                            var ret_type_symbol = member_symbol.ReturnType;
                            var is_generic = ret_type_symbol.IsNotInstGenericType();
                            if (is_generic) AnyGeneric = true;
                            if (ret_type_symbol.IsUnmanagedType) kind = UnionCaseTypeKind.Unmanaged;
                            else if (ret_type_symbol.IsReferenceType) kind = UnionCaseTypeKind.Class;
                            if (is_generic)
                            {
                                if (!ret_type_symbol.IsReferenceType) kind = UnionCaseTypeKind.None;
                            }
                            var symbol_attr = member_symbol.GetAttributes().FirstOrDefault(a =>
                                a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionSymbolAttribute");
                            if (symbol_attr == null)
                            {
                                symbol_attr = ret_type_symbol.GetAttributes().FirstOrDefault(a =>
                                    a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionSymbolAttribute");
                            }
                            if (symbol_attr != null)
                            {
                                var args = symbol_attr.NamedArguments.ToDictionary(a => a.Key, a => a.Value);
                                var symbol_attr_IsUnmanagedType = MayBool.None;
                                var symbol_attr_IsReferenceType = MayBool.None;
                                if (args.TryGetValue("IsUnmanagedType", out var _symbol_attr_IsUnmanagedType))
                                    symbol_attr_IsUnmanagedType = (MayBool)(byte)_symbol_attr_IsUnmanagedType.Value!;
                                else if (args.TryGetValue("IsReferenceType", out var _symbol_attr_IsReferenceType))
                                    symbol_attr_IsReferenceType = (MayBool)(byte)_symbol_attr_IsReferenceType.Value!;
                                if (symbol_attr_IsUnmanagedType is MayBool.True) kind = UnionCaseTypeKind.Unmanaged;
                                else if (symbol_attr_IsReferenceType is MayBool.True) kind = UnionCaseTypeKind.Class;
                                if (is_generic)
                                {
                                    if (symbol_attr_IsReferenceType is MayBool.False) kind = UnionCaseTypeKind.None;
                                }
                                if (symbol_attr_IsUnmanagedType is MayBool.False && kind is UnionCaseTypeKind.Unmanaged)
                                    kind = UnionCaseTypeKind.None;
                                if (symbol_attr_IsReferenceType is MayBool.False && kind is UnionCaseTypeKind.Class)
                                    kind = UnionCaseTypeKind.None;
                            }
                            if (symbol_attr == null)
                            {
                                if (ret_type_symbol is not ITypeParameterSymbol &&
                                    SymbolEqualityComparer.Default.Equals(
                                        ret_type_symbol.OriginalDefinition.ContainingAssembly,
                                        compilation.Assembly))
                                {
                                    var desc = Utils.MakeInfo(Id,
                                        Strings.Get("Generator.Union.Info.PossiblyInvalidSymbol"));
                                    diagnostics.Add(Diagnostic.Create(desc, member.GetLocation()));
                                }
                            }
                            cases.Add(new UnionCase(case_name, tag, ret_type, kind, is_generic));
                        }
                        else
                        {
                            var desc = Utils.MakeWarning(Id,
                                Strings.Get("Generator.Union.Error.IllegalTemplateMember"));
                            if (member is BaseTypeDeclarationSyntax bts)
                            {
                                diagnostics.Add(Diagnostic.Create(desc, bts.Identifier.GetLocation()));
                            }
                            else
                            {
                                diagnostics.Add(Diagnostic.Create(desc, member.GetLocation()));
                            }
                        }
                    }
                }
                var Name = syntax.Identifier.ToString();
                return (
                    Name, UnionAttr: union_attr, IsReadOnly, isClass: IsClass,
                    cases.ToImmutableArray(), AnyGeneric, GenMethods, GenBase,
                    AlwaysEq.Create(diagnostics)
                );
            }
        );

        context.RegisterSourceOutput(sources, static (ctx, input) =>
        {
            var (Name, UnionAttr, IsReadOnly, IsClass, Cases, AnyGeneric, GenMethods, GenBase, Diagnostics) = input;
            if (Diagnostics.Value.Count > 0)
            {
                foreach (var diagnostic in Diagnostics.Value)
                {
                    ctx.ReportDiagnostic(diagnostic);
                }
            }
            var code = new TemplateStructUnion(
                GenBase, Name, UnionAttr, IsReadOnly, IsClass, Cases, AnyGeneric, GenMethods
            ).Gen();
            var source_text = SourceText.From(code, Encoding.UTF8);
            var raw_source_file_name = GenBase.FileFullName;
            var sourceFileName = $"{raw_source_file_name}.union.g.cs";
            ctx.AddSource(sourceFileName, source_text);
        });
    }
}
