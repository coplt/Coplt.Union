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

    public static int Sum(Tree Tree)
    {
        switch (Tree.Tag)
        {
            case Tags.Node:
                ref var node = ref Tree.Node;
                return node.Value + Sum(node.Left) + Sum(node.Right);
            default:
                return 0;
        }
    }

    public static int Sum2(Tree Tree) => Tree switch
    {
        // no by ref pattern match
        { IsNode: true, Node: var node } => node.Value + Sum(node.Left) + Sum(node.Right),
        _ => 0,
    };
}
