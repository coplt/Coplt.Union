# Coplt.Union

[![.NET](https://github.com/coplt/Coplt.Union/actions/workflows/dotnet.yml/badge.svg)](https://github.com/coplt/Coplt.Union/actions/workflows/dotnet.yml)
![MIT](https://img.shields.io/github/license/coplt/Coplt.Union)

- Sera.Union  
  [![Nuget](https://img.shields.io/nuget/v/Coplt.Union)](https://www.nuget.org/packages/Coplt.Union/)
  [![openupm](https://img.shields.io/npm/v/net.sera.union?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.sera.union/)
- Sera.Union.Utilities  
  [![Nuget](https://img.shields.io/nuget/v/Coplt.Union.Utilities)](https://www.nuget.org/packages/Coplt.Union.Utilities/)  
  Includes `Option<T>` and `Result<T, E>`

Generate Tagged Union using source generator

- Source distribution, no runtime dependencies
- All unmanaged types will overlap
- All classes will overlap
- Other types are sequential
- Support generics, but generics cannot overlap

## Usage

```cs
[Union]
public readonly partial struct Union1
{
    // This template is not used at runtime, its only purpose is to provide type symbols to the roslyn analyzer
    [UnionTemplate]
    private interface Template
    {
        int A();
        string B();
        bool C();
        (int a, int b) D();
        void E();               // Tag only variant
        List<int>? F();
        (int a, string b) G();
        // Record mode, in this mode, multiple fields are stored separately.
        void H(int a, int b, string c, HashSet<int> d, (int a, string b) e);
    }
}
```

Will generate (pseudocode):

```cs
public readonly partial struct Union1
{
    private readonly __impl_ _impl;
    
    // If the first item is a Tag only, it starts at 0, otherwise it starts at 1
    // Use [UnionTag(value)] to explicitly mark enum values
    public enum Tags : byte { A = 1, B, C, D, E, F, G, H }
    
    private struct __impl_
    {
        public object? _c0;                 // All classes will overlap
        public object? _c1;                 // The second reference type is used by variant H, record mode allows multiple types
        public __unmanaged_ _u;             // All unmanaged types will overlap
        public (int a, string b) _f0_0;     // Mixed types cannot overlap
        public readonly Tags _tag;
        
        [StructLayout(LayoutKind.Explicit)] internal struct __unmanaged_
        {
            [FieldOffset(0)] public int  _0;
            [FieldOffset(0)] public bool _1;
            [FieldOffset(0)] public (int a, int b) _2;
            [FieldOffset(0)] public __record_7_unmanaged_ _3;
        }
        
        // The fields of the record mode variant are stored separately, with the unmanaged part having a separate structure
        internal struct __record_7_unmanaged_
        {
            public int _0;
            public int _1;
        }
    }
    
    public static Union1 MakeA(int value) { ... }
    public static Union1 MakeB(string value) { ... }
    public static Union1 MakeC(bool value) { ... }
    public static Union1 MakeD((int a, int b) value) { ... }
    public static Union1 MakeE() { ... }
    public static Union1 MakeF(List<int>? value) { ... }
    public static Union1 MakeG((int a, string b) value) { ... }
    public static Union1 MakeH(int a, int b, string c, HashSet<int> d, (int a, string b) e) { ... }
    
    public readonly Tags Tag { get; }
    public readonly bool IsA { get; }
    public readonly bool IsB { get; }
    public readonly bool IsC { get; }
    public readonly bool IsD { get; }
    public readonly bool IsE { get; }
    public readonly bool IsF { get; }
    public readonly bool IsG { get; }
    public readonly bool IsH { get; }
    
    // ref readonly if sturct is readonly, otherwise ref only
    // If the current tag does not match, a null reference will be returned
    public ref readonly int A  => ref _impl._u._0;
    public ref readonly string B  => ref Unsafe.As<object?, string>(ref _impl._c0);
    public ref readonly bool C  => ref _impl._u._1;
    public ref readonly (int a, int b) D  => ref _impl._u._2;
    // E is a Tag only so there is no value getter
    public ref readonly List<int>? F  => ref Unsafe.As<object?, List<int>?>(ref _impl._c0);
    public ref readonly (int a, string b) G  => ref _impl._f0_0;
    // When gets a record variant, it will get a magic ref struct for forwarding field references
    public VariantH H => new(ref _impl);
    
    ... Eq Cmp ToString
    
    public readonly ref struct VariantH
    {
        private ref readonly __impl_ _impl;
        
        public ref readonly int a => ref _impl._u._3._0;
        public ref readonly int b  => ref _impl._u._3._1;
        public ref readonly string c => ref Unsafe.As<object?, string>(ref _impl._c0);
        public ref readonly HashSet<int> d  => ref Unsafe.As<object?, HashSet<int>>(ref _impl._c1);
        public ref readonly (int a, string b) e  => ref _impl._f0_0;
        
        ... Eq Cmp ToString
    }
}
```

Complete generate output:

<details>
  <summary>Union1.union.g.cs</summary>

```cs
// <auto-generated/>

#nullable disable warnings
#nullable enable annotations

using Coplt.Union;

public readonly partial struct Union1
    : global::Coplt.Union.ITaggedUnion
    , global::System.IEquatable<Union1>
    , global::System.IComparable<Union1>
    #if NET7_0_OR_GREATER
    , global::System.Numerics.IEqualityOperators<Union1, Union1, bool>
    , global::System.Numerics.IComparisonOperators<Union1, Union1, bool>
    #endif
{
    #region Fields

    private readonly __impl_ _impl;

    #endregion // Fields

    #region Ctor

    private Union1(__impl_ _impl) { this._impl = _impl; }

    #endregion // Ctor

    #region Tag Getter

    public readonly Tags Tag
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag;
    }

    #endregion // Tag Getter

    #region Tags

    public enum Tags : byte
    {
        A = 1,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
    }

    #endregion // Tags

    #region Impl

    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]
    internal struct __impl_
    {
        public object? _c0;
        public object? _c1;
        public __unmanaged_ _u;
        public (int a, string b) _f0_0;
        public readonly Tags _tag;

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public __impl_(Tags _tag)
        {
            this._c0 = null;
            this._c1 = null;
            global::System.Runtime.CompilerServices.Unsafe.SkipInit(out this._u);
            this._f0_0 = default!;
            this._tag = _tag;
        }

        [global::System.Runtime.CompilerServices.CompilerGenerated]
        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]
        internal struct __unmanaged_
        {
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public int _0;
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public bool _1;
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public (int a, int b) _2;
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public __record_7_unmanaged_ _3;
        }

        [global::System.Runtime.CompilerServices.CompilerGenerated]
        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Auto)]
        internal struct __record_7_unmanaged_
        {
            public int _0;
            public int _1;
        }
    }

    #endregion // Impl

    #region Views

    public readonly ref struct VariantH
        #if NET9_0_OR_GREATER
        : global::System.IEquatable<VariantH>
        , global::System.IComparable<VariantH>
        #endif
    {
        #region Fields

        private readonly ref readonly __impl_ _impl;

        #endregion // Fields

        #region Ctor

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal VariantH(ref readonly __impl_ impl)
        {
            _impl = ref impl;
        }

        #endregion // Ctor

        #region Getter

        public ref readonly int a
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._u._3._0;
        }
        public ref readonly int b
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._u._3._1;
        }
        public ref readonly string c
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => ref global::System.Runtime.CompilerServices.Unsafe.As<object?, string>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._c0);
        }
        public ref readonly global::System.Collections.Generic.HashSet<int> d
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => ref global::System.Runtime.CompilerServices.Unsafe.As<object?, global::System.Collections.Generic.HashSet<int>>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._c1);
        }
        public ref readonly (int a, string b) e
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._f0_0;
        }

        #endregion // Getter

        #region Equals

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(scoped VariantH other) =>
            global::System.Collections.Generic.EqualityComparer<int>.Default.Equals(a, other.a) &&
            global::System.Collections.Generic.EqualityComparer<int>.Default.Equals(b, other.b) &&
            global::System.Collections.Generic.EqualityComparer<string>.Default.Equals(c, other.c) &&
            global::System.Collections.Generic.EqualityComparer<global::System.Collections.Generic.HashSet<int>>.Default.Equals(d, other.d) &&
            global::System.Collections.Generic.EqualityComparer<(int a, string b)>.Default.Equals(e, other.e);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => global::System.HashCode.Combine(a, b, c, d, e);

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => false;

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(scoped VariantH left, scoped VariantH right) => left.Equals(right);
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(scoped VariantH left, scoped VariantH right) => !left.Equals(right);

        #endregion // Equals

        #region CompareTo

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(scoped VariantH other)
        {
            var _0 = global::System.Collections.Generic.Comparer<int>.Default.Compare(a, other.a);
            if (_0 != 0) return _0;
            var _1 = global::System.Collections.Generic.Comparer<int>.Default.Compare(b, other.b);
            if (_1 != 0) return _1;
            var _2 = global::System.Collections.Generic.Comparer<string>.Default.Compare(c, other.c);
            if (_2 != 0) return _2;
            var _3 = global::System.Collections.Generic.Comparer<global::System.Collections.Generic.HashSet<int>>.Default.Compare(d, other.d);
            if (_3 != 0) return _3;
            var _4 = global::System.Collections.Generic.Comparer<(int a, string b)>.Default.Compare(e, other.e);
            if (_4 != 0) return _4;
            return 0;
        }

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(scoped VariantH left, scoped VariantH right) => left.CompareTo(right) < 0;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(scoped VariantH left, scoped VariantH right) => left.CompareTo(right) > 0;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(scoped VariantH left, scoped VariantH right) => left.CompareTo(right) <= 0;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(scoped VariantH left, scoped VariantH right) => left.CompareTo(right) >= 0;

        #endregion // CompareTo

        #region ToString

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{nameof(Union1)}.{nameof(Tags.H)} {{ a = {a}, b = {b}, c = {c}, d = {d}, e = {e} }}";

        #endregion // ToString

        #region Deconstruct

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out int a, out int b, out string c, out global::System.Collections.Generic.HashSet<int> d, out (int a, string b) e)
        {
            a = this.a;
            b = this.b;
            c = this.c;
            d = this.d;
            e = this.e;
        }

        #endregion // Deconstruct
    }

    #endregion // Views

    #region Make

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeA(int value)
    {
        var _impl = new __impl_(Tags.A);
        _impl._u._0 = value;
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeB(string value)
    {
        var _impl = new __impl_(Tags.B);
        _impl._c0 = value;
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeC(bool value)
    {
        var _impl = new __impl_(Tags.C);
        _impl._u._1 = value;
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeD((int a, int b) value)
    {
        var _impl = new __impl_(Tags.D);
        _impl._u._2 = value;
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeE()
    {
        var _impl = new __impl_(Tags.E);
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeF(global::System.Collections.Generic.List<int>? value)
    {
        var _impl = new __impl_(Tags.F);
        _impl._c0 = value;
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeG((int a, string b) value)
    {
        var _impl = new __impl_(Tags.G);
        _impl._f0_0 = value;
        return new Union1(_impl);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeH(int a, int b, string c, global::System.Collections.Generic.HashSet<int> d, (int a, string b) e)
    {
        var _impl = new __impl_(Tags.H);
        _impl._u._3._0 = a;
        _impl._u._3._1 = b;
        _impl._c0 = c;
        _impl._c1 = d;
        _impl._f0_0 = e;
        return new Union1(_impl);
    }

    #endregion // Make

    #region Is

    public readonly bool IsA
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.A;
    }

    public readonly bool IsB
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.B;
    }

    public readonly bool IsC
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.C;
    }

    public readonly bool IsD
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.D;
    }

    public readonly bool IsE
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.E;
    }

    public readonly bool IsF
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.F;
    }

    public readonly bool IsG
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.G;
    }

    public readonly bool IsH
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.H;
    }

    #endregion // Is

    #region Getter

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public ref readonly int A
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => ref !this.IsA ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<int>() : ref this._impl._u._0!;
    }

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public ref readonly string B
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => ref !this.IsB ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<string>() : ref global::System.Runtime.CompilerServices.Unsafe.As<object?, string>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._c0);
    }

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public ref readonly bool C
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => ref !this.IsC ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<bool>() : ref this._impl._u._1!;
    }

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public ref readonly (int a, int b) D
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => ref !this.IsD ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<(int a, int b)>() : ref this._impl._u._2!;
    }

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public ref readonly global::System.Collections.Generic.List<int>? F
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => ref !this.IsF ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<global::System.Collections.Generic.List<int>?>() : ref global::System.Runtime.CompilerServices.Unsafe.As<object?, global::System.Collections.Generic.List<int>?>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl)._c0);
    }

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public ref readonly (int a, string b) G
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => ref !this.IsG ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<(int a, string b)>() : ref this._impl._f0_0!;
    }

    [global::System.Diagnostics.CodeAnalysis.UnscopedRef]
    public VariantH H
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsH ? throw new global::System.NullReferenceException() : new VariantH(ref global::System.Runtime.CompilerServices.Unsafe.AsRef<Union1.__impl_>(in this._impl));
    }

    #endregion // Getter

    #region Equals

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Union1 other) => this.Tag != other.Tag ? false : this.Tag switch
    {
        Tags.A => global::System.Collections.Generic.EqualityComparer<int>.Default.Equals(this.A, other.A),
        Tags.B => global::System.Collections.Generic.EqualityComparer<string>.Default.Equals(this.B, other.B),
        Tags.C => global::System.Collections.Generic.EqualityComparer<bool>.Default.Equals(this.C, other.C),
        Tags.D => global::System.Collections.Generic.EqualityComparer<(int a, int b)>.Default.Equals(this.D, other.D),
        Tags.F => global::System.Collections.Generic.EqualityComparer<global::System.Collections.Generic.List<int>?>.Default.Equals(this.F, other.F),
        Tags.G => global::System.Collections.Generic.EqualityComparer<(int a, string b)>.Default.Equals(this.G, other.G),
        Tags.H => this.H.Equals(other.H),
        _ => true,
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode() => this.Tag switch
    {
        Tags.A => global::System.HashCode.Combine(this.Tag, this.A),
        Tags.B => global::System.HashCode.Combine(this.Tag, this.B),
        Tags.C => global::System.HashCode.Combine(this.Tag, this.C),
        Tags.D => global::System.HashCode.Combine(this.Tag, this.D),
        Tags.F => global::System.HashCode.Combine(this.Tag, this.F),
        Tags.G => global::System.HashCode.Combine(this.Tag, this.G),
        Tags.H => global::System.HashCode.Combine(this.Tag, this.H.GetHashCode()),
        _ => global::System.HashCode.Combine(this.Tag),
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj) => obj is Union1 other && Equals(other);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Union1 left, Union1 right) => left.Equals(right);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Union1 left, Union1 right) => !left.Equals(right);

    #endregion // Equals

    #region CompareTo

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(Union1 other) => this.Tag != other.Tag ? global::System.Collections.Generic.Comparer<Tags>.Default.Compare(this.Tag, other.Tag) : this.Tag switch
    {
        Tags.A => global::System.Collections.Generic.Comparer<int>.Default.Compare(this.A, other.A),
        Tags.B => global::System.Collections.Generic.Comparer<string>.Default.Compare(this.B, other.B),
        Tags.C => global::System.Collections.Generic.Comparer<bool>.Default.Compare(this.C, other.C),
        Tags.D => global::System.Collections.Generic.Comparer<(int a, int b)>.Default.Compare(this.D, other.D),
        Tags.F => global::System.Collections.Generic.Comparer<global::System.Collections.Generic.List<int>?>.Default.Compare(this.F, other.F),
        Tags.G => global::System.Collections.Generic.Comparer<(int a, string b)>.Default.Compare(this.G, other.G),
        Tags.H => this.H.CompareTo(other.H),
        _ => 0,
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Union1 left, Union1 right) => left.CompareTo(right) < 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Union1 left, Union1 right) => left.CompareTo(right) > 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Union1 left, Union1 right) => left.CompareTo(right) <= 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Union1 left, Union1 right) => left.CompareTo(right) >= 0;

    #endregion // CompareTo

    #region ToString

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override string ToString() => this.Tag switch
    {
        Tags.A => $"{nameof(Union1)}.{nameof(Tags.A)} {{ {(this.A)} }}",
        Tags.B => $"{nameof(Union1)}.{nameof(Tags.B)} {{ {(this.B)} }}",
        Tags.C => $"{nameof(Union1)}.{nameof(Tags.C)} {{ {(this.C)} }}",
        Tags.D => $"{nameof(Union1)}.{nameof(Tags.D)} {{ {(this.D)} }}",
        Tags.E => $"{nameof(Union1)}.{nameof(Tags.E)}",
        Tags.F => $"{nameof(Union1)}.{nameof(Tags.F)} {{ {(this.F)} }}",
        Tags.G => $"{nameof(Union1)}.{nameof(Tags.G)} {{ {(this.G)} }}",
        Tags.H => $"{(this.H.ToString())}",
        _ => nameof(Union1),
    };

    #endregion // ToString
}
```

</details>

### How to use

You can manually determine the Tag or use pattern matching.  
But remember C# **does not have enum exhaustion semantics**.

```cs
var u = Union1.MakeA(123);

if (u is { Tag: Union1.Tags.A, A: var a }) { }

if (u is { IsA: true, A: var a }) { }

if (u.IsA)
{
    var a = u.A;
}

switch (u.Tag)
{
    case Union1.Tags.A:
        break;
  ...
}

switch (u.Tag)
{
    case { IsA: true, A: var a }:
        break;
  ...
}
```

## Examples

### Foo

```cs
// sizeof(Foo) == 16 (8 data, 1 tag, 7 padding)
[Union]
public partial struct Foo
{
    [UnionTemplate]
    private interface Template
    {
        int A(); 
        float B();
        long C();
        double D();
    }
}
```

### Option

```cs
[Union]
public partial struct Option<T>
{
    [UnionTemplate]
    private interface Template
    {
        void None();    // tag is 0
        T Some();
    }
}
```

### Result

```cs
[Union]
public partial struct Result<T, E>
{
    [UnionTemplate]
    private interface Template
    {
        T Ok();    // tag is 1
        E Err();
    }
}
```

### Shape

[F# Shape](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions#remarks)

```cs
[Union]
public partial struct Shape
{
    [UnionTemplate]
    private interface Template
    {
        void Rectangle(float Width, float Length);
        void Circle(float Radius);
        void Prism(float Width1, float Width2, float Height);
    }
}

public static void Foo()
{
    var rect = Shape.MakeRectangle(Length: 1.3f, Width: 1.3f);
    var circ = Shape.MakeCircle(1.0f);
    var prism = Shape.MakePrism(5f, 2f, Height: 3f);
}
public static void Foo(Shape shape)
{
    switch (shape)
    {
        case { IsRectangle: true, Rectangle.Width: var width }:
            break;
        case { IsCircle: true, Circle.Radius: var radius }:
            break;
        case { IsPrism: true, Prism.Width1: var width }:
            break;
    }
}
```

### Tree

[F# Tree](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions#using-discriminated-unions-for-tree-data-structures)

```cs
[Union]
public partial class Tree
{
    [UnionTemplate]
    private interface Template
    {
        void Tip();
        void Node(int Value, Tree Left, Tree Right);
    }

    public static int Sum(Tree Tree) => Tree switch
    {
        // Node: is a ref struct, which will avoid copying when matching patterns
        { IsNode: true, Node: var (value, left, right) } => value + Sum(left) + Sum(right),
        _ => 0,
    };

    public static int Sum2(Tree Tree) => Tree switch
    {
        // Node: is a ref struct, which will avoid copying when matching patterns
        { IsNode: true, Node: var node } =>
            node.Value + Sum(node.Left) + Sum(node.Right),
        _ => 0,
    };
}
```


