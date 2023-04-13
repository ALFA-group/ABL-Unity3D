using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions.Helpers;
using Linefy;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.Unity;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public class LabelDrawer
    {
        private readonly LabelsRenderer _labelsRenderer;
        private int _numUsedLabels;

        public LabelDrawer(Color color, float textSize, bool drawBackground, int maxNumLabels = 20)
        {
            this._labelsRenderer = new LabelsRenderer(maxNumLabels)
            {
                drawBackground = drawBackground,
                textColor = color,
                backgroundColor = Color.Lerp(color, Color.black, 0.7f),
                size = textSize
            };
        }

        public void ResetNumUsedLabels()
        {
            this._numUsedLabels = 0;
        }

        public void HideAllUnusedLabels()
        {
            for (int i = this._numUsedLabels; i < this._labelsRenderer.count; ++i)
                // Hide all unused labels
                this._labelsRenderer.SetText(i, string.Empty);
        }

        public void Draw(Matrix4x4 transformMatrix)
        {
            this._labelsRenderer.Draw(transformMatrix);
        }

        public void AddLabel(Vector3 point, string text)
        {
            if (this._labelsRenderer.count <= this._numUsedLabels) this._labelsRenderer.count++;

            this._labelsRenderer[this._numUsedLabels] = new Label(text, point);
            ++this._numUsedLabels;
        }
    }

    public class PolylineDrawer
    {
        private readonly int _lineWidth;

        private readonly Polyline _polyline;

        private readonly List<(Vector3, Vector3)> _segments = new List<(Vector3, Vector3)>();
        private int _numUsedPolylineVertices;

        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        public PolylineDrawer(int lineWidth = 2, int maxNumLines = 500, Texture2D? texture = null,
            float textureOffset = 4f, float textureScale = 0.4f,
            bool autoTextureOffset = false)
        {
            this._polyline = new Polyline(maxNumLines)
            {
                transparent = true,
                widthMultiplier = 2,
                isClosed = false
            };

            this._lineWidth = lineWidth;

            if (null == texture) return;

            this._polyline.texture = texture;
            this._polyline.textureOffset = textureOffset;
            this._polyline.textureOffset = textureScale;
            this._polyline.autoTextureOffset = autoTextureOffset;
        }

        public void ResetNumPolylineVertices()
        {
            this._numUsedPolylineVertices = 0;
            this._segments.Clear();
        }

        public void HideAllUnusedLines()
        {
            for (int i = this._numUsedPolylineVertices; i < this._polyline.count; i++)
            {
                // Hide all unused vertices.
                this._polyline.SetWidth(i, 0);
            }

            this._segments.Clear();
        }

        public void Draw(Matrix4x4 transformMatrix)
        {
            this._polyline.Draw(transformMatrix);
        }

        private void AddPolylineVertex(Vector3 point, Color color, int width)
        {
            if (this._polyline.count <= this._numUsedPolylineVertices) this._polyline.count++;

            this._polyline[this._numUsedPolylineVertices] = new PolylineVertex(point, color, width);
            ++this._numUsedPolylineVertices;
        }

        /// <summary>
        ///     Starts a new line at the specified point.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        public void StartDrawingLine(Vector3 point, Color color, int width)
        {
            if (this._numUsedPolylineVertices > 0)
            {
                // Need to draw a width zero segment to the new start.
                var lastPoint = this._polyline.GetPosition(this._numUsedPolylineVertices - 1);
                this.AddPolylineVertex(lastPoint, Color.black, 0);
                this.AddPolylineVertex(point, Color.black, 0);
            }

            // And then actually start the new line.
            this.AddPolylineVertex(point, color, width);
        }

        /// <summary>
        ///     Continues an existing line.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        private void ContinueDrawingLine(Vector3 point, Color color, int width)
        {
            this.AddPolylineVertex(point, color, width);
        }


        public void DrawPath(Color color, IEnumerable<Vector3> unityPoints)
        {
            Vector3? firstPoint = null;
            Vector3? previousPoint = null;
            foreach (var point in unityPoints)
            {
                if (!firstPoint.HasValue)
                {
                    firstPoint = point;
                    this.StartDrawingLine(point, color, this._lineWidth);
                }
                else
                {
                    int width = this._lineWidth;
                    if (previousPoint != null && this._segments.Contains(((Vector3)previousPoint, point))) width = 0;

                    this.ContinueDrawingLine(point, color, width);

                    if (previousPoint != null) this._segments.Add(((Vector3)previousPoint, point));
                }

                previousPoint = point;
            }
        }
    }
    
    public class ShowIntent : MonoBehaviour, IIntentDrawer
    {
        [MinValue(0)]
        public float extraHeightAboveTerrain = 0.1f;
        [MinValue(0)]
        public float textSize = 2f;
        [MinValue(0)]
        public int circleLineWidth = 2;
        [MinValue(0)]
        public int arrowWidth = 9;
        public bool drawTextBackground = true;

        public Texture2D? arrowTexture;
        public float textureScale = 0.5f;
        public float textureOffset = 4f;
        public bool autoTextureOffset;

        [NonSerialized] private LabelDrawer? _blueLabelDrawer;
        private int _layerMask;

        [NonSerialized] private PolylineDrawer? _polylineArrow;

        [NonSerialized] private PolylineDrawer? _polylineForCircle;

        [NonSerialized] private LabelDrawer? _redLabelDrawer;

        [NonSerialized] private RefSimWorldState? _stateRef;

        private Vector3 SimToUnityCoordsHelper(Vector2 v)   
        {
            return GeneralUnityUtilities.SimToUnityCoords(
                v, this.extraHeightAboveTerrain, this._layerMask);
        }

        private void Awake()
        {
            this._layerMask = LayerMask.GetMask("TerrainVisuals");
            if (null == this.arrowTexture) throw new Exception("Arrow texture must be defined");
            this._polylineArrow = new PolylineDrawer(this.arrowWidth, texture: this.arrowTexture, textureOffset: this.textureOffset,
                textureScale: this.textureScale, autoTextureOffset: this.autoTextureOffset);
            this._polylineForCircle = new PolylineDrawer(this.circleLineWidth);
            this._redLabelDrawer =
                new LabelDrawer(GetPathColor(Team.Red, 0.5f), this.textSize, this.drawTextBackground);
            this._blueLabelDrawer =
                new LabelDrawer(GetPathColor(Team.Blue, 0.5f), this.textSize, this.drawTextBackground);
        }

        private void Update()
        {
            Assert.IsNotNull(this._polylineArrow);
            if (null == this._polylineArrow) return;

            Assert.IsNotNull(this._polylineForCircle);
            if (null == this._polylineForCircle) return;

            Assert.IsNotNull(this._blueLabelDrawer);
            if (null == this._blueLabelDrawer) return;

            Assert.IsNotNull(this._redLabelDrawer);
            if (null == this._redLabelDrawer) return;

            this._redLabelDrawer.ResetNumUsedLabels();
            this._blueLabelDrawer.ResetNumUsedLabels();
            this._polylineArrow.ResetNumPolylineVertices();
            this._polylineForCircle.ResetNumPolylineVertices();

            if (null == this._stateRef) this._stateRef = FindObjectOfType<RefSimWorldState>();

            var state = RefSimWorldState.Fetch(this._stateRef);
            if (null != state)
            {
                var throwawayState = state.DeepCopy();
                throwawayState.actions.DrawIntent(throwawayState, this);
            }

            this._polylineArrow.StartDrawingLine(Vector3.zero, Color.clear, 0);
            this._polylineForCircle.StartDrawingLine(Vector3.zero, Color.clear, 0);

            this._polylineArrow.HideAllUnusedLines();
            this._polylineForCircle.HideAllUnusedLines();

            this._redLabelDrawer.HideAllUnusedLabels();
            this._redLabelDrawer.HideAllUnusedLabels();

            var localToWorldMatrix = this.transform.localToWorldMatrix;
            this._polylineArrow.Draw(localToWorldMatrix);
            this._polylineForCircle.Draw(localToWorldMatrix);
            this._redLabelDrawer.Draw(localToWorldMatrix);
            this._blueLabelDrawer.Draw(localToWorldMatrix);
        }

        public void DrawPath(Team team, IEnumerable<Vector2> pathPoints)
        {
            var color = GetPathColor(team);
            this.DrawPath(color, pathPoints);
        }

        public void DrawCircle(Team team, Circle circle)
        {
            var color = GetPathColor(team, 0.5f);
            var points = circle.ToPolygon(32).ToList();
            points.Add(points[0]);
            if (null == this._polylineForCircle) throw new Exception("Polyline no arrow cannot be null.");
            this._polylineForCircle.DrawPath(color, points.Select(this.SimToUnityCoordsHelper));
        }

        public void DrawText(Team team, Vector2 position, string text)
        {
            Assert.IsNotNull(this._redLabelDrawer);
            if (null == this._redLabelDrawer) return;
            Assert.IsNotNull(this._blueLabelDrawer);
            if (null == this._blueLabelDrawer) return;
            var labelDrawer = team == Team.Blue ? this._blueLabelDrawer : this._redLabelDrawer;

            labelDrawer.AddLabel(new Vector3(position.x, this.extraHeightAboveTerrain, position.y), text);
        }

        public void DrawRect(Rect rect, Team team)
        {
            var color = GetPathColor(team, 0.5f);
            if (null == this._polylineForCircle) throw new Exception("Polyline no arrow cannot be null.");
            this._polylineForCircle.DrawPath(color, rect.EnumerateCorners(true).Select(this.SimToUnityCoordsHelper));
        }

        public void DrawPath(Color color, IEnumerable<Vector2> pathPoints)
        {
            var pathPointsInUnityCoords = pathPoints.Select(this.SimToUnityCoordsHelper);
            if (null == this._polylineArrow) throw new Exception("PolyLine arrow cannot be null");
            this._polylineArrow.DrawPath(color, pathPointsInUnityCoords);
        }

        private static Color GetPathColor(Team team, float whiten)
        {
            var c = GetPathColor(team);
            return Color.Lerp(c, Color.white, whiten);
        }

        private static Color GetPathColor(Team team)
        {
            return team == Team.Blue ? Color.blue : Color.red;
        }


    }
}