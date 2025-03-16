using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Coplt.Analyzers.Generators.Templates;
using Coplt.Union.Analyzers.Resources;
using Coplt.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ATemplate = Coplt.Analyzers.Generators.Templates.ATemplate;

namespace Coplt.Union.Analyzers.Generators.Templates;

public enum UnionCaseTypeKind
{
    None,
    Unmanaged,
    Class
}

public record struct RecordMeta(string ViewName);

public record struct RecordItem(
    string Type,
    string Name,
    UnionCaseTypeKind Kind,
    bool IsGeneric,
    string? DefaultValue
);

public record struct UnionCase(
    string Name,
    string? Tag,
    string Type,
    UnionCaseTypeKind Kind,
    bool IsGeneric,
    ImmutableArray<RecordItem> Items,
    RecordMeta Meta
)
{
    public bool IsRecord => Items.Length > 0;
}

public record struct UnionAttr(
    string TagsName,
    bool ExternalTags,
    string ExternalTagsName,
    string? TagsUnderlying,
    bool? GenerateToString,
    bool GenerateEquals,
    bool GenerateCompareTo,
    string ViewName
)
{
    public static UnionAttr FromData(AttributeData data, List<Diagnostic> diagnostics)
    {
        var a = new UnionAttr(
            "Tags", false, "{0}Tags", null,
            null, true, true, "Variant{0}"
        );
        foreach (var kv in data.NamedArguments)
        {
            switch (kv.Key)
            {
                case "TagsName":
                    a.TagsName = (string)kv.Value.Value!;
                    break;
                case "ExternalTags":
                    a.ExternalTags = (bool)kv.Value.Value!;
                    break;
                case "ExternalTagsName":
                    a.ExternalTagsName = (string)kv.Value.Value!;
                    break;
                case "TagsUnderlying":
                    a.TagsUnderlying = kv.Value.Value?.ToString();
                    if (a.TagsUnderlying is not ("sbyte" or "byte" or "short" or "ushort" or "int" or "uint" or "long"
                        or "ulong"))
                    {
                        a.TagsUnderlying = null;
                        var desc = Utils.MakeError(UnionGenerator.Id, Strings.Get("Generator.Union.Error.Underlying"));
                        var syntax = (AttributeSyntax)data.ApplicationSyntaxReference!.GetSyntax();
                        try
                        {
                            var expr = syntax.ArgumentList!.Arguments
                                .Where(static a => a.NameEquals?.Name.ToString() == "TagsUnderlying")
                                .Select(static a => a.Expression)
                                .First();
                            diagnostics.Add(Diagnostic.Create(desc, expr.GetLocation()));
                        }
                        catch
                        {
                            diagnostics.Add(Diagnostic.Create(desc, syntax.GetLocation()));
                        }
                    }
                    break;
                case "GenerateToString":
                    a.GenerateToString = (bool)kv.Value.Value!;
                    break;
                case "GenerateEquals":
                    a.GenerateEquals = (bool)kv.Value.Value!;
                    break;
                case "GenerateCompareTo":
                    a.GenerateCompareTo = (bool)kv.Value.Value!;
                    break;
                case "ViewName":
                    a.ViewName = (string)kv.Value.Value!;
                    break;
            }
        }
        return a;
    }
}

public record struct UnionGenerateMethod(bool GenToString, bool GenEquals, bool GenCompareTo);

public class TemplateStructUnion(
    GenBase GenBase,
    string Name,
    Accessibility Accessibility,
    UnionAttr Attr,
    bool ReadOnly,
    bool IsClass,
    ImmutableArray<UnionCase> Cases,
    bool AnyGeneric,
    UnionGenerateMethod GenMethods,
    bool SupportByRefFields
) : ATemplate(GenBase)
{
    public const string AggressiveInlining =
        "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

    public const string CompilerGenerated = "[global::System.Runtime.CompilerServices.CompilerGenerated]";
    public const string LayoutAuto =
        "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]";
    public const string LayoutExplicit =
        "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
    public const string FieldOffset0 = "[global::System.Runtime.InteropServices.FieldOffset(0)]";

    private List<StringBuilder> ExTypes = new();

    private string HashName = "";

    private string @readonly => IsClass ? string.Empty : " readonly";

    private string Self = null!;

    private string RoSelf = null!;
    private string RoImpl = null!;

    protected override void DoGen()
    {
        var impl_name = $"__impl_";

        Self = IsClass || ReadOnly
            ? "this"
            : $"global::System.Runtime.CompilerServices.Unsafe.AsRef<{TypeName}>(in this)";
        RoSelf = IsClass || !ReadOnly
            ? "this"
            : $"global::System.Runtime.CompilerServices.Unsafe.AsRef<{TypeName}>(in this)";
        RoImpl = IsClass || !ReadOnly
            ? "this._impl"
            : $"global::System.Runtime.CompilerServices.Unsafe.AsRef<{TypeName}.{impl_name}>(in this._impl)";

        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(MemoryMarshal.AsBytes(FullName.AsSpan()).ToArray());
            HashName = string.Join("", bytes.Select(b => $"{b:X2}"));
            HashName = $"{Name}_{HashName}";
        }

        var tags_name = Attr.ExternalTags ? string.Format(Attr.ExternalTagsName, Name) : Attr.TagsName;

        if (Attr.ExternalTags)
        {
            GenTags(tags_name, "");
            sb.AppendLine();
        }

        BuildTypes();

        sb.AppendLine(GenBase.Target.Code);
        sb.AppendLine($"    : global::Coplt.Union.ITaggedUnion");
        if (GenMethods.GenEquals)
            sb.AppendLine($"    , global::System.IEquatable<{TypeName}>");
        if (GenMethods.GenCompareTo)
            sb.AppendLine($"    , global::System.IComparable<{TypeName}>");
        if (GenMethods.GenEquals || GenMethods.GenCompareTo)
        {
            sb.AppendLine($"    #if NET7_0_OR_GREATER");
            if (GenMethods.GenEquals)
                sb.AppendLine($"    , global::System.Numerics.IEqualityOperators<{TypeName}, {TypeName}, bool>");
            if (GenMethods.GenCompareTo)
                sb.AppendLine($"    , global::System.Numerics.IComparisonOperators<{TypeName}, {TypeName}, bool>");
            sb.AppendLine($"    #endif");
        }
        sb.AppendLine("{");

        sb.AppendLine($"    #region Fields");
        sb.AppendLine();
        sb.AppendLine(ReadOnly ? $"    private{@readonly} {impl_name} _impl;" : $"    private {impl_name} _impl;");
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Fields");
        sb.AppendLine();
        sb.AppendLine($"    #region Ctor");
        sb.AppendLine();
        sb.AppendLine($"    private {Name}({impl_name} _impl) {{ this._impl = _impl; }}");
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Ctor");

        sb.AppendLine();
        sb.AppendLine($"    #region Tag Getter");
        sb.AppendLine();
        sb.AppendLine($"    public{@readonly} {tags_name} Tag");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        {AggressiveInlining}");
        sb.AppendLine($"        get => this._impl._tag;");
        sb.AppendLine($"    }}");
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Tag Getter");

        if (!Attr.ExternalTags)
        {
            sb.AppendLine();
            GenTags(tags_name);
        }

        GenImpl(impl_name, tags_name);

        if (SupportByRefFields) GenRefViews(impl_name, tags_name);
        else GenValueViews(impl_name, tags_name);

        if (Cases.Length > 0)
        {
            GenMake(impl_name, tags_name);

            GenIs(tags_name);

            GenGetter(impl_name);
        }
        else
        {
            GenMakeEmpty();
        }

        sb.AppendLine();
        GenEquatable(tags_name);

        sb.AppendLine();
        GenComparable(tags_name);

        sb.AppendLine();
        GenToStr(tags_name);

        sb.AppendLine("}");

        foreach (var ex in ExTypes)
        {
            sb.AppendLine();
            sb.Append(ex);
        }
    }

    private record struct RecordTypeDefine(
        string UnmanagedTypeName,
        List<(string Type, int Index)> UnmanagedFields,
        List<(string Type, int Index, int NthByKind)> ClassFields,
        List<(string Type, int Index, int NthByKind)> OtherFields
    );

    private int UnmanagedTypeInc = 0;
    private int OtherTypeInc = 0;
    private int MaxClassTypes = 0;
    private readonly Dictionary<string, int> UnmanagedTypes = new();
    private readonly Dictionary<string, (int Index, int Max)> OtherTypes = new();
    private readonly Dictionary<int, RecordTypeDefine> RecordTypeDefines = new();

    private void BuildTypes()
    {
        foreach (var (@case, i) in Cases.Select(static (a, b) => (a, b)))
        {
            if (!@case.IsRecord)
            {
                if (@case.Type == "void") continue;
                switch (@case.Kind)
                {
                    case UnionCaseTypeKind.Unmanaged:
                        if (!UnmanagedTypes.ContainsKey(@case.Type)) UnmanagedTypes.Add(@case.Type, UnmanagedTypeInc++);
                        break;
                    case UnionCaseTypeKind.Class:
                        MaxClassTypes = Math.Max(MaxClassTypes, 1);
                        break;
                    default:
                        if (!OtherTypes.TryGetValue(@case.Type, out var ot))
                            ot = (OtherTypeInc++, 1);
                        OtherTypes[@case.Type] = ot;
                        break;
                }
                continue;
            }
            else
            {
                var unmanaged_struct_name = AnyGeneric
                    ? $"ℍⅈⅆⅇ__{HashName}__record_{i}_unmanaged_"
                    : $"__record_{i}_unmanaged_";
                var def = new RecordTypeDefine(unmanaged_struct_name, new(), new(), new());
                var class_inc = 0;
                var other_type_max_inc = new Dictionary<string, object>();
                foreach (var (item, j) in @case.Items.Select(static (a, b) => (a, b)))
                {
                    switch (item.Kind)
                    {
                        case UnionCaseTypeKind.Unmanaged:
                            def.UnmanagedFields.Add((item.Type, j));
                            break;
                        case UnionCaseTypeKind.Class:
                        {
                            var nth = class_inc++;
                            def.ClassFields.Add((item.Type, j, nth));
                            MaxClassTypes = Math.Max(MaxClassTypes, nth + 1);
                            break;
                        }
                        default: goto OtherType;
                    }
                    continue;
                    OtherType:
                    if (!other_type_max_inc.TryGetValue(item.Type, out var nth_boxed))
                    {
                        nth_boxed = 0;
                        other_type_max_inc.Add(item.Type, nth_boxed);
                    }
                    {
                        var nth = Unsafe.Unbox<int>(nth_boxed)++;
                        def.OtherFields.Add((item.Type, j, nth));
                        if (!OtherTypes.TryGetValue(item.Type, out var ot))
                            ot = (OtherTypeInc++, 1);
                        else ot.Max = Math.Max(ot.Max, nth + 1);
                        OtherTypes[item.Type] = ot;
                    }
                    continue;
                }
                if (def.UnmanagedFields.Count > 0)
                {
                    UnmanagedTypes.Add(unmanaged_struct_name, UnmanagedTypeInc++);
                }
                RecordTypeDefines.Add(i, def);
            }
        }
    }

    private void GenTags(string name, string spaces = "    ")
    {
        sb.AppendLine($"    #region Tags");
        sb.AppendLine();
        var type = Attr.TagsUnderlying;
        if (type == null)
        {
            if (Cases.Length < byte.MaxValue) type = "byte";
            else if (Cases.Length < short.MaxValue) type = "short";
            else type = "int";
        }
        sb.AppendLine($"{spaces}public enum {name} : {type}");
        sb.AppendLine($"{spaces}{{");
        foreach (var @case in Cases)
        {
            sb.Append($"{spaces}    {@case.Name}");
            if (@case.Tag != null) sb.Append($" = {@case.Tag}");
            sb.AppendLine(",");
        }
        sb.AppendLine($"{spaces}}}");
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Tags");
    }

    private void GenValueViews(string impl_name, string tags_name)
    {
        var ro = ReadOnly ? " readonly" : "";

        var first = true;
        foreach (var (@case, i) in Cases.Select(static (a, b) => (a, b)))
        {
            if (!@case.IsRecord) continue;
            if (first)
            {
                sb.AppendLine();
                sb.AppendLine($"    #region Views");
                sb.AppendLine();
                first = false;
            }
            var meta = @case.Meta;
            var space = "        ";
            var view_name = string.Format(meta.ViewName, @case.Name);
            sb.AppendLine($"    public{ro} struct {view_name} : ");
            sb.AppendLine($"{space}global::System.IEquatable<{view_name}>");
            sb.AppendLine($"{space}, global::System.IComparable<{view_name}>");
            sb.AppendLine($"{space}#if NET7_0_OR_GREATER");
            sb.AppendLine($"{space}, global::System.Numerics.IEqualityOperators<{view_name}, {view_name}, bool>");
            sb.AppendLine($"{space}, global::System.Numerics.IComparisonOperators<{view_name}, {view_name}, bool>");
            sb.AppendLine($"{space}#endif");
            sb.AppendLine($"    {{");
            sb.AppendLine($"{space}private{ro} {impl_name} _impl;");

            #region Ctor

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public {view_name}()");
            sb.AppendLine($"{space}{{");
            sb.AppendLine($"{space}    _impl = new {impl_name}({tags_name}.{@case.Name});");
            sb.AppendLine($"{space}}}");

            #endregion

            #region Getter

            if (@case.Items.Length > 0) sb.AppendLine();
            var def = RecordTypeDefines[i];
            if (def.UnmanagedFields.Count > 0)
            {
                var uti = UnmanagedTypes[def.UnmanagedTypeName];
                foreach (var (type, index) in def.UnmanagedFields)
                {
                    var item = @case.Items[index];
                    sb.AppendLine($"{space}[global::System.Diagnostics.CodeAnalysis.UnscopedRef]");
                    sb.AppendLine($"{space}public ref{ro} {type} {item.Name}");
                    sb.AppendLine($"{space}{{");
                    sb.AppendLine($"{space}    {AggressiveInlining}");
                    sb.AppendLine($"{space}    get => ref {RoImpl}._u._{uti}._{index};");
                    sb.AppendLine($"{space}}}");
                }
            }
            foreach (var (type, index, nth) in def.ClassFields)
            {
                var item = @case.Items[index];
                sb.AppendLine($"{space}[global::System.Diagnostics.CodeAnalysis.UnscopedRef]");
                sb.AppendLine($"{space}public ref{ro} {type} {item.Name}");
                sb.AppendLine($"{space}{{");
                sb.AppendLine($"{space}    {AggressiveInlining}");
                sb.AppendLine(
                    $"{space}    get => ref global::System.Runtime.CompilerServices.Unsafe.As<object?, {type}>(ref {RoImpl}._c{nth});");
                sb.AppendLine($"{space}}}");
            }
            foreach (var (type, index, nth) in def.OtherFields)
            {
                var (ti, _) = OtherTypes[type];
                var item = @case.Items[index];
                sb.AppendLine($"{space}[global::System.Diagnostics.CodeAnalysis.UnscopedRef]");
                sb.AppendLine($"{space}public ref{ro} {type} {item.Name}");
                sb.AppendLine($"{space}{{");
                sb.AppendLine($"{space}    {AggressiveInlining}");
                sb.AppendLine($"{space}    get => ref {RoImpl}._f{ti}_{nth};");
                sb.AppendLine($"{space}}}");
            }

            #endregion

            #region Equatable

            #region Equals

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.Append($"{space}public bool Equals({view_name} other) =>\n{space}    ");
            {
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append($" &&\n{space}    ");
                    sb.Append(
                        $"global::System.Collections.Generic.EqualityComparer<{item.Type}>.Default.Equals({item.Name}, other.{item.Name})");
                }
            }
            sb.AppendLine($";");

            #endregion

            #region HashCode

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            if (@case.Items.Length < 8)
            {
                sb.Append($"{space}public override int GetHashCode() => global::System.HashCode.Combine(");
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append($", ");
                    sb.Append($"{item.Name}");
                }
                sb.AppendLine($");");
            }
            else
            {
                sb.AppendLine($"{space}public override int GetHashCode()");
                sb.AppendLine($"{space}{{");
                sb.AppendLine($"{space}    var hasher = new global::System.HashCode();");
                foreach (var item in @case.Items)
                {
                    sb.AppendLine($"{space}    hasher.Add({item.Name});");
                }
                sb.AppendLine($"{space}    return hasher.ToHashCode();");
                sb.AppendLine($"{space}}}");
            }

            #endregion

            #region other

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public override bool Equals(object? obj) => obj is {view_name} other && Equals(other);");

            sb.AppendLine();

            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator ==({view_name} left, {view_name} right) => left.Equals(right);");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator !=({view_name} left, {view_name} right) => !left.Equals(right);");

            #endregion

            #endregion

            #region Comparable

            #region CompareTo

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine($"{space}public int CompareTo({view_name} other)");
            sb.AppendLine($"{space}{{");
            {
                for (var n = 0; n < @case.Items.Length; n++)
                {
                    var item = @case.Items[n];
                    sb.AppendLine(
                        $"{space}    var _{n} = global::System.Collections.Generic.Comparer<{item.Type}>.Default.Compare({item.Name}, other.{item.Name});");

                    sb.AppendLine($"{space}    if (_{n} != 0) return _{n};");
                }
            }
            sb.AppendLine($"{space}    return 0;");
            sb.AppendLine($"{space}}}");

            #endregion

            #region other

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator <({view_name} left, {view_name} right) => left.CompareTo(right) < 0;");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator >({view_name} left, {view_name} right) => left.CompareTo(right) > 0;");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator <=({view_name} left, {view_name} right) => left.CompareTo(right) <= 0;");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator >=({view_name} left, {view_name} right) => left.CompareTo(right) >= 0;");

            #endregion

            #endregion

            #region ToString

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.Append(
                $"{space}public override string ToString() => $\"{{nameof({TypeName})}}.{{nameof({tags_name}.{@case.Name})}} {{{{ ");
            {
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append(", ");
                    sb.Append($"{item.Name} = {{{item.Name}}}");
                }
            }
            sb.AppendLine(" }}\";");

            #endregion

            #region Deconstruct

            if (@case.Items.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"{space}{AggressiveInlining}");
                sb.Append($"{space}public void Deconstruct(");
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append(", ");
                    sb.Append($"out {item.Type} {item.Name}");
                }
                sb.AppendLine($")");
                sb.AppendLine($"{space}{{");
                foreach (var item in @case.Items)
                {
                    sb.AppendLine($"{space}    {item.Name} = this.{item.Name};");
                }
                sb.AppendLine($"{space}}}");
            }

            #endregion

            sb.AppendLine($"    }}");
        }

        if (!first)
        {
            sb.AppendLine();
            sb.AppendLine($"    #endregion // Views");
        }
    }

    private void GenRefViews(string impl_name, string tags_name)
    {
        var ro = ReadOnly ? " readonly" : "";

        var first = true;
        foreach (var (@case, i) in Cases.Select(static (a, b) => (a, b)))
        {
            if (!@case.IsRecord) continue;
            if (first)
            {
                sb.AppendLine();
                sb.AppendLine($"    #region Views");
                sb.AppendLine();
                first = false;
            }
            var meta = @case.Meta;
            var space = "        ";
            var view_name = string.Format(meta.ViewName, @case.Name);
            sb.AppendLine($"    public readonly ref struct {view_name}");
            sb.AppendLine($"{space}#if NET9_0_OR_GREATER");
            sb.AppendLine($"{space}: global::System.IEquatable<{view_name}>");
            sb.AppendLine($"{space}, global::System.IComparable<{view_name}>");
            // sb.AppendLine($"{space}, global::System.Numerics.IEqualityOperators<{view_name}, {view_name}, bool>");
            // sb.AppendLine($"{space}, global::System.Numerics.IComparisonOperators<{view_name}, {view_name}, bool>");
            sb.AppendLine($"{space}#endif");
            sb.AppendLine($"    {{");
            sb.AppendLine($"{space}#region Fields");
            sb.AppendLine();
            sb.AppendLine($"{space}private readonly ref{ro} {impl_name} _impl;");
            sb.AppendLine();
            sb.AppendLine($"{space}#endregion // Fields");

            #region Ctor

            sb.AppendLine();
            sb.AppendLine($"{space}#region Ctor");
            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}internal {view_name}(ref{ro} {impl_name} impl)");
            sb.AppendLine($"{space}{{");
            sb.AppendLine($"{space}    _impl = ref impl;");
            sb.AppendLine($"{space}}}");
            sb.AppendLine();
            sb.AppendLine($"{space}#endregion // Ctor");

            #endregion

            #region Getter

            sb.AppendLine();
            sb.AppendLine($"{space}#region Getter");
            sb.AppendLine();
            var def = RecordTypeDefines[i];
            if (def.UnmanagedFields.Count > 0)
            {
                var uti = UnmanagedTypes[def.UnmanagedTypeName];
                foreach (var (type, index) in def.UnmanagedFields)
                {
                    var item = @case.Items[index];
                    sb.AppendLine($"{space}public ref{ro} {type} {item.Name}");
                    sb.AppendLine($"{space}{{");
                    sb.AppendLine($"{space}    {AggressiveInlining}");
                    sb.AppendLine($"{space}    get => ref {RoImpl}._u._{uti}._{index};");
                    sb.AppendLine($"{space}}}");
                }
            }
            foreach (var (type, index, nth) in def.ClassFields)
            {
                var item = @case.Items[index];
                sb.AppendLine($"{space}public ref{ro} {type} {item.Name}");
                sb.AppendLine($"{space}{{");
                sb.AppendLine($"{space}    {AggressiveInlining}");
                sb.AppendLine(
                    $"{space}    get => ref global::System.Runtime.CompilerServices.Unsafe.As<object?, {type}>(ref {RoImpl}._c{nth});");
                sb.AppendLine($"{space}}}");
            }
            foreach (var (type, index, nth) in def.OtherFields)
            {
                var (ti, _) = OtherTypes[type];
                var item = @case.Items[index];
                sb.AppendLine($"{space}public ref{ro} {type} {item.Name}");
                sb.AppendLine($"{space}{{");
                sb.AppendLine($"{space}    {AggressiveInlining}");
                sb.AppendLine($"{space}    get => ref {RoImpl}._f{ti}_{nth};");
                sb.AppendLine($"{space}}}");
            }
            sb.AppendLine();
            sb.AppendLine($"{space}#endregion // Getter");

            #endregion

            #region Equatable

            sb.AppendLine();
            sb.AppendLine($"{space}#region Equals");

            #region Equals

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.Append($"{space}public bool Equals(scoped {view_name} other) =>\n{space}    ");
            {
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append($" &&\n{space}    ");
                    sb.Append(
                        $"global::System.Collections.Generic.EqualityComparer<{item.Type}>.Default.Equals({item.Name}, other.{item.Name})");
                }
            }
            sb.AppendLine($";");

            #endregion

            #region HashCode

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            if (@case.Items.Length < 8)
            {
                sb.Append($"{space}public override int GetHashCode() => global::System.HashCode.Combine(");
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append($", ");
                    sb.Append($"{item.Name}");
                }
                sb.AppendLine($");");
            }
            else
            {
                sb.AppendLine($"{space}public override int GetHashCode()");
                sb.AppendLine($"{space}{{");
                sb.AppendLine($"{space}    var hasher = new global::System.HashCode();");
                foreach (var item in @case.Items)
                {
                    sb.AppendLine($"{space}    hasher.Add({item.Name});");
                }
                sb.AppendLine($"{space}    return hasher.ToHashCode();");
                sb.AppendLine($"{space}}}");
            }

            #endregion

            #region other

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public override bool Equals(object? obj) => false;");

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator ==(scoped {view_name} left, scoped {view_name} right) => left.Equals(right);");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator !=(scoped {view_name} left, scoped {view_name} right) => !left.Equals(right);");

            #endregion

            sb.AppendLine();
            sb.AppendLine($"{space}#endregion // Equals");

            #endregion

            #region Comparable

            sb.AppendLine();
            sb.AppendLine($"{space}#region CompareTo");

            #region CompareTo

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine($"{space}public int CompareTo(scoped {view_name} other)");
            sb.AppendLine($"{space}{{");
            {
                for (var n = 0; n < @case.Items.Length; n++)
                {
                    var item = @case.Items[n];
                    sb.AppendLine(
                        $"{space}    var _{n} = global::System.Collections.Generic.Comparer<{item.Type}>.Default.Compare({item.Name}, other.{item.Name});");

                    sb.AppendLine($"{space}    if (_{n} != 0) return _{n};");
                }
            }
            sb.AppendLine($"{space}    return 0;");
            sb.AppendLine($"{space}}}");

            #endregion

            #region other

            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator <(scoped {view_name} left, scoped {view_name} right) => left.CompareTo(right) < 0;");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator >(scoped {view_name} left, scoped {view_name} right) => left.CompareTo(right) > 0;");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator <=(scoped {view_name} left, scoped {view_name} right) => left.CompareTo(right) <= 0;");
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.AppendLine(
                $"{space}public static bool operator >=(scoped {view_name} left, scoped {view_name} right) => left.CompareTo(right) >= 0;");

            #endregion

            sb.AppendLine();
            sb.AppendLine($"{space}#endregion // CompareTo");

            #endregion

            #region ToString

            sb.AppendLine();
            sb.AppendLine($"{space}#region ToString");
            sb.AppendLine();
            sb.AppendLine($"{space}{AggressiveInlining}");
            sb.Append(
                $"{space}public override string ToString() => $\"{{nameof({TypeName})}}.{{nameof({tags_name}.{@case.Name})}} {{{{ ");
            {
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append(", ");
                    sb.Append($"{item.Name} = {{{item.Name}}}");
                }
            }
            sb.AppendLine(" }}\";");

            sb.AppendLine();
            sb.AppendLine($"{space}#endregion // ToString");

            #endregion

            #region Deconstruct

            {
                sb.AppendLine();
                sb.AppendLine($"{space}#region Deconstruct");
                sb.AppendLine();
                sb.AppendLine($"{space}{AggressiveInlining}");
                sb.Append($"{space}public void Deconstruct(");
                var _first = true;
                foreach (var item in @case.Items)
                {
                    if (_first) _first = false;
                    else sb.Append(", ");
                    sb.Append($"out {item.Type} {item.Name}");
                }
                sb.AppendLine($")");
                sb.AppendLine($"{space}{{");
                foreach (var item in @case.Items)
                {
                    sb.AppendLine($"{space}    {item.Name} = this.{item.Name};");
                }
                sb.AppendLine($"{space}}}");
                sb.AppendLine();
                sb.AppendLine($"{space}#endregion // Deconstruct");
            }

            #endregion

            sb.AppendLine($"    }}");
        }

        if (!first)
        {
            sb.AppendLine();
            sb.AppendLine($"    #endregion // Views");
        }
    }

    private void GenImpl(string name, string tags_name)
    {
        sb.AppendLine();
        sb.AppendLine($"    #region Impl");
        sb.AppendLine();
        sb.AppendLine($"    {CompilerGenerated}");
        sb.AppendLine($"    {LayoutAuto}");
        sb.AppendLine($"    internal struct {name}");
        sb.AppendLine($"    {{");

        var __unmanaged_ = AnyGeneric ? $"ℍⅈⅆⅇ__{HashName}__unmanaged_" : "__unmanaged_";

        #region Class Fields

        for (var i = 0; i < MaxClassTypes; i++)
        {
            sb.AppendLine($"        public object? _c{i};");
        }

        #endregion

        #region Unmanaged Fields

        if (UnmanagedTypes.Count > 0)
        {
            sb.AppendLine($"        public {__unmanaged_} _u;");
        }

        #endregion

        #region Other Fields

        foreach (var kv in OtherTypes)
        {
            for (var i = 0; i < kv.Value.Max; i++)
            {
                sb.AppendLine($"        public {kv.Key} _f{kv.Value.Index}_{i};");
            }
        }

        #endregion

        #region Tag Field

        sb.AppendLine($"        public{@readonly} {tags_name} _tag;");

        #endregion

        #region Ctor

        sb.AppendLine();
        sb.AppendLine($"        {AggressiveInlining}");
        sb.AppendLine($"        public {name}({tags_name} _tag)");
        sb.AppendLine($"        {{");
        for (var i = 0; i < MaxClassTypes; i++)
        {
            sb.AppendLine($"            this._c{i} = null;");
        }
        if (UnmanagedTypes.Count > 0)
            sb.AppendLine($"            global::System.Runtime.CompilerServices.Unsafe.SkipInit(out this._u);");
        foreach (var kv in OtherTypes)
        {
            for (var i = 0; i < kv.Value.Max; i++)
            {
                sb.AppendLine($"            this._f{kv.Value.Index}_{i} = default!;");
            }
        }
        sb.AppendLine($"            this._tag = _tag;");
        sb.AppendLine($"        }}");

        #endregion

        #region Unmanaged Struct

        if (UnmanagedTypes.Count > 0)
        {
            var ex_sb = AnyGeneric ? new StringBuilder() : sb;
            var space = AnyGeneric ? "" : "        ";
            ex_sb.AppendLine();
            ex_sb.AppendLine($"{space}{CompilerGenerated}");
            ex_sb.AppendLine($"{space}{LayoutExplicit}");
            ex_sb.AppendLine($"{space}internal struct {__unmanaged_}");
            ex_sb.AppendLine($"{space}{{");
            foreach (var kv in UnmanagedTypes)
            {
                ex_sb.AppendLine($"{space}    {FieldOffset0}");
                ex_sb.AppendLine($"{space}    public {kv.Key} _{kv.Value};");
            }
            ex_sb.AppendLine($"{space}}}");
            if (AnyGeneric) ExTypes.Add(ex_sb);
        }

        #endregion

        #region Records

        foreach (var kv_define in RecordTypeDefines)
        {
            if (kv_define.Value.UnmanagedFields.Count > 0)
            {
                var ex_sb = AnyGeneric ? new StringBuilder() : sb;
                var space = AnyGeneric ? "" : "        ";
                ex_sb.AppendLine();
                ex_sb.AppendLine($"{space}{CompilerGenerated}");
                ex_sb.AppendLine($"{space}{LayoutAuto}");
                ex_sb.AppendLine($"{space}internal struct {kv_define.Value.UnmanagedTypeName}");
                ex_sb.AppendLine($"{space}{{");
                foreach (var (type, index) in kv_define.Value.UnmanagedFields)
                {
                    ex_sb.AppendLine($"{space}    public {type} _{index};");
                }
                ex_sb.AppendLine($"{space}}}");
                if (AnyGeneric) ExTypes.Add(ex_sb);
            }
        }

        #endregion

        sb.AppendLine($"    }}");
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Impl");
    }

    private void GenMakeEmpty()
    {
        sb.AppendLine();
        sb.AppendLine($"    #region Make");
        sb.AppendLine();
        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine($"    public static {TypeName} Make() => new(default(__impl_)!);");
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Make");
    }

    private void GenMake(string impl_name, string tags_name)
    {
        sb.AppendLine();
        sb.AppendLine($"    #region Make");
        var space = "        ";
        foreach (var (@case, i) in Cases.Select(static (a, b) => (a, b)))
        {
            sb.AppendLine();
            sb.AppendLine($"    {AggressiveInlining}");
            sb.Append($"    public static {TypeName} Make{@case.Name}(");
            if (@case.IsRecord)
            {
                var first = true;
                foreach (var item in @case.Items)
                {
                    if (first) first = false;
                    else sb.Append(", ");
                    sb.Append($"{item.Type} {item.Name}");
                    if (item.DefaultValue != null)
                    {
                        sb.Append($" = {item.DefaultValue}");
                    }
                }
            }
            else if (@case.Type != "void")
            {
                sb.Append(@case.Type);
                sb.Append(" value");
            }
            sb.AppendLine($")");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        var _impl = new {impl_name}({tags_name}.{@case.Name});");
            if (@case.IsRecord)
            {
                var def = RecordTypeDefines[i];
                if (def.UnmanagedFields.Count > 0)
                {
                    var uti = UnmanagedTypes[def.UnmanagedTypeName];
                    foreach (var (_, index) in def.UnmanagedFields)
                    {
                        var item = @case.Items[index];
                        sb.AppendLine($"{space}_impl._u._{uti}._{index} = {item.Name};");
                    }
                }
                foreach (var (_, index, nth) in def.ClassFields)
                {
                    var item = @case.Items[index];
                    sb.AppendLine($"{space}_impl._c{nth} = {item.Name};");
                }
                foreach (var (type, index, nth) in def.OtherFields)
                {
                    var (ti, _) = OtherTypes[type];
                    var item = @case.Items[index];
                    sb.AppendLine($"{space}_impl._f{ti}_{nth} = {item.Name};");
                }
            }
            else if (@case.Type != "void")
            {
                if (@case.Kind == UnionCaseTypeKind.None)
                {
                    var (index, _) = OtherTypes![@case.Type];
                    sb.AppendLine($"{space}_impl._f{index}_0 = value;");
                }
                else if (@case.Kind == UnionCaseTypeKind.Class)
                {
                    sb.AppendLine($"{space}_impl._c0 = value;");
                }
                else if (@case.Kind == UnionCaseTypeKind.Unmanaged)
                {
                    var index = UnmanagedTypes[@case.Type];
                    sb.AppendLine($"{space}_impl._u._{index} = value;");
                }
            }
            sb.AppendLine($"{space}return new {TypeName}(_impl);");
            sb.AppendLine($"    }}");
        }
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Make");
    }

    private void GenIs(string tags_name)
    {
        sb.AppendLine();
        sb.AppendLine($"    #region Is");
        foreach (var @case in Cases)
        {
            sb.AppendLine();
            sb.AppendLine($"    public{@readonly} bool Is{@case.Name}");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        {AggressiveInlining}");
            sb.AppendLine($"        get => this._impl._tag == {tags_name}.{@case.Name};");
            sb.AppendLine($"    }}");
        }
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Is");
    }

    private void GenGetter(string impl_type)
    {
        sb.AppendLine();
        sb.AppendLine($"    #region Getter");
        foreach (var @case in Cases)
        {
            if (@case is { IsRecord: false, Type: "void" }) continue;
            sb.AppendLine();
            if (!IsClass) sb.AppendLine("    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]");
            var ro = ReadOnly ? " readonly" : "";
            var ret_type = @case.IsRecord ? string.Format(@case.Meta.ViewName, @case.Name) : @case.Type;
            var ret_ref = @case.IsRecord && SupportByRefFields ? "" : $" ref{ro}";
            sb.AppendLine($"    public{ret_ref} {ret_type} {@case.Name}");
            sb.AppendLine($"    {{");

            void GenField()
            {
                if (@case.IsRecord)
                {
                    if (SupportByRefFields)
                        sb.Append(
                            $"new {ret_type}(ref {RoImpl})");
                    else
                        sb.Append(
                            $"global::System.Runtime.CompilerServices.Unsafe.As<{impl_type}, {ret_type}>(ref {RoImpl})");
                }
                else if (@case.Kind == UnionCaseTypeKind.None)
                {
                    var (index, _) = OtherTypes![@case.Type];
                    sb.Append($"this._impl._f{index}_0!");
                }
                else if (@case.Kind == UnionCaseTypeKind.Class)
                {
                    sb.Append(
                        $"global::System.Runtime.CompilerServices.Unsafe.As<object?, {@case.Type}>(ref {RoImpl}._c0)");
                }
                else if (@case.Kind == UnionCaseTypeKind.Unmanaged)
                {
                    var index = UnmanagedTypes[@case.Type];
                    sb.Append($"this._impl._u._{index}!");
                }
            }

            #region Getter

            sb.AppendLine($"        {AggressiveInlining}");
            if (@case.IsRecord && SupportByRefFields)
                sb.Append(
                    $"        get => !this.Is{@case.Name} ? throw new global::System.NullReferenceException() : ");
            else
                sb.Append(
                    $"        get => ref !this.Is{@case.Name} ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<{ret_type}>() : ref ");
            GenField();
            sb.AppendLine($";");

            #endregion

            sb.AppendLine($"    }}");
        }
        sb.AppendLine();
        sb.AppendLine($"    #endregion // Getter");
    }

    private void GenEquatable(string tags_name)
    {
        if (!GenMethods.GenEquals) return;

        sb.AppendLine($"    #region Equals");
        sb.AppendLine();

        var q = IsClass && TypeName.Last() != '?' ? "?" : string.Empty;

        #region Equals

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public{@readonly} bool Equals({TypeName}{q} other) => this.Tag != other{q}.Tag ? false : this.Tag switch");
        sb.AppendLine($"    {{");
        foreach (var @case in Cases)
        {
            if (@case is { IsRecord: false, Type: "void" }) continue;
            if (@case.IsRecord)
            {
                sb.AppendLine(
                    $"        {tags_name}.{@case.Name} => {Self}.{@case.Name}.Equals(other.{@case.Name}),");
            }
            else
            {
                sb.AppendLine(
                    $"        {tags_name}.{@case.Name} => global::System.Collections.Generic.EqualityComparer<{@case.Type}>.Default.Equals({Self}.{@case.Name}, other.{@case.Name}),");
            }
        }
        sb.AppendLine($"        _ => true,");
        sb.AppendLine($"    }};");

        #endregion

        sb.AppendLine();

        #region HashCode

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine($"    public{@readonly} override int GetHashCode() => this.Tag switch");
        sb.AppendLine($"    {{");
        foreach (var @case in Cases)
        {
            if (@case is { IsRecord: false, Type: "void" }) continue;
            sb.AppendLine(
                @case.IsRecord
                    ? $"        {tags_name}.{@case.Name} => global::System.HashCode.Combine(this.Tag, {Self}.{@case.Name}.GetHashCode()),"
                    : $"        {tags_name}.{@case.Name} => global::System.HashCode.Combine(this.Tag, {Self}.{@case.Name}),");
        }
        sb.AppendLine($"        _ => global::System.HashCode.Combine(this.Tag),");
        sb.AppendLine($"    }};");

        #endregion

        sb.AppendLine();

        #region other

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public{@readonly} override bool Equals(object? obj) => obj is {TypeName} other && Equals(other);");

        sb.AppendLine();

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public static bool operator ==({TypeName}{q} left, {TypeName}{q} right) => left.Equals(right);");
        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public static bool operator !=({TypeName}{q} left, {TypeName}{q} right) => !left.Equals(right);");

        #endregion

        sb.AppendLine();
        sb.AppendLine($"    #endregion // Equals");
    }

    private void GenComparable(string tags_name)
    {
        if (!GenMethods.GenCompareTo) return;

        sb.AppendLine($"    #region CompareTo");
        sb.AppendLine();

        var q = IsClass && TypeName.Last() != '?' ? "?" : string.Empty;

        #region CompareTo

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public{@readonly} int CompareTo({TypeName}{q} other) => this.Tag != other{q}.Tag ? global::System.Collections.Generic.Comparer<{tags_name}{q}>.Default.Compare(this.Tag, other{q}.Tag) : this.Tag switch");
        sb.AppendLine($"    {{");
        foreach (var @case in Cases)
        {
            if (@case is { IsRecord: false, Type: "void" }) continue;
            if (@case.IsRecord)
            {
                sb.AppendLine(
                    $"        {tags_name}.{@case.Name} => {Self}.{@case.Name}.CompareTo(other.{@case.Name}),");
            }
            else
            {
                sb.AppendLine(
                    $"        {tags_name}.{@case.Name} => global::System.Collections.Generic.Comparer<{@case.Type}>.Default.Compare({Self}.{@case.Name}, other.{@case.Name}),");
            }
        }
        sb.AppendLine($"        _ => 0,");
        sb.AppendLine($"    }};");

        #endregion

        sb.AppendLine();

        #region other

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public static bool operator <({TypeName} left, {TypeName} right) => left.CompareTo(right) < 0;");
        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public static bool operator >({TypeName} left, {TypeName} right) => left.CompareTo(right) > 0;");
        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public static bool operator <=({TypeName} left, {TypeName} right) => left.CompareTo(right) <= 0;");
        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine(
            $"    public static bool operator >=({TypeName} left, {TypeName} right) => left.CompareTo(right) >= 0;");

        #endregion

        sb.AppendLine();
        sb.AppendLine($"    #endregion // CompareTo");
    }

    private void GenToStr(string tags_name)
    {
        if (!GenMethods.GenToString) return;

        sb.AppendLine($"    #region ToString");
        sb.AppendLine();

        sb.AppendLine($"    {AggressiveInlining}");
        sb.AppendLine($"    public{@readonly} override string ToString() => this.Tag switch");
        sb.AppendLine($"    {{");
        foreach (var @case in Cases)
        {
            if (@case.IsRecord)
            {
                sb.Append(
                    $"        {tags_name}.{@case.Name} => $\"{{({Self}.{@case.Name}.ToString())}}");
            }
            else
            {
                sb.Append(
                    $"        {tags_name}.{@case.Name} => $\"{{nameof({TypeName})}}.{{nameof({tags_name}.{@case.Name})}}");
                if (@case.Type != "void") sb.Append($" {{{{ {{({Self}.{@case.Name})}} }}}}");
            }

            sb.AppendLine($"\",");
        }
        sb.AppendLine($"        _ => nameof({TypeName}),");
        sb.AppendLine($"    }};");

        sb.AppendLine();
        sb.AppendLine($"    #endregion // ToString");
    }
}
