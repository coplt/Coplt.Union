using Coplt.Union;
using Coplt.Union.Misc;

namespace Tests;

[Union, UnionSymbol(IsReferenceType = MayBool.True)]
public partial class list<T>
{
    [UnionTemplate]
    private interface Template
    {
        void Nil();
        void Cons(T Item, list<T> Tail);
    }
}
