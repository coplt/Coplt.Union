using System.Text.Json;
using Coplt.Union.Utilities;

namespace Tests;

using ResultIntInt = Coplt.Union.Utilities.Result<int, int>;

public class TestResult
{
    [Test]
    public void TestJson1()
    {
        var o = ResultIntInt.Ok(123);
        Console.WriteLine(o);
        var json = JsonSerializer.Serialize(o);
        Console.WriteLine(json);
        var r = JsonSerializer.Deserialize<ResultIntInt>(json);
        Console.WriteLine(r);
        Assert.That(r, Is.EqualTo(o));
    }

    [Test]
    public void TestJson2()
    {
        var o = ResultIntInt.Err(456);
        Console.WriteLine(o);
        var json = JsonSerializer.Serialize(o);
        Console.WriteLine(json);
        var r = JsonSerializer.Deserialize<ResultIntInt>(json);
        Console.WriteLine(r);
        Assert.That(r, Is.EqualTo(o));
    }
}
