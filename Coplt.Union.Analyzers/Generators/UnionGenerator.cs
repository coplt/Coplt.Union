using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
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
    private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );
    public const string Id = "Union";

    private record struct Varying(
        string Name,
        Accessibility DeclaredAccessibility,
        UnionAttr UnionAttr,
        bool IsReadOnly,
        bool isClass,
        ImmutableArray<UnionCase> Cases,
        bool AnyGeneric,
        UnionGenerateMethod GenMethods,
        GenBase GenBase,
        bool SupportByRefFields,
        ImmutableArray<string> Generics,
        string Constraints,
        string NameSpace,
        string TypeFullName,
        string TypeNestedName,
        AlwaysEq<List<Diagnostic>> Diagnostics
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sources_1 = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Coplt.Union.UnionAttribute",
            static (syntax, _) => syntax is StructDeclarationSyntax or ClassDeclarationSyntax,
            Transform
        );
        var sources_2 = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Coplt.Union.Union2Attribute",
            static (syntax, _) => syntax is StructDeclarationSyntax or ClassDeclarationSyntax,
            Transform
        );

        context.RegisterSourceOutput(sources_1, Output);
        context.RegisterSourceOutput(sources_2, Output);
    }

    private static void Output(SourceProductionContext ctx, Varying input)
    {
        var (Name, Accessibility, UnionAttr, IsReadOnly, IsClass, Cases, AnyGeneric, GenMethods, GenBase, SupportByRefFields, Generics,
            Constraints, NameSpace, TypeFullName, TypeNestedName, Diagnostics) = input;
        if (Diagnostics.Value.Count > 0)
        {
            foreach (var diagnostic in Diagnostics.Value)
            {
                ctx.ReportDiagnostic(diagnostic);
            }
        }
        var code =
            new TemplateStructUnion(GenBase, Name, Accessibility, UnionAttr, IsReadOnly, IsClass, Cases, AnyGeneric, GenMethods, SupportByRefFields, Generics,
                    Constraints, NameSpace, TypeFullName, TypeNestedName)
                .Gen();
        var source_text = SourceText.From(code, Encoding.UTF8);
        var raw_source_file_name = GenBase.FileFullName;
        var v = UnionAttr.Union2 ? $"2" : "";
        var sourceFileName = $"{raw_source_file_name}.union{v}.g.cs";
        ctx.AddSource(sourceFileName, source_text);
    }

    private static Varying Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
    {
        var diagnostics = new List<Diagnostic>();
        var compilation = ctx.SemanticModel.Compilation;
        var attr = ctx.Attributes.First();
        var union_attr = UnionAttr.FromData(attr, diagnostics);
        var syntax = (TypeDeclarationSyntax)ctx.TargetNode;
        var semantic_model = ctx.SemanticModel;
        var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var GenBase = Utils.BuildGenBase(syntax, symbol, compilation);
        var SupportByRefFields = compilation.SupportsRuntimeCapability(RuntimeCapability.ByRefFields);
        var IsReadOnly = symbol.IsReadOnly;
        var IsClass = syntax is ClassDeclarationSyntax;
        var HasToString = symbol.GetMembers("ToString")
            .Where(s => s is IMethodSymbol)
            .Cast<IMethodSymbol>()
            .Any(s => s.TypeParameters.Length == 0 && s.Parameters.Length == 0);
        var GenToString = union_attr.GenerateToString ?? !HasToString;
        var GenEquals = union_attr.GenerateEquals;
        var GenCompareTo = union_attr.GenerateCompareTo;
        var GenMethods = new UnionGenerateMethod(GenToString, GenEquals, GenCompareTo);
        var Templates = syntax.Members.Where(static t => t is InterfaceDeclarationSyntax)
            .Cast<InterfaceDeclarationSyntax>()
            .Select(t =>
            {
                var symbol = semantic_model.GetDeclaredSymbol(t);
                return (t, symbol);
            })
            .Where(i =>
            {
                var (_, symbol) = i;
                return symbol!.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionTemplateAttribute");
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
        var variant_names = new HashSet<string>();
        foreach (var (template, _) in Templates)
        {
            foreach (var (member, i) in template.Members.Select((a, b) => (a, b)))
            {
                if (member is MethodDeclarationSyntax mds)
                {
                    var case_name = mds.Identifier.ToString();
                    if (!variant_names.Add(case_name))
                    {
                        var desc = Utils.MakeError(Id, Strings.Get("Generator.Union.Error.RecordOverloaded"));
                        if (member is BaseTypeDeclarationSyntax bts)
                        {
                            diagnostics.Add(Diagnostic.Create(desc, bts.Identifier.GetLocation()));
                        }
                        else
                        {
                            diagnostics.Add(Diagnostic.Create(desc, member.GetLocation()));
                        }
                        continue;
                    }
                    var member_symbol = (IMethodSymbol)semantic_model.GetDeclaredSymbol(mds)!;
                    var ret_type_symbol = member_symbol.ReturnType;
                    var is_void = ret_type_symbol.SpecialType == SpecialType.System_Void;
                    var variant_attr = member_symbol.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.Union.VariantAttribute")
                        ?.NamedArguments.ToDictionary(a => a.Key, a => a.Value);
                    string? tag = null;
                    if (variant_attr != null && variant_attr.TryGetValue("Tag", out var tag_const))
                    {
                        tag = tag_const.Value?.ToString();
                    }
                    if (member_symbol.TypeParameters.Length > 0)
                    {
                        var desc = Utils.MakeError(Id, Strings.Get("Generator.Union.Error.GenericVariant"));
                        diagnostics.Add(Diagnostic.Create(desc, member.GetLocation()));
                    }
                    var parameters = member_symbol.Parameters;
                    var is_record = parameters.Length > 0;
                    if (is_record && !is_void)
                    {
                        var desc = Utils.MakeWarning(Id, Strings.Get("Generator.Union.Warn.RecordHasReturn"));
                        diagnostics.Add(Diagnostic.Create(desc, mds.ReturnType.GetLocation()));
                    }
                    if (i == 0 && tag == null && (is_record || !is_void))
                    {
                        tag = "1";
                    }
                    if (!is_record)
                    {
                        var ret_type_name = ret_type_symbol.ToDisplayString(TypeDisplayFormat);
                        var is_generic = ret_type_symbol.IsNotInstGenericType();
                        if (is_generic) AnyGeneric = true;
                        var kind = UnionCaseTypeKind.None;
                        if (ret_type_symbol.IsUnmanagedType)
                            kind = UnionCaseTypeKind.Unmanaged;
                        else if (ret_type_symbol.IsReferenceType) kind = UnionCaseTypeKind.Class;
                        if (is_generic)
                        {
                            if (!ret_type_symbol.IsReferenceType) kind = UnionCaseTypeKind.None;
                        }
                        var symbol_attr = member_symbol.GetAttributes()
                            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionSymbolAttribute");
                        if (symbol_attr == null)
                        {
                            symbol_attr = ret_type_symbol.GetAttributes()
                                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionSymbolAttribute");
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
                            if (symbol_attr_IsUnmanagedType is MayBool.True)
                                kind = UnionCaseTypeKind.Unmanaged;
                            else if (symbol_attr_IsReferenceType is MayBool.True) kind = UnionCaseTypeKind.Class;
                            if (is_generic)
                            {
                                if (symbol_attr_IsReferenceType is MayBool.False) kind = UnionCaseTypeKind.None;
                            }
                            if (symbol_attr_IsUnmanagedType is MayBool.False && kind is UnionCaseTypeKind.Unmanaged) kind = UnionCaseTypeKind.None;
                            if (symbol_attr_IsReferenceType is MayBool.False && kind is UnionCaseTypeKind.Class) kind = UnionCaseTypeKind.None;
                        }
                        if (symbol_attr == null)
                        {
                            if (ret_type_symbol is not ITypeParameterSymbol &&
                                SymbolEqualityComparer.Default.Equals(ret_type_symbol.OriginalDefinition.ContainingAssembly, compilation.Assembly))
                            {
                                var desc = Utils.MakeInfo(Id, Strings.Get("Generator.Union.Info.PossiblyInvalidSymbol"));
                                diagnostics.Add(Diagnostic.Create(desc, member.GetLocation()));
                            }
                        }
                        cases.Add(new UnionCase(case_name, tag, ret_type_name, kind, is_generic, ImmutableArray<RecordItem>.Empty, default));
                    }
                    else
                    {
                        var items = ImmutableArray.CreateBuilder<RecordItem>();
                        foreach (var parameter in parameters)
                        {
                            if (parameter.RefKind != RefKind.None)
                            {
                                var desc = Utils.MakeError(Id, Strings.Get("Generator.Union.Error.ByRef"));
                                foreach (var location in parameter.Locations)
                                {
                                    diagnostics.Add(Diagnostic.Create(desc, location));
                                }
                            }
                            var type_symbol = parameter.Type;
                            var type_name = type_symbol.ToDisplayString(TypeDisplayFormat);
                            var arg_name = parameter.Name;
                            var is_generic = type_symbol.IsNotInstGenericType();
                            if (is_generic) AnyGeneric = true;
                            var kind = UnionCaseTypeKind.None;
                            if (type_symbol.IsUnmanagedType)
                                kind = UnionCaseTypeKind.Unmanaged;
                            else if (type_symbol.IsReferenceType) kind = UnionCaseTypeKind.Class;
                            if (is_generic)
                            {
                                if (!type_symbol.IsReferenceType) kind = UnionCaseTypeKind.None;
                            }
                            var symbol_attr = parameter.GetAttributes()
                                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionSymbolAttribute");
                            if (symbol_attr == null)
                            {
                                symbol_attr = type_symbol.GetAttributes()
                                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Coplt.Union.UnionSymbolAttribute");
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
                                if (symbol_attr_IsUnmanagedType is MayBool.True)
                                    kind = UnionCaseTypeKind.Unmanaged;
                                else if (symbol_attr_IsReferenceType is MayBool.True) kind = UnionCaseTypeKind.Class;
                                if (is_generic)
                                {
                                    if (symbol_attr_IsReferenceType is MayBool.False) kind = UnionCaseTypeKind.None;
                                }
                                if (symbol_attr_IsUnmanagedType is MayBool.False && kind is UnionCaseTypeKind.Unmanaged) kind = UnionCaseTypeKind.None;
                                if (symbol_attr_IsReferenceType is MayBool.False && kind is UnionCaseTypeKind.Class) kind = UnionCaseTypeKind.None;
                            }
                            if (symbol_attr == null)
                            {
                                if (type_symbol is not ITypeParameterSymbol &&
                                    SymbolEqualityComparer.Default.Equals(type_symbol.OriginalDefinition.ContainingAssembly, compilation.Assembly))
                                {
                                    var desc = Utils.MakeInfo(Id, Strings.Get("Generator.Union.Info.PossiblyInvalidSymbol"));
                                    foreach (var location in parameter.Locations)
                                    {
                                        diagnostics.Add(Diagnostic.Create(desc, location));
                                    }
                                }
                            }
                            var is_enum = type_symbol is { TypeKind: TypeKind.Enum };
                            var defv = parameter.HasExplicitDefaultValue
                                ? is_enum
                                    ? $"({type_name}){parameter.ExplicitDefaultValue}"
                                    : parameter.ExplicitDefaultValue switch
                                    {
                                        null => "default",
                                        bool v => v ? "true" : "false",
                                        string v => $"\"{v.Replace("\"", "\\\"")}\"",
                                        (byte or sbyte or short or ushort or int or uint or long or ulong or double) and var v => $"{v}",
                                        float v => $"{v}f",
                                        decimal v => $"{v}m",
                                        var v => $"({type_name}){v}",
                                    }
                                : null;
                            items.Add(new RecordItem(type_name, arg_name, kind, is_generic, defv));
                        }
                        var meta = new RecordMeta(union_attr.ViewName);
                        if (variant_attr != null)
                        {
                            if (variant_attr.TryGetValue("ViewName", out var ViewNameConst))
                            {
                                meta.ViewName = ViewNameConst.Value?.ToString()!;
                            }
                        }
                        cases.Add(new UnionCase(case_name, tag, "void", UnionCaseTypeKind.None, false, items.ToImmutableArray(), meta));
                    }
                }
                else
                {
                    var desc = Utils.MakeWarning(Id, Strings.Get("Generator.Union.Error.IllegalTemplateMember"));
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
        var generics = symbol.TypeParameters.Select(t => t.ToDisplayString(TypeDisplayFormat)).ToImmutableArray();
        var constraints = GetConstraints(symbol);
        var ns = symbol.ContainingNamespace.ToDisplayString(NameSpaceDisplayFormat);
        var type_full_name = symbol.ToDisplayString(TypeDisplayFormat);
        var type_nested_name = symbol.ToDisplayString(NestedDisplayFormat);
        return new(Name, symbol.DeclaredAccessibility, UnionAttr: union_attr, IsReadOnly, isClass: IsClass, cases.ToImmutableArray(), AnyGeneric, GenMethods,
            GenBase, SupportByRefFields, generics, constraints, ns, type_full_name, type_nested_name, AlwaysEq.Create(diagnostics));
    }

    private static string GetConstraints(INamedTypeSymbol symbol)
    {
        var a = symbol.ToDisplayString(ConstraintDisplayFormat);
        var first_where = a.IndexOf("where", StringComparison.Ordinal);
        if (first_where < 0) return "";
        return a.Substring(first_where);
    }

    private static readonly SymbolDisplayFormat ConstraintDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    private static readonly SymbolDisplayFormat NameSpaceDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
    );

    private static readonly SymbolDisplayFormat NestedDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes
    );
}
