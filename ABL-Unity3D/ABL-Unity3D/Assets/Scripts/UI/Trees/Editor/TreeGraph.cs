using System;
using System.Collections.Generic;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Trees.Editor
{
    public class TreeGraph : EditorWindow
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private TreeGraphView _graphView;
        private VisualTree _previouslyDisplayedTree;
        private bool _showGraphBasedOnSelection = true;

        public void Update()
        {
            if (this._showGraphBasedOnSelection) this.UpdateSelectedGraph();
        }

        private void OnDisable()
        {
            this.ClearView();
        }

        private void OnDestroy()
        {
            foreach (var disposable in this._disposables) disposable?.Dispose();
            this._disposables.Clear();
        }

        [MenuItem("Graph/Tree Visualizer")]
        public static bool OpenTreeGraphWindow()
        {
            var window = GetWindow<TreeGraph>();
            window._showGraphBasedOnSelection = true;
            window.titleContent = new GUIContent("Tree Visualizer: Selection");
            return true; // awful hack to get this callable by Odin conditionally
        }

        public static bool OpenNewTreeGraphWindow(string title, VisualTree treeToDisplay)
        {
            Debug.Log($"Creating window {title}");
            var window = CreateInstance<TreeGraph>();
            window._showGraphBasedOnSelection = false;
            window.titleContent = new GUIContent(title);
            window.Show();
            window.SetTree(treeToDisplay);
            return true; // awful hack to get this callable by Odin conditionally
        }

        private void ConstructGraphViewFromGpTree(VisualTree tree)
        {
            this.ClearView();

            if (tree == null) return;

            this._previouslyDisplayedTree = tree;

            this._graphView = new TreeGraphView(tree)
            {
                name = "Tree Graph"
            };

            this._graphView.StretchToParentSize();
            this.rootVisualElement.Add(this._graphView);
        }

        private void ClearView()
        {
            if (this._graphView == null) return;

            this.rootVisualElement.Remove(this._graphView);
            this._graphView = null;
            this._previouslyDisplayedTree = null;
        }

        private void UpdateSelectedGraph()
        {
            var selected = Selection.activeObject as GameObject;
            var hasGpTree = selected ? selected.GetComponent<IHasTree>() : null;
            var newTree = hasGpTree?.Tree;
            if (selected == null || hasGpTree == null || newTree == null)
            {
                this.ClearView();
                return;
            }

            this.SetTree(newTree);
        }

        private void SetTree(VisualTree newTree)
        {
            if (newTree.Equals(this._previouslyDisplayedTree)) return;

            this.ConstructGraphViewFromGpTree(newTree);
            var callLater = Observable.Timer(TimeSpan.FromMilliseconds(100))
                .Subscribe(_ => this._graphView?.FrameAll());
            this._disposables.Add(callLater);
        }
    }
}