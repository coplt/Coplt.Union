using Coplt.Union;
using Coplt.Union.Misc;

namespace Tests;

[Union2, UnionSymbol(IsReferenceType = MayBool.True)]
public partial class list<T>
{
    [UnionTemplate]
    private interface Template
    {
        void Nil();
        void Cons(T Item, list<T> Tail);
    }
}

public static class TestList
{
    public static void Foo()
    {
        var a = list<int>.Cons(1, list<int>.Nil);
    }
}
