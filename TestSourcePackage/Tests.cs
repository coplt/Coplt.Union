using System.Diagnostics.CodeAnalysis;
using Coplt.Union;

namespace TestSourcePackage;

[Union]
public partial struct Foo
{
    [UnionTemplate]
    private interface Template
    {
        int A();
        float B();
    }

    public void Some()
    {
        Console.WriteLine(IsA ? A : IsB ? B : null);
    }
}
