using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using UnityEngine.UI;
using Linefy.Internal;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class LinefyDemo_Painting : MonoBehaviour  {

        public Slider ColorSlider;
        public Image ColorSliderThumb;
        public Slider WidthSlider;
        public Slider GridSizeSlider;

        public bool drawSafeRects;
        public Texture2D uiSafeRectTexture;

        [System.Serializable]
        public struct SafeRect {
            public bool bottomAlign;
            public Rect rect;

            public void DrawDebug(Texture2D debugTexture) {

                GUI.DrawTexture(r, debugTexture);
            }

            Rect r { 
                get {
                    Rect r = rect;
                    if (bottomAlign) {
                        r.position = new Vector2(r.position.x, Screen.height - r.height - r.position.y);
                    }
                    return r;
                }
            }

            public bool containsMouse { 
                get {
 
                    return r.Contains(Event.current.mousePosition);
                }
            }
        }
        
        public SafeRect[] safeRects;
        public Color BrushColor;
        List<Stroke> strokes = new List<Stroke>();
 

        public class Stroke {

            public Polyline pl;

            Vector2 latestPos;
            float totalLength = 0;
            Vector2 extrapolatedPoint;
            Color color;

            public Stroke(Vector2 firstPos, Color color) {
                this.latestPos = firstPos;
                this.color = color;
                pl = new Polyline(0);
                pl.transparent = true;
                pl.AddVertex(new PolylineVertex(firstPos, Color.red, 5, 0));
            }

            public void Add(Vector2 pos) {
                if (pl.count >= 2) {
                    pos = Vector2.Lerp(pos, extrapolatedPoint, 0.5f);
                }
                float dist = Vector2.Distance(pos, latestPos);

                if (dist > 3) {
                    totalLength += dist;
                    Vector3 posToAdd = pos;
                    pl.AddVertex(new PolylineVertex(posToAdd, Color.red, 5, totalLength));
                    latestPos = pos;
                    if (pl.count >= 2) {
                        extrapolatedPoint = pl[pl.count - 1].position + pl[pl.count - 1].position - pl[pl.count - 2].position;
                    }
                }
            }

            public void ApplyWidth(float width) {
                float fadeDistance = Mathf.Max( width, 20) * 10;
                for (int i = 0; i < pl.count; i++) {
                    PolylineVertex v = pl[i];
                    pl.feather = width / 5f;
                    float k = Mathf.Clamp01(Mathf.Min(v.textureOffset / fadeDistance, (totalLength - v.textureOffset) / fadeDistance));
                    float nd = Mathf.Sqrt(1f - ((k -= 1f) * k));
                    v.width = nd;
                    v.color = this.color;
                    v.color.a = v.color.a * nd * nd;
                    v.width *= width;
                    pl[i] = v;
                }
            }
        }

        Stroke current;
        GUIStyle labelStyle;
        public Camera cam;

        int xGridLines;
        int yGridLines;

        [Range(0, 1)]
        public float camOffset;

        [Range(0, 4)]
        public float gridWidth = 0.5f;

        [Range(0, 4)]
        public float gridFeather = 0.5f;

        [Range(-5, 5)]
        public float gridViewOffset = 0;

        public Color gridColor;

        public int GridRenderOrder;

        public Lines _grid;
        public Lines grid {
            get {
                if (_grid == null) {
                    _grid = new Lines("Grid", 10, true, 1);
                    _grid.feather = 1;
                    _grid.widthMultiplier = 1;
                }
                return _grid;
            }
        }

        private void OnEnable() {
            labelStyle = new GUIStyle();
            labelStyle.fontSize = 30;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;
            xGridLines = -1;
            yGridLines = -1;
        }

        bool mouseInSafeRect() {
            Event e = Event.current;
            for (int i = 0; i<safeRects.Length; i++) {
                if (safeRects[i].containsMouse ) {
                    return true;
                }
            }
            return false;
        }

        void OnGUI() {
            if (drawSafeRects) {
                for (int i = 0; i < safeRects.Length; i++) {
                    safeRects[i].DrawDebug(uiSafeRectTexture);
 
                }
            }

            if (Application.isPlaying) {
                Event e = Event.current;

                if (e.type == EventType.MouseDown && !mouseInSafeRect() && current == null ) {
                    current = new Stroke(e.mousePosition, BrushColor);
                    current.pl.renderOrder = strokes.Count;
                    e.Use();
                }

                if (e.type == EventType.MouseDrag && current != null) {
                    current.Add(e.mousePosition);
                    e.Use();
                }

                if (e.type == EventType.MouseUp && current != null) {
                    strokes.Add(current);
                    current = null;
                    e.Use();
                }

                if (current != null) {
                    current.ApplyWidth(WidthSlider.value * 20);
                }
            }
        }

        public float gridZOffset = 0;

        void Update() {
            Matrix4x4 clipMatrix = Matrix4x4Utility.NearClipPlaneGUISpaceMatrix(cam, camOffset);

            if (Application.isPlaying) {
                if (current != null) {
                    current.pl.Draw(clipMatrix);
                }

                for (int i = strokes.Count - 1; i >= 0; i--) {
                    strokes[i].pl.Draw(clipMatrix);
                }
            }

            DrawGrid(clipMatrix);

            float v = ColorSlider.value;
            BrushColor = Color.HSVToRGB(v, 1, 1);
            ColorSliderThumb.color = BrushColor;
        }

        void DrawGrid(Matrix4x4 clipMatrix) {
            float gridSize = GridSizeSlider.value;
            int nxGridLines = Mathf.CeilToInt(cam.pixelWidth / gridSize) + 1;
            int nyGridLines = Mathf.CeilToInt(cam.pixelHeight / gridSize) + 1;

            grid.renderOrder = GridRenderOrder;

            if (nxGridLines != xGridLines || nyGridLines != yGridLines) {
                xGridLines = nxGridLines;
                yGridLines = nyGridLines;
                grid.count = xGridLines + yGridLines;
            }

            grid.widthMultiplier = gridWidth;
            grid.colorMultiplier = gridColor;
            grid.feather = gridFeather;
            grid.viewOffset = gridViewOffset;

            for (int x = 0; x < xGridLines; x++) {
                float xPos = x * gridSize;
                Vector2 a = new Vector2(xPos, 0);
                Vector2 b = new Vector2(xPos, cam.pixelHeight);
                grid.SetPosition(x, a, b);
            }

            for (int y = 0; y < yGridLines; y++) {
                float yPos = y * gridSize;
                Vector2 a = new Vector2(0, yPos);
                Vector2 b = new Vector2(cam.pixelWidth, yPos);
                grid.SetPosition(xGridLines + y, a, b);
            }

            grid.Draw(clipMatrix);
        }
 
    }
}
