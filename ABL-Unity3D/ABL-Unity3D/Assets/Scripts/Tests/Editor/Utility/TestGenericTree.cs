using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Utilities.GeneralCSharp;
using Random = System.Random;

#nullable enable

namespace Tests.Editor.Utility
{
    public static class TestGenericTree
    {
        [Test]
        public static void CheckEnumerateAll()
        {
            var expectedNumTreeNodes = 100;

            var tree = CreateTree(5, new RangeInt(1, expectedNumTreeNodes));
            var all = tree.EnumerateSelfAndDescendants().ToList();
            Assert.That(all.Count, Is.EqualTo(expectedNumTreeNodes));
            for (var i = 1; i <= expectedNumTreeNodes; ++i) Assert.That(all.Any(node => node.contents == i));
        }

        [Test]
        public static void CheckEnumerateLeaves()
        {
            var expectedNumTreeNodes = 100;

            var tree = CreateTree(5, new RangeInt(1, expectedNumTreeNodes));
            var all = tree.EnumerateSelfAndDescendants().ToList();
            var leaves = tree.EnumerateAllLeaves().ToList();

            Assert.That(leaves.Count, Is.LessThan(all.Count));

            foreach (var node in leaves) Assert.That(node.NumChildren, Is.EqualTo(0));

            foreach (var node in all.Except(leaves)) Assert.That(node.NumChildren, Is.GreaterThan(0));
        }


        public static GenericTreeNode<int> CreateTree(int maxChildrenPerParent, RangeInt contents)
        {
            Assert.Positive(contents.length);

            var root = new GenericTreeNode<int>(contents.start);
            var r = new Random(1);
            for (int i = contents.start + 1; i < contents.end; ++i) AddToTree(root, maxChildrenPerParent, i, r);

            return root;
        }

        public static void AddToTree(GenericTreeNode<int> tree, int maxChildrenPerParent, int value, Random r)
        {
            if (tree.NumChildren >= maxChildrenPerParent)
            {
                int index = r.Next(tree.NumChildren);
                var child = tree.Children[index];
                AddToTree(child, maxChildrenPerParent, value, r);
            }
            else
            {
                var child = new GenericTreeNode<int>(value);
                tree.AddChild(child);
            }
        }
    }
}