namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1()
    {
        var u = Union1.A(123);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.A));
        Assert.That(u.A, Is.EqualTo(123));

        u = Union1.B("asd");
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.B));
        Assert.That(u.B, Is.EqualTo("asd"));

        u = Union1.C(true);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.C));
        Assert.That(u.C, Is.True);

        u = Union1.D((1, 2));
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.D));
        Assert.That(u.D, Is.EqualTo((1, 2)));

        u = Union1.E;
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.E));

        u = Union1.F(null);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.F));
        Assert.That(u.F, Is.Null);

        var l = new List<int>();
        u = Union1.F(l);
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.F));
        Assert.That(u.F, Is.EqualTo(l));

        u = Union1.G((123, "asd"));
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.G));
        Assert.That(u.G, Is.EqualTo((123, "asd")));

        u = Union1.A(123);
        Assert.Throws<NullReferenceException>(() => Console.WriteLine(u.B));

        u = Union1.H(123, 456, "qwe", new(), (789, "asd"));
        Console.WriteLine(u.H.ToString());
        Assert.That(u.Tag, Is.EqualTo(Union1.Tags.H));
        Assert.That(u.H.a, Is.EqualTo(123));
        Assert.That(u.H.b, Is.EqualTo(456));
        Assert.That(u.H.c, Is.EqualTo("qwe"));
        Assert.That(u.H.d.GetType(), Is.EqualTo(typeof(HashSet<int>)));
        Assert.That(u.H.e, Is.EqualTo((789, "asd")));
    }


    [Test]
    public void Test2()
    {
        var u = Union2.A(123);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.A));
        Assert.That(u.A, Is.EqualTo(123));

        u = Union2.B("asd");
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.B));
        Assert.That(u.B, Is.EqualTo("asd"));

        u = Union2.C(true);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.C));
        Assert.That(u.C, Is.True);

        u = Union2.D((1, 2));
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.D));
        Assert.That(u.D, Is.EqualTo((1, 2)));

        u = Union2.E;
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.E));

        u = Union2.F(null);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.F));
        Assert.That(u.F, Is.Null);

        var l = new List<int>();
        u = Union2.F(l);
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.F));
        Assert.That(u.F, Is.EqualTo(l));

        u = Union2.G((123, "asd"));
        Assert.That(u.Tag, Is.EqualTo(Union2.Tags.G));
        Assert.That(u.G, Is.EqualTo((123, "asd")));

        u = Union2.A(123);
        Assert.Throws<NullReferenceException>(() => Console.WriteLine(u.B));

        u.A = 456;
        Assert.That(u.A, Is.EqualTo(456));
    }
}
