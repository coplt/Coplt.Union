using Coplt.Union;

namespace Tests;

[Union2]
public partial struct Generic1<T>
{
    [UnionTemplate]
    private interface Template
    {
        void A(int a, T b);
        int B();
        float C();
    }
}

public class TestLoadGeneric
{
    [Test]
    public void Test1()
    {
        var a = Generic1<int>.B(123);
        Console.WriteLine(a);
    }
    
}
