using System.Numerics;
using Coplt.Union;

namespace Tests;

[Union]
public partial struct Shape
{
    [UnionTemplate]
    private interface Template
    {
        void Rectangle(float Width, float Length);
        void Circle(float Radius);
        void Prism(float Width1, float Width2, float Height);
    }
}

public static class ShapeFoo
{
    public static void Foo()
    {
        var rect = Shape.MakeRectangle(Length: 1.3f, Width: 1.3f);
        var circ = Shape.MakeCircle(1.0f);
        var prism = Shape.MakePrism(5f, 2f, Height: 3f);
    }
    public static void Foo(Shape shape)
    {
        switch (shape)
        {
            case { IsRectangle: true, Rectangle.Width: var width }:
                break;
            case { IsCircle: true, Circle.Radius: var radius }:
                break;
            case { IsPrism: true, Prism.Width1: var width }:
                break;
        }
    }
}
