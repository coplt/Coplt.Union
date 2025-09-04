using Coplt.Union;
using Coplt.Union.Misc;

namespace Tests;

public partial class TestSymbol
{
    [Union2]
    public readonly partial struct Union1
    {
        [UnionTemplate]
        private interface Template
        {
            int A();
            string B();
        }
    }

    [Union2]
    [UnionSymbol(IsUnmanagedType = MayBool.False)]
    public readonly partial struct Union2
    {
        [UnionTemplate]
        private interface Template
        {
            [UnionSymbol(IsUnmanagedType = MayBool.False)]
            Union1 Union1();
        }
    }
    
    [Union2]
    public readonly partial struct Union3
    {
        [UnionTemplate]
        private interface Template
        {
            Union2 Union2();
        }
    }
}
