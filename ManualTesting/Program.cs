﻿using System.Runtime.InteropServices;
using Sera.TaggedUnion;

var a = SomeUnmanaged<int>.MakeFoo(123);
Console.WriteLine(a);

var b = SomeClass<string>.MakeFoo("asd");
Console.WriteLine(b);

//var foo = new Foo<int>();
//Console.WriteLine(foo);


[Union]
public partial struct SomeUnmanaged<T> where T : unmanaged
{
    [UnionTemplate]
    private abstract class Template
    {
        public abstract T Foo();
    }
}

[Union]
public partial struct SomeClass<T> where T : class
{
    [UnionTemplate]
    private abstract class Template
    {
        public abstract T Foo();
    }
}

//[StructLayout(LayoutKind.Explicit)]
//public struct Foo<T> where T : unmanaged
//{
//    [FieldOffset(0)]
//    public T Value;
//}

//[StructLayout(LayoutKind.Explicit)]
//public struct Bar<T> where T : class
//{
//    [FieldOffset(0)]
//    public T Value;
//}
