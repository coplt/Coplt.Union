using System;
using Coplt.Union;
using Coplt.Union.Misc;

namespace Tests;

[Union, UnionSymbol(IsReferenceType = MayBool.True)]
public partial class Tree
{
    [UnionTemplate]
    private interface Template
    {
        void Tip();
        void Node(int Value, Tree Left, Tree Right);
    }

    public static int Sum(Tree Tree) => Tree switch
    {
        { IsNode: true, Node: var (value, left, right) } => value + Sum(left) + Sum(right),
        _ => 0,
    };

    public static int Sum2(Tree Tree) => Tree switch
    {
        { IsNode: true, Node.Value: var value, Node.Left: var left, Node.Right: var right } =>
            value + Sum(left) + Sum(right),
        _ => 0,
    };
}
