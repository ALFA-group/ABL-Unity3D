using UnityEngine;

namespace UI.Trees
{
    public abstract class TreeHolder : MonoBehaviour, IHasTree
    {
        public VisualTree Tree { get; set; }

        protected abstract void SetTreeHelper();

        [ContextMenu("Set Tree")]
        public void SetTree()
        {
            this.SetTreeHelper();
        }

        [ContextMenu("Clear Tree")]
        public void ClearTree()
        {
            this.Tree = null;
        }
    }
}