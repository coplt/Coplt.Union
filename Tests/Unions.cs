using System.Runtime.InteropServices;
using Coplt.Union;

namespace Tests;

[Union2]
public readonly partial struct Union1
{
    [UnionTemplate]
    private interface Template
    {
        int A();
        string B();
        bool C();
        (int a, int b) D();
        void E();
        List<int>? F();
        (int a, string b) G();
        void H(int a, int b, string c, HashSet<int> d, (int a, string b) e);
    }
}

[Union2]
public partial struct Union2
{
    [UnionTemplate]
    private interface Template
    {
        int A();
        string B();
        bool C();
        (int a, int b) D();
        void E();
        List<int>? F();
        (int a, string b) G();
        void H(int a, int b, string c, HashSet<int> d, (int a, string b) e);
    }
}

[Union2]
public partial struct Option<T>
{
    [UnionTemplate]
    private interface Template
    {
        T Some();
        void None();
    }
}

[Union2]
public partial struct Result<T, E>
{
    [UnionTemplate]
    private interface Template
    {
        T Ok();
        E Err();
    }
}

[Union2]
public partial struct Empty { }

[Union2(TagsName = "Kind")]
public partial struct Union3 { }

[Union2(ExternalTags = true)]
public partial struct Union4 { }

[Union2(ExternalTags = true, ExternalTagsName = "{0}Kind")]
public partial struct Union5 { }

[Union2(TagsUnderlying = typeof(ulong))]
public partial struct Union6 { }

[Union2]
public partial struct Union7
{
    [UnionTemplate]
    private interface Template
    {
        [Variant(Tag = 123)]
        int Foo();
    }
}

[Union2(ExternalTags = true, ExternalTagsName = "Union8Foo")]
public partial struct Union8 { }

[Union2]
public partial class Union9
{
    [UnionTemplate]
    private interface Template
    {
        int A();
        string B();
        bool C();
        (int a, int b) D();
        void E();
        List<int>? F();
        (int a, string b) G();
        void H(int a, int b, string c, HashSet<int> d, (int a, string b) e);
    }
}

[Union2(GenerateEquals = false, GenerateCompareTo = false)]
public partial class Union10
{
    public override string ToString() => "Fuck";
}

[Union2]
public partial class Union11
{
    [UnionTemplate]
    private interface Template
    {
        void Foo((float, string) a, (float, string) b, (int, string) c);
    }
}

public struct Union12Foo { }

[Union2]
public partial class Union12
{
    [UnionTemplate]
    private interface Template
    {
        void Foo(int a = 1, bool b = true, string c = "foo", int? d = null, object? e = null);
        void Foo2(byte a = 1, uint b = 1, Union12Foo c = default, nuint d = 1);
        void Foo3(float a = 1.0f, double b = 1.0, decimal c = 1.0m, ulong d = 1);
        void Foo4(LayoutKind a = LayoutKind.Auto, LayoutKind b = LayoutKind.Sequential);
    }
}
