﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Coplt.Union.Analyzers.Utilities;

internal static class Utils
{
    public static void GetUsings(SyntaxNode? node, HashSet<string> usings)
    {
        for (;;)
        {
            if (node == null) break;
            if (node is CompilationUnitSyntax cus)
            {
                foreach (var use in cus.Usings)
                {
                    usings.Add(use.ToString());
                }
                return;
            }
            node = node.Parent;
        }
    }

    public static string GetAccessStr(this Accessibility self) => self switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Internal => "internal",
        Accessibility.Private => "private",
        _ => "",
    };

    public static NameWrap WrapName(this INamedTypeSymbol symbol)
    {
        var access = symbol.DeclaredAccessibility.GetAccessStr();
        var type_decl = symbol switch
        {
            { IsValueType: true, IsRecord: true, IsReadOnly: false } => "partial record struct",
            { IsValueType: true, IsRecord: true, IsReadOnly: true } => "readonly partial record struct",
            { IsValueType: true, IsRecord: false, IsReadOnly: true, IsRefLikeType: false } => "readonly partial struct",
            { IsValueType: true, IsRecord: false, IsReadOnly: false, IsRefLikeType: true } => "ref partial struct",
            { IsValueType: true, IsRecord: false, IsReadOnly: true, IsRefLikeType: true } =>
                "readonly ref partial struct",
            { IsValueType: true, IsRecord: false, IsReadOnly: false, IsRefLikeType: false } => "partial struct",
            { IsValueType: false, IsRecord: true, IsAbstract: false } => "partial record",
            { IsValueType: false, IsRecord: true, IsAbstract: true } => "abstract partial record",
            { IsValueType: false, IsStatic: true } => "static partial class",
            { IsValueType: false, IsAbstract: true, } => "abstract partial class",
            _ => "partial class",
        };
        var generic = string.Empty;
        if (symbol.IsGenericType)
        {
            var ps = new List<string>();
            foreach (var tp in symbol.TypeParameters)
            {
                var variance = tp.Variance switch
                {
                    VarianceKind.Out => "out ",
                    VarianceKind.In => "in ",
                    _ => "",
                };
                ps.Add($"{variance}{tp.ToDisplayString()}");
            }
            generic = $"<{string.Join(", ", ps)}>";
        }
        return new NameWrap($"{access} {type_decl} {symbol.Name}{generic}");
    }

    public static ImmutableList<NameWrap>? WrapNames(this INamedTypeSymbol symbol,
        ImmutableList<NameWrap>? childs = null)
    {
        NameWrap wrap;
        var parent = symbol.ContainingType;
        if (parent == null)
        {
            var ns = symbol.ContainingNamespace;
            if (ns == null || ns.IsGlobalNamespace) return childs;
            wrap = new NameWrap($"namespace {ns}");
            return childs?.Insert(0, wrap) ?? ImmutableList.Create(wrap);
        }
        wrap = parent.WrapName();
        return WrapNames(parent, childs?.Insert(0, wrap) ?? ImmutableList.Create(wrap));
    }

    public static DiagnosticDescriptor MakeError(LocalizableString msg)
        => new("EntityUniverse", msg, msg, "", DiagnosticSeverity.Error, true);

    public static DiagnosticDescriptor MakeWarning(LocalizableString msg)
        => new("EntityUniverse", msg, msg, "", DiagnosticSeverity.Warning, true);
    
    public static DiagnosticDescriptor MakeInfo(LocalizableString msg)
        => new("EntityUniverse", msg, msg, "", DiagnosticSeverity.Info, true);

    public static bool IsNotInstGenericType(this ITypeSymbol type) =>
        type is ITypeParameterSymbol
        || (type is INamedTypeSymbol { IsGenericType: true, TypeArguments: var typeArguments }
            && typeArguments.Any(IsNotInstGenericType))
        || (type is IArrayTypeSymbol { ElementType: var e } && e.IsNotInstGenericType())
        || (type is IPointerTypeSymbol { PointedAtType: var p } && p.IsNotInstGenericType());
}
