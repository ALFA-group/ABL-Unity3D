#nullable enable

namespace GP
{
    public class NodeWrapper
    {
        private readonly int? _childIndex;
        public readonly Node? parent;
        public Node child;

        public NodeWrapper(Node? parent, Node child, int? childIndex)
        {
            this.parent = parent;
            this.child = child;
            this._childIndex = childIndex;
        }

        public NodeWrapper(Node child)
        {
            this.child = child;
        }

        public void ReplaceWith(Node newChild)
        {
            if (this.parent != null && this._childIndex != null) this.parent.children[(int)this._childIndex] = newChild;
            this.child = newChild;
        }
    }
}