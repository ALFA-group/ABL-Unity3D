using System;
using System.Linq;
using GP;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Layered;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
// GraphView

// Microsoft Automatic Graph Layout
using MsGraph = Microsoft.Msagl.Core.Layout.GeometryGraph;
using MsNode = Microsoft.Msagl.Core.Layout.Node;
using MsEdge = Microsoft.Msagl.Core.Layout.Edge;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace UI.Trees.Editor
{
    public class TreeGraphView : GraphView
    {
        private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);

        public TreeGraphView(VisualTree tree)
        {
            this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());

            SetLayout(tree);
            this.ConstructFromGpTree(tree);
        }

        private static MsNode GetNode(VisualTree node)
        {
            var rect = new Rectangle(0, 0, (node.sym.Length + 40) * 5,
                80 + node.children.Count * 60); 
            var roundedRect = new RoundedRect(rect, 0.5, 0.5);
            return new MsNode(roundedRect, node);
        }

        private static void SetLayout(VisualTree tree)
        {
            var graph = new MsGraph();
            var root = GetNode(tree);
            graph.Nodes.Add(root);
            FillMsGraph(tree, graph, root);

            var settings = new SugiyamaLayoutSettings
            {
                Transformation = PlaneTransformation.Rotation(Math.PI / 2)
            };
            var layout = new LayeredLayout(graph, settings);
            layout.Run();

            foreach (var node in graph.Nodes)
                ((VisualTree)node.UserData).center = new Vector2((float)node.Center.X, (float)node.Center.Y);
        }

        private static void FillMsGraph(VisualTree tree, MsGraph graph, MsNode parent)
        {
            // We reverse here because we transform the graph
            // by 90 degrees so the orientation of the nodes gets messed up.
            
            var reverse = tree.children.ToList();
            reverse.Reverse();

            foreach (var child in reverse)
            {
                var childNode = GetNode(child);
                graph.Nodes.Add(childNode);

                var edge = new MsEdge(parent, childNode);
                graph.Edges.Add(edge);

                FillMsGraph(child, graph, childNode);
            }
        }

        private Node ConstructFromGpTree(VisualTree tree, bool root = true)
        {
            var node = this.CreateTreeNode(tree.sym);
            if (root) node.inputContainer.RemoveAt(0); // Root node has no input

            for (var i = 1; i <= tree.children.Count; i++)
            {
                // Create a new output port
                var outputPort = GeneratePort(node, Direction.Output);

                if (null != tree.gpTree)
                    // i - 1 because 1 <= i <= tree.children.Count, not 0 < i < tree.children.Count
                    outputPort.portName = GpRunner.GetChildPropertyNameAtChildrenIndex(i - 1, tree.gpTree);

                node.outputContainer.Add(outputPort);

                // Generate the child at the correct position
                var child = tree.children[i - 1];
                var childNode = this.ConstructFromGpTree(child, false);
                childNode.SetPosition(new Rect(child.center, this._defaultNodeSize));

                // Get the child's input
                var inputPort = (Port)childNode.inputContainer.ElementAt(0);
                inputPort.portName = "Input";

                // Connect the nodes
                var edge = outputPort.ConnectTo(inputPort);

                // Add to view
                this.AddElement(childNode);
                this.AddElement(edge);
            }

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(tree.center, this._defaultNodeSize));
            this.AddElement(node);

            return node;
        }

        private static Port GeneratePort(Node node, Direction portDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(
                Orientation.Horizontal,
                portDirection,
                capacity,
                typeof(float)); // Float is an arbitrary type because we don't pass values between nodes
        }

        private Node CreateTreeNode(string nodeName)
        {
            var node = new Node { title = nodeName };

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(Vector2.zero, this._defaultNodeSize));

            return node;
        }
    }
}