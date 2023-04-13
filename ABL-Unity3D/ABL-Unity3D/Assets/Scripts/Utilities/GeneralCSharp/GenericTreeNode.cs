using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

#nullable enable

namespace Utilities.GeneralCSharp
{
    public class GenericTreeNode<T>
    {
        // Contract is that children and parents always point to each other.
        // this.children contains no duplicates.
        private List<GenericTreeNode<T>>? _children;
        public T contents;

        public GenericTreeNode(T contents)
        {
            this.contents = contents;
        }

        public GenericTreeNode(T contents, GenericTreeNode<T> parent)
        {
            this.contents = contents;
            parent.AddChild(this);
        }

        public GenericTreeNode<T>? Parent { get; private set; }

        public IReadOnlyList<GenericTreeNode<T>> Children => this._children ??= new List<GenericTreeNode<T>>();
        public int NumChildren => this._children?.Count ?? 0;

        public void AddChild(GenericTreeNode<T> child)
        {
            this._children ??= new List<GenericTreeNode<T>>();
            Assert.IsFalse(this._children.Contains(child));

            child.ClearParent();

            child.Parent = this;
            this._children.Add(child);
        }

        public GenericTreeNode<T>? GetChild(int index)
        {
            if (null == this._children) return null;
            if (index < 0) return null;
            if (index >= this._children.Count) return null;

            return this._children[index];
        }

        private void ClearParent()
        {
            if (this.Parent != null)
            {
                this.Parent._children?.Remove(this);
                this.Parent = null;
            }
        }

        public IEnumerable<GenericTreeNode<T>> EnumerateSelfAndDescendants()
        {
            if (null == this._children) return this.ToEnumerable();

            return this.ToEnumerable().Concat(
                this._children.SelectMany(child => child.EnumerateSelfAndDescendants())
            );
        }

        public IEnumerable<GenericTreeNode<T>> EnumerateAllLeaves()
        {
            if (null == this._children || this._children.Count < 1) return this.ToEnumerable();

            return this._children.SelectMany(child => child.EnumerateAllLeaves());
        }
    }
}