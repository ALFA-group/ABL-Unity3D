using System.Collections.Generic;
using System.Linq;
using GP;
using Planner;
using UnityEngine;

#nullable enable

namespace UI.Trees
{
    public class VisualTree
    {
        public readonly List<VisualTree> children;
        public readonly Node? gpTree;
        
        public readonly Dictionary<Method, Decomposition>? methodDecompositions;
        public readonly string sym;
        public Vector2 center;

        public VisualTree(string symbol, List<VisualTree> children, Node gpTree)
        {
            this.sym = symbol;
            this.children = children;
            this.gpTree = gpTree;
        }

        public VisualTree(string symbol, List<VisualTree> children,
            Dictionary<Method, Decomposition> methodDecompositions)
        {
            this.sym = symbol;
            this.children = children;
            this.methodDecompositions = methodDecompositions;
        }

        public static VisualTree GpTreeToVisualTree(Node tree)
        {
            var genericChildren = tree.children.Select(GpTreeToVisualTree).ToList();
            return new VisualTree(tree.symbol, genericChildren, tree);
        }

        public static VisualTree? MethodDecompositionDictionaryToVisualTree(Plan p)
        {
            return MethodDecompositionDictionaryToVisualTreeHelper(p.dMethodToDecomposition, p.topTask);
        }

        private static VisualTree? MethodDecompositionDictionaryToVisualTreeHelper(
            Dictionary<Method, Decomposition> dict, Method? method)
        {
            if (null == method) return null;

            string genericSym = method.notes + " " + method;
            var genericChildren = new List<VisualTree>();

            if (dict.TryGetValue(method, out var d))
            {
                genericSym += d.mode;

                foreach (var subtask in d.subtasks)
                {
                    var newChild = MethodDecompositionDictionaryToVisualTreeHelper(dict, subtask);
                    if (newChild != null) genericChildren.Add(newChild);
                }
            }
            else
            {
                return new VisualTree(genericSym, genericChildren, dict);
            }

            // Skip the entries with only a single linear child
            return genericChildren.Count == 1 ? genericChildren[0] : new VisualTree(genericSym, genericChildren, dict);
        }
    }
}