namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1()
    {
        var u = Union1.MakeA(123);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.A));
        Assert.That(u.A, Is.EqualTo(123));

        u = Union1.MakeB("asd");
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.B));
        Assert.That(u.B, Is.EqualTo("asd"));

        u = Union1.MakeC(true);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.C));
        Assert.That(u.C, Is.True);

        u = Union1.MakeD((1, 2));
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.D));
        Assert.That(u.D, Is.EqualTo((1, 2)));

        u = Union1.MakeE();
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.E));

        u = Union1.MakeF(null);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.F));
        Assert.That(u.F, Is.Null);

        var l = new List<int>();
        u = Union1.MakeF(l);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.F));
        Assert.That(u.F, Is.EqualTo(l));

        u = Union1.MakeG((123, "asd"));
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.G));
        Assert.That(u.G, Is.EqualTo((123, "asd")));

        u = Union1.MakeA(123);
        Assert.Throws<NullReferenceException>(() => Console.WriteLine(u.B));
    }


    [Test]
    public void Test2()
    {
        var u = Union2.MakeA(123);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.A));
        Assert.That(u.A, Is.EqualTo(123));

        u = Union2.MakeB("asd");
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.B));
        Assert.That(u.B, Is.EqualTo("asd"));

        u = Union2.MakeC(true);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.C));
        Assert.That(u.C, Is.True);

        u = Union2.MakeD((1, 2));
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.D));
        Assert.That(u.D, Is.EqualTo((1, 2)));

        u = Union2.MakeE();
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.E));

        u = Union2.MakeF(null);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.F));
        Assert.That(u.F, Is.Null);

        var l = new List<int>();
        u = Union2.MakeF(l);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.F));
        Assert.That(u.F, Is.EqualTo(l));

        u = Union2.MakeG((123, "asd"));
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.G));
        Assert.That(u.G, Is.EqualTo((123, "asd")));

        u = Union2.MakeA(123);
        Assert.Throws<NullReferenceException>(() => Console.WriteLine(u.B));

        u.A = 456;
        Assert.That(u.A, Is.EqualTo(456));
    }
}
