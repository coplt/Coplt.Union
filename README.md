# Sera.Union

[![.NET](https://github.com/sera-net/Sera.Union/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sera-net/Sera.Union/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/v/Sera.Union)](https://www.nuget.org/packages/Sera.Union/)
![MIT](https://img.shields.io/github/license/sera-net/Sera.Union)

Generate Tagged Union using source generator

- All unmanaged types will overlap
- All classes will overlap
- Other types will be tiled

## Example

```cs
[Union]
public readonly partial struct Union1
{
    [UnionTemplate]
    private abstract class Template
    {
        public abstract int A();
        public abstract string B();
        public abstract bool C();
        public abstract (int a, int b) D();
        public abstract void E();
        public abstract List<int>? F();
        public abstract (int a, string b) G();
    }
}
```

Generate output:

<details>
  <summary>Union1.union.g.cs</summary>

```cs
// <auto-generated/>

#nullable enable

using Sera.TaggedUnion;

public readonly partial struct Union1
    : global::Sera.TaggedUnion.ITaggedUnion
    , global::System.IEquatable<Union1>
    , global::System.IComparable<Union1>
#if NET7_0_OR_GREATER
    , global::System.Numerics.IEqualityOperators<Union1, Union1, bool>
    , global::System.Numerics.IComparisonOperators<Union1, Union1, bool>
#endif
{
    private readonly __impl_ _impl;
    private Union1(__impl_ _impl) { this._impl = _impl; }

    public readonly Tags Tag
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag;
    }

    public enum Tags : byte
    {
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        G = 7,
    }

    [global::System.Runtime.CompilerServices.CompilerGenerated]
    private struct __impl_
    {
        public __class_ _class_;
        public __unmanaged_ _unmanaged_;
        public (int a, string b) _0;
        public readonly Tags _tag;

        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]
        public struct __class_
        {
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public string _0;
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public List<int>? _1;
        }

        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]
        public struct __unmanaged_
        {
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public global::Sera.TaggedUnion.Hidden.Case<int> _0;
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public global::Sera.TaggedUnion.Hidden.Case<bool> _1;
            [global::System.Runtime.InteropServices.FieldOffset(0)]
            public global::Sera.TaggedUnion.Hidden.Case<(int a, int b)> _2;
        }

        public __impl_(Tags _tag)
        {
            this._class_ = default;
            global::System.Runtime.CompilerServices.Unsafe.SkipInit(out this._unmanaged_);
            this._0 = default!;
            this._tag = _tag;
        }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeA(int value)
    {
        var _impl = new __impl_(Tags.A);
        _impl._unmanaged_._0.Value = value;
        return new Union1(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeB(string value)
    {
        var _impl = new __impl_(Tags.B);
        _impl._class_._0 = value;
        return new Union1(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeC(bool value)
    {
        var _impl = new __impl_(Tags.C);
        _impl._unmanaged_._1.Value = value;
        return new Union1(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeD((int a, int b) value)
    {
        var _impl = new __impl_(Tags.D);
        _impl._unmanaged_._2.Value = value;
        return new Union1(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeE()
    {
        var _impl = new __impl_(Tags.E);
        return new Union1(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeF(List<int>? value)
    {
        var _impl = new __impl_(Tags.F);
        _impl._class_._1 = value;
        return new Union1(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Union1 MakeG((int a, string b) value)
    {
        var _impl = new __impl_(Tags.G);
        _impl._0 = value;
        return new Union1(_impl);
    }

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

    public int A
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsA ? default! : this._impl._unmanaged_._0.Value;
    }
    public string B
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsB ? default! : this._impl._class_._0;
    }
    public bool C
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsC ? default! : this._impl._unmanaged_._1.Value;
    }
    public (int a, int b) D
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsD ? default! : this._impl._unmanaged_._2.Value;
    }
    public List<int>? F
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsF ? default! : this._impl._class_._1;
    }
    public (int a, string b) G
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => !this.IsG ? default! : this._impl._0;
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Union1 other) => this.Tag != other.Tag ? false : this.Tag switch
    {
        Tags.A => global::System.Collections.Generic.EqualityComparer<int>.Default.Equals(this.A, other.A),
        Tags.B => global::System.Collections.Generic.EqualityComparer<string>.Default.Equals(this.B, other.B),
        Tags.C => global::System.Collections.Generic.EqualityComparer<bool>.Default.Equals(this.C, other.C),
        Tags.D => global::System.Collections.Generic.EqualityComparer<(int a, int b)>.Default.Equals(this.D, other.D),
        Tags.F => global::System.Collections.Generic.EqualityComparer<List<int>?>.Default.Equals(this.F, other.F),
        Tags.G => global::System.Collections.Generic.EqualityComparer<(int a, string b)>.Default.Equals(this.G, other.G),
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
        _ => global::System.HashCode.Combine(this.Tag),
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj) => obj is Union1 other && Equals(other);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Union1 left, Union1 right) => Equals(left, right);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Union1 left, Union1 right) => !Equals(left, right);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(Union1 other) => this.Tag != other.Tag ? Comparer<Tags>.Default.Compare(this.Tag, other.Tag) : this.Tag switch
    {
        Tags.A => global::System.Collections.Generic.Comparer<int>.Default.Compare(this.A, other.A),
        Tags.B => global::System.Collections.Generic.Comparer<string>.Default.Compare(this.B, other.B),
        Tags.C => global::System.Collections.Generic.Comparer<bool>.Default.Compare(this.C, other.C),
        Tags.D => global::System.Collections.Generic.Comparer<(int a, int b)>.Default.Compare(this.D, other.D),
        Tags.F => global::System.Collections.Generic.Comparer<List<int>?>.Default.Compare(this.F, other.F),
        Tags.G => global::System.Collections.Generic.Comparer<(int a, string b)>.Default.Compare(this.G, other.G),
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

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override string ToString() => this.Tag switch
    {
        Tags.A => $"{nameof(Union1)}.{nameof(Tags.A)} {{ {this.A} }}",
        Tags.B => $"{nameof(Union1)}.{nameof(Tags.B)} {{ {this.B} }}",
        Tags.C => $"{nameof(Union1)}.{nameof(Tags.C)} {{ {this.C} }}",
        Tags.D => $"{nameof(Union1)}.{nameof(Tags.D)} {{ {this.D} }}",
        Tags.E => $"{nameof(Union1)}.{nameof(Tags.E)}",
        Tags.F => $"{nameof(Union1)}.{nameof(Tags.F)} {{ {this.F} }}",
        Tags.G => $"{nameof(Union1)}.{nameof(Tags.G)} {{ {this.G} }}",
        _ => nameof(Union1),
    };
}
```

</details>

---

### Support generics

**Generics don't overlap**

```cs
[Union]
public partial struct Option<T>
{
    [UnionTemplate]
    private abstract class Template
    {
        public abstract T Some();
        public abstract void None();
    }
}

[Union]
public partial struct Result<T, E>
{
    [UnionTemplate]
    private abstract class Template
    {
        public abstract E Ok();
        public abstract E Err();
    }
}
```

Generate output:

<details>
  <summary>Option[T].union.g.cs</summary>

```cs
// <auto-generated/>

#nullable enable

using Sera.TaggedUnion;


public partial struct Option<T>
    : global::Sera.TaggedUnion.ITaggedUnion
    , global::System.IEquatable<Option<T>>
    , global::System.IComparable<Option<T>>
#if NET7_0_OR_GREATER
    , global::System.Numerics.IEqualityOperators<Option<T>, Option<T>, bool>
    , global::System.Numerics.IComparisonOperators<Option<T>, Option<T>, bool>
#endif
{
    private __impl_ _impl;
    private Option(__impl_ _impl) { this._impl = _impl; }

    public readonly Tags Tag
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag;
    }

    public enum Tags : byte
    {
        Some = 1,
        None = 2,
    }

    [global::System.Runtime.CompilerServices.CompilerGenerated]
    private struct __impl_
    {
        public T _0;
        public readonly Tags _tag;

        public __impl_(Tags _tag)
        {
            this._0 = default!;
            this._tag = _tag;
        }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Option<T> MakeSome(T value)
    {
        var _impl = new __impl_(Tags.Some);
        _impl._0 = value;
        return new Option<T>(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Option<T> MakeNone()
    {
        var _impl = new __impl_(Tags.None);
        return new Option<T>(_impl);
    }

    public readonly bool IsSome
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.Some;
    }
    public readonly bool IsNone
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.None;
    }

    public T Some
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        readonly get => !this.IsSome ? default! : this._impl._0;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        set { if (this.IsSome) { this._impl._0 = value; } }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Option<T> other) => this.Tag != other.Tag ? false : this.Tag switch
    {
        Tags.Some => global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(this.Some, other.Some),
        _ => true,
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode() => this.Tag switch
    {
        Tags.Some => global::System.HashCode.Combine(this.Tag, this.Some),
        _ => global::System.HashCode.Combine(this.Tag),
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Option<T> left, Option<T> right) => Equals(left, right);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Option<T> left, Option<T> right) => !Equals(left, right);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(Option<T> other) => this.Tag != other.Tag ? Comparer<Tags>.Default.Compare(this.Tag, other.Tag) : this.Tag switch
    {
        Tags.Some => global::System.Collections.Generic.Comparer<T>.Default.Compare(this.Some, other.Some),
        _ => 0,
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Option<T> left, Option<T> right) => left.CompareTo(right) < 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Option<T> left, Option<T> right) => left.CompareTo(right) > 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Option<T> left, Option<T> right) => left.CompareTo(right) <= 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Option<T> left, Option<T> right) => left.CompareTo(right) >= 0;

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override string ToString() => this.Tag switch
    {
        Tags.Some => $"{nameof(Option<T>)}.{nameof(Tags.Some)} {{ {this.Some} }}",
        Tags.None => $"{nameof(Option<T>)}.{nameof(Tags.None)}",
        _ => nameof(Option<T>),
    };
}

```

</details>

<br>

<details>
  <summary>Result[T,E].union.g.cs</summary>

```cs
// <auto-generated/>

#nullable enable

using Sera.TaggedUnion;


public partial struct Result<T, E>
    : global::Sera.TaggedUnion.ITaggedUnion
    , global::System.IEquatable<Result<T, E>>
    , global::System.IComparable<Result<T, E>>
#if NET7_0_OR_GREATER
    , global::System.Numerics.IEqualityOperators<Result<T, E>, Result<T, E>, bool>
    , global::System.Numerics.IComparisonOperators<Result<T, E>, Result<T, E>, bool>
#endif
{
    private __impl_ _impl;
    private Result(__impl_ _impl) { this._impl = _impl; }

    public readonly Tags Tag
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag;
    }

    public enum Tags : byte
    {
        Ok = 1,
        Err = 2,
    }

    [global::System.Runtime.CompilerServices.CompilerGenerated]
    private struct __impl_
    {
        public E _0;
        public readonly Tags _tag;

        public __impl_(Tags _tag)
        {
            this._0 = default!;
            this._tag = _tag;
        }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> MakeOk(E value)
    {
        var _impl = new __impl_(Tags.Ok);
        _impl._0 = value;
        return new Result<T, E>(_impl);
    }
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> MakeErr(E value)
    {
        var _impl = new __impl_(Tags.Err);
        _impl._0 = value;
        return new Result<T, E>(_impl);
    }

    public readonly bool IsOk
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.Ok;
    }
    public readonly bool IsErr
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => this._impl._tag == Tags.Err;
    }

    public E Ok
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        readonly get => !this.IsOk ? default! : this._impl._0;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        set { if (this.IsOk) { this._impl._0 = value; } }
    }
    public E Err
    {
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        readonly get => !this.IsErr ? default! : this._impl._0;
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        set { if (this.IsErr) { this._impl._0 = value; } }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Result<T, E> other) => this.Tag != other.Tag ? false : this.Tag switch
    {
        Tags.Ok => global::System.Collections.Generic.EqualityComparer<E>.Default.Equals(this.Ok, other.Ok),
        Tags.Err => global::System.Collections.Generic.EqualityComparer<E>.Default.Equals(this.Err, other.Err),
        _ => true,
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode() => this.Tag switch
    {
        Tags.Ok => global::System.HashCode.Combine(this.Tag, this.Ok),
        Tags.Err => global::System.HashCode.Combine(this.Tag, this.Err),
        _ => global::System.HashCode.Combine(this.Tag),
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj) => obj is Result<T, E> other && Equals(other);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Result<T, E> left, Result<T, E> right) => Equals(left, right);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Result<T, E> left, Result<T, E> right) => !Equals(left, right);

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(Result<T, E> other) => this.Tag != other.Tag ? Comparer<Tags>.Default.Compare(this.Tag, other.Tag) : this.Tag switch
    {
        Tags.Ok => global::System.Collections.Generic.Comparer<E>.Default.Compare(this.Ok, other.Ok),
        Tags.Err => global::System.Collections.Generic.Comparer<E>.Default.Compare(this.Err, other.Err),
        _ => 0,
    };

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Result<T, E> left, Result<T, E> right) => left.CompareTo(right) < 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Result<T, E> left, Result<T, E> right) => left.CompareTo(right) > 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Result<T, E> left, Result<T, E> right) => left.CompareTo(right) <= 0;
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Result<T, E> left, Result<T, E> right) => left.CompareTo(right) >= 0;

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly override string ToString() => this.Tag switch
    {
        Tags.Ok => $"{nameof(Result<T, E>)}.{nameof(Tags.Ok)} {{ {this.Ok} }}",
        Tags.Err => $"{nameof(Result<T, E>)}.{nameof(Tags.Err)} {{ {this.Err} }}",
        _ => nameof(Result<T, E>),
    };
}

```

</details>
