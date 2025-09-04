using System.Runtime.InteropServices;
using Coplt.Union;

#pragma warning disable CS0219

Console.Write("");

// var f = new FileStruct();
//
// var a = SomeUnmanaged<int>.MakeFoo(123);
// Console.WriteLine(a);
//
// var b = SomeClass<string>.MakeFoo("asd");
// Console.WriteLine(b);
//
// switch (b)
// {
//     case { IsA: true, A: var aa }:
//         Console.WriteLine(aa);
//         break;
//     case { IsFoo: true, Foo: var aa }:
//         Console.WriteLine(aa);
//         break;
// }
//
// //var foo = new Foo<int>();
// //Console.WriteLine(foo);
//
// //var bar = new Bar<string>();
// //Console.WriteLine(bar);
//
// [Union]
// public partial struct SomeUnmanaged<T> where T : unmanaged
// {
//     [UnionTemplate]
//     private interface Template
//     {
//         T Foo();
//         (T a, int b) A();
//         T[] B();
//         List<T> C();
//         int X();
//     }
// }
//
// [Union]
// public partial struct SomeClass<T> where T : class
// {
//     [UnionTemplate]
//     private interface Template
//     {
//         T Foo();
//         (T a, int b) A();
//         T[] B();
//         List<T> C();
//         int X();
//     }
// }
//
// //[StructLayout(LayoutKind.Explicit)]
// //public struct Foo<T> where T : unmanaged
// //{
// //    [FieldOffset(0)]
// //    public T Value;
// //}
//
// //[StructLayout(LayoutKind.Explicit)]
// //public struct Bar<T> where T : class
// //{
// //    [FieldOffset(0)]
// //    public T[] Value;
// //}
//
// file struct FileStruct { }
//
// [Union]
// public readonly partial struct Union1
// {
//     [UnionTemplate]
//     private interface Template
//     {
//         int A();
//         string B();
//         bool C();
//         (int a, int b) D();
//         void E();
//         List<int>? F();
//         (int a, string b) G();
//     }
// }
//
//
// [Union]
// public partial struct Option<T>
// {
//     [UnionTemplate]
//     private interface Template
//     {
//         T Some();
//         void None();
//     }
// }
//
// [Union]
// public partial struct Result<T, E>
// {
//     [UnionTemplate]
//     private interface Template
//     {
//         T Ok();
//         E Err();
//     }
// }
//
// public struct TestExtension1
// {
//     // public int A => 1;
//
//     // public struct A(int A);
// }
//
// public static class TestExtension1Ex
// {
//     extension(TestExtension1 a)
//     {
//         public static TestExtension1 A() => new();
//     }
//
//     public static void Foo(TestExtension1 a)
//     {
//         // var b = a.A;
//         // var c = TestExtension1.A();
//     }
// }
//
// public static class TestExtension1Ex2
// {
//     extension(TestExtension1 a)
//     {
//         public TestExtension1 A => new();
//     }
// }
//
// [Union2]
//  public readonly partial struct Union2
//  {
//      [UnionTemplate]
//      private interface Template
//      {
//          int A();
//          string B();
//          bool C();
//          (int a, int b) D();
//          void E();
//          List<int>? F();
//          void G(int a, string b);
//      }
//  }
//  
//  public static class Test
//  {
//      public static void Foo()
//      {
//          var a = Union2.A(1);
//          var b = a.A;
//      }
//  }

[Union2]
public readonly partial struct TestUnion<T> where T : unmanaged
{
    [UnionTemplate]
    private interface Template
    {
        void Foo(int a, T b);
    }
}
