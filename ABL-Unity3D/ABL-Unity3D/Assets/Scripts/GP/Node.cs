using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;
using Object = System.Object;

#nullable enable

namespace GP
{
    public class NodeComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node? node1, Node? node2)
        {
            return node1?.Equals(node2) ?? null == node2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <remarks>
        /// Ideally we would leave getting the hashcode of the node up to an overridden method in the node class/a node subclass,
        /// but this won't work because the node symbol and children cannot be readonly as currently written. In the future, maybe we will
        /// refactor the node class to take the node symbol as a parameter to the constructor, and rewrite Node.DeepCopy. For now, this is
        /// simpler.
        /// </remarks>
        public int GetHashCode(Node? node)
        {
            if (null == node) return 0;

            var children = node.children;
            if (!node.DoesChildrenOrderMatter)
            {
                children = children.OrderBy(n => n.symbol).ToList();
            }

            var childrenHashCodes = children.Select(c => new NodeComparer().GetHashCode(c));
            var childrenHashCode = GenericUtilities.CombineHashCodes(childrenHashCodes);
            
            return GenericUtilities.CombineHashCodes(
                new[]
                {
                    node.symbol.GetHashCode(),
                    childrenHashCode,
                    node.returnType.GetHashCode()
                }
            );
        }
    }

    
    /// <remarks>
    /// Ideally we would make Node.GetHashCode and Node.Equals a virtual method, so that each Node subclass can define it's own method.
    /// But this cannot be done as currently written because the node symbol and children cannot be readonly as currently written.
    /// Thus, we include the field <see cref="Node.DoesChildrenOrderMatter"/> to allow for one special method. 
    /// In the future, maybe we will
    /// refactor the node class to take the node symbol as a parameter to the constructor, and rewrite Node.DeepCopy. For now, this is
    /// simpler. 
    /// </remarks>
    [HideReferenceObjectPicker]
    public class Node 
    {
        
        public readonly Type returnType;

        [HideIf("@GpRunner.IsTerminal(this.GetType())")]
        
        public List<Node> children;
        
        public string symbol;

        public virtual bool DoesChildrenOrderMatter => true;

        [JsonConstructor]
        public Node(Type returnType, List<Node> children) 
        {
            this.symbol = this.GetType().Name;
            this.children = children;
            this.returnType = returnType;
        }

        public Node(Type returnType) 
        {
            this.symbol = this.GetType().Name;
            this.children = new List<Node>();
            this.returnType = returnType;
        }

        public bool Equals(Node? otherNode)
        {
            if (null == otherNode) return false;
            
            var myNodes = this.IterateNodes();
            var theirNodes = otherNode.IterateNodes();
            if (!this.DoesChildrenOrderMatter)
            {
                myNodes = myNodes.OrderBy(n => n.symbol);
                theirNodes = theirNodes.OrderBy(n => n.symbol);
            }
            var myNodeAsArray = myNodes as Node[] ?? myNodes.ToArray();
            var theirNodesAsArray = theirNodes as Node[] ?? theirNodes.ToArray();
            
            if (myNodeAsArray.Length != theirNodesAsArray.Length) return false;

            for (var i = 0; i < myNodeAsArray.Length; i++)
            {
                var myNode = myNodeAsArray[i];
                var theirNode = theirNodesAsArray[i];
                if (!myNode.ShallowEquals(theirNode)) return false;
            }
            return true;
        }
 
        public bool ShallowEquals(Node? root)
        {
            if (null == root) return false;
            return this.symbol == root.symbol && this.returnType == root.returnType;
        }

        public IEnumerable<Node> IterateTerminals()
        {
            if (this.children.Count == 0) yield return this;

            foreach (var child1 in this.children)
            foreach (var child2 in child1.IterateTerminals())
                yield return child2;
        }

        public IEnumerable<Node> IterateNodes()
        {
            yield return this;

            foreach (var child1 in this.children)
            foreach (var child2 in child1.IterateNodes())
                yield return child2;
        }

        private IEnumerable<NodeWrapper> IterateNodeWrappersHelper()
        {
            for (var i = 0; i < this.children.Count; i++)
            {
                var child = this.children[i];
                yield return new NodeWrapper(this, child, i);

                foreach (var childNodeWrapper in child.IterateNodeWrappersHelper()) yield return childNodeWrapper;
            }
        }

        public IEnumerable<NodeWrapper> IterateNodeWrappers()
        {
            yield return new NodeWrapper(this);

            foreach (var wrapper in this.IterateNodeWrappersHelper()) yield return wrapper;
        }

        // Skip 1 because we include the root node in IterateNodeWrappers
        // And the root is always a placeholder.
        public IEnumerable<NodeWrapper> IterateNodeWrapperWithoutRoot()
        {
            return this.IterateNodeWrappers().Skip(1);
        }

        public Node DeepCopy()
        {
            var clone = (Node)this.MemberwiseClone();
            clone.children = this.children.ToList();
            for (var i = 0; i < clone.children.Count; i++) clone.children[i] = this.children[i].DeepCopy();
            return clone;
        }

        public IEnumerable<int> GetSymTypeAndFilterLocationsInDescendants(Type descendantReturnType,
            List<FilterAttribute> filters)
        {
            var currentLocation = 1;

            foreach (var descendant in this.IterateNodes().Skip(1))
            {
                if (descendant.returnType == descendantReturnType &&
                    GpRunner.GetFilterAttributes(descendant.GetType()).SequenceEqual(filters))
                    yield return currentLocation;

                currentLocation++;
            }
        }

        public Node GetNodeAtIndex(int goalIndex)
        {
            var node = this.IterateNodes().Skip(goalIndex).FirstOrDefault() ??
                       throw new ArgumentOutOfRangeException(nameof(goalIndex));
            return node;
        }

        public NodeWrapper GetNodeWrapperAtIndex(int goalIndex)
        {
            var node = this.IterateNodeWrappers().Skip(goalIndex).FirstOrDefault() ??
                       throw new ArgumentOutOfRangeException(nameof(goalIndex));
            return node;
        }

        public int GetHeight()
        {
            if (this.children.Count == 0) return 0; // Leaf node has height 0
            return this.children.Max(child => child.GetHeight()) + 1;
        }

        public int GetDepthOfNodeAtIndex(int goalIndex)
        {
            var node = this.GetNodeAtIndex(goalIndex);
            return this.GetDepthOfNode(node);
        }

        public int GetDepthOfNode(Node node)
        {
            return this.GetHeight() - node.GetHeight();
        }

        public void PrintAsList(string prefix = "")
        {
            CustomPrinter.PrintLine($"{prefix}{this.ToStringInListForm()}");
        }

        public string ToStringInListForm()
        {
            var result = new StringBuilder(this.symbol);
            if (this.children.Count > 0)
            {
                result.Append("(");
                result.Append(string.Join(", ", this.children.Select(child => child.ToStringInListForm())));
                result.Append(")");
            }

            return result.ToString();
        }

        public virtual Node Mutate(GpRunner gp, int maxDepth)
        {
            bool fullyGrow = gp.rand.NextBool(); 
            var filters = GpRunner.GetFilterAttributes(this.GetType());
            var returnTypeSpecification = new ReturnTypeSpecification(this.returnType, filters);
            return gp.GenerateRandomTreeOfReturnType(returnTypeSpecification, maxDepth, fullyGrow);
        }
    }
}