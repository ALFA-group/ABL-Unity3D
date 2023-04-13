using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;

namespace Linefy {

    [System.Serializable]
    public struct RangeFloat  {
        public float from;
        public float to;
 
        public RangeFloat (float from, float to) {
            this.from = from;
            this.to = to;
        }

        public bool InRange(float t ) {
            if (t >= from && t <= to) {
                return true;
            } else {
                return false;
            }
        }

        public bool InRange(float t, ref float lv) {
            if (InRange(t)) {
                lv = Mathf.InverseLerp(from, to, t);
                return true;
            } else {
                return false;
            }
        }
    }

    [System.Serializable]
    public struct Label {
        public string text;
        public Vector3 position;
        public Vector2 offset;

        public Label(string text, Vector3 position, Vector2 offset) {
            this.text = text;
            this.position = position;
            this.offset = offset;
        }

        public Label(string text, Vector3 position ) {
            this.text = text;
            this.position = position;
            this.offset = Vector2.zero;
        }

        public Label(string text ) {
            this.text = text;
            this.position = Vector3.zero;
            this.offset = Vector2.zero;
        }
    }

    /// <summary>
    /// Algorithm for calculating the Width
    /// </summary>
    public enum WidthMode {
        /// <summary>
        /// Billboarded orientation, constant onscreen Width measured in pixels, perspective distortions are ignored.
        /// </summary>
        PixelsBillboard,
        /// <summary>
        /// Billboarded orientation, Width measured in world units, respects an perspective distortion.
        /// </summary>
        WorldspaceBillboard,
        /// <summary>
        /// Billboarded orientation, constant onscreen Width measured in percents of Screen.height , perspective distortions are ignored.
        /// </summary>
        PercentOfScreenHeight,
        /// <summary>
        ///  Width is measured in world units, surface oriented on Z axis (lying on XY plane). Respects perspective distortion and matrix orientation.  
        /// </summary>
        WorldspaceXY
    }
 
    /// <summary>
    /// Depricated!
    /// </summary>
 
    [System.Serializable]
    [System.Obsolete("VisualPropertiesBlock is depricated. Use classes from Linefy.Serialization namespace")]
    public struct VisualPropertiesBlock {
        public bool transparent;
        public float widthMuliplier;
        public WidthMode widthMode;
        public Color colorMuliplier;
        public float feather;
        public int renderOrder;
        public UnityEngine.Rendering.CompareFunction zTest;
        public float viewOffset;
        public float depthOffset;
        public Texture texture;

        public VisualPropertiesBlock( float width, Color color, bool transparent ) {
            this.transparent = transparent;
            this.widthMuliplier = width;
            this.colorMuliplier = color;
            this.feather = 1;
            this.renderOrder = 0;
            this.viewOffset = 0;
            this.depthOffset = 0;
            this.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            this.widthMode = WidthMode.PixelsBillboard;
            this.texture = null;
        }

        /// <summary>
        /// transparent ctor
        /// </summary>
         public VisualPropertiesBlock(float width, Color color, float feather ) {
            this.transparent = true;
            this.widthMuliplier = width;
            this.colorMuliplier = color;
            this.feather = feather;
            this.renderOrder = 0;
            this.viewOffset = 0;
            this.depthOffset = 0;
            this.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            this.widthMode = WidthMode.PixelsBillboard;
            this.texture = null;

        }

        /// <summary>
        /// transparent ctor
        /// </summary>
        public VisualPropertiesBlock(float width, Color color, float feather, int renderOrder) {
            this.transparent = true;
            this.widthMuliplier = width;
            this.colorMuliplier = color;
            this.feather = feather;
            this.renderOrder = renderOrder;
            this.viewOffset = 0;
            this.depthOffset = 0;
            this.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            this.widthMode = WidthMode.PixelsBillboard;
            this.texture = null;
        }

        /// <summary>
        /// opaque ctor
        /// </summary>
        public VisualPropertiesBlock(float width, Color color) {
            this.transparent = false;
            this.widthMuliplier = width;
            this.colorMuliplier = color;
            this.feather = 1;
            this.renderOrder = 0;
            this.viewOffset = 0;
            this.depthOffset = 0;
            this.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            this.widthMode = WidthMode.PixelsBillboard;
            this.texture = null;
        }
    }

    /// <summary>
    /// Lighting data calculation mode  
    /// </summary>
    public enum LightingMode {
        /// <summary>
        /// no lighting data
        /// </summary>
        Unlit,
        /// <summary>
        ///  mesh normals
        /// </summary>
        Lit,
        /// <summary>
        /// mesh normals and tangents
        /// </summary>
        NormalMapped
    }
 
    /// <summary>
    /// Mesh normals recalculation mode
    /// </summary>
    public enum NormalsRecalculationMode {
        Unweighted,
        Weighted
    }
 
    public enum SmoothingGroupsImportMode {
        FromSource,
        PerPolygon,
        ForceSmoothAll 
    }

    [System.Serializable]
    public struct ModificationInfo {
        public string name;
        public string date;
        public int hash;

        public ModificationInfo( string name ) {
            this.name = name;
            date = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            hash = (date + name).GetHashCode(); 
        }
    }

    [System.Serializable]
    public struct Line {
        public Vector3 positionA;
        public Vector3 positionB;
        public float widthA;
        public float widthB;
        public Color colorA;
        public Color colorB;
        public float textureOffsetA;
        public float textureOffsetB;


        public Line(Vector3 a, Vector3 b, Color ca, Color cb, float widthA, float widthB ) {
            positionA = a;
            positionB = b;
            colorA = ca;
            colorB = cb;
            this.widthA = widthA;
            this.widthB = widthB;
            this.textureOffsetA = 0;
            this.textureOffsetB = 1;
        }

        public Line(Vector3 a, Vector3 b, Color ca, Color cb, float widthA, float widthB, float textureOffsetA, float textureOffsetB) {
            positionA = a;
            positionB = b;
            colorA = ca;
            colorB = cb;
            this.widthA = widthA;
            this.widthB = widthB;
            this.textureOffsetA = textureOffsetA;
            this.textureOffsetB = textureOffsetB;
        }

        public Line(Vector3 a, Vector3 b, Color ca, Color cb, float width) {
            positionA = a;
            positionB = b;
            colorA = ca;
            colorB = cb;
            widthA = width;
            widthB = width;
            textureOffsetA = 0;
            textureOffsetB = 1;
        }

        public Line(Vector3 a, Vector3 b, Color color ) {
            positionA = a;
            positionB = b;
            colorA = color;
            colorB = color;
            widthA = 1;
            widthB = 1;
            textureOffsetA = 0;
            textureOffsetB = 1;
        }

        public Line(Vector3 a, Vector3 b, Color color, float width) {
            positionA = a;
            positionB = b;
            colorA = color;
            colorB = color;
            widthA = width;
            widthB = width;
            textureOffsetA = 0;
            textureOffsetB = 1;
        }

        public Line(Vector3 a, Vector3 b) {
            positionA = a;
            positionB = b;
            colorA = Color.white;
            colorB = Color.white;
            widthA = 1;
            widthB = 1;
            textureOffsetA = 0;
            textureOffsetB = 1;
        }

        public Line(float width, Color color) {
            positionA = Vector3.zero;
            positionB = Vector3.zero;
            widthA = width;
            widthB = width;
            colorA = color;
            colorB = color;
            textureOffsetA = 0;
            textureOffsetB = 1;
        }

        public override string ToString() {
            return string.Format("A:{0} B:{1} widthA:{2} widthB:{3} colorA:{4} colorB:{5}", positionA, positionB, widthA, widthB, colorA, colorB);
        }
    }

    [System.Serializable]
    public struct Dot {
        /// <summary>
        /// Dot visiblity
        /// </summary>
        public bool enabled;

 
        public Vector3 position;
  
        public float size { 
            get {
                return size2d.x;
            }

            set {
                size2d.Set(value, value);
            }
        }

        public Vector2 size2d;

        /// <summary>
        /// Index of rect in DotsAtlas
        /// </summary>
        public int rectIndex;
        public Color color;

        /// <summary>
        /// local 2d offset 
        /// </summary>
        public Vector2 offset;

        public Dot(Vector3 pos, float size, int rectIndex, Color color) {
            this.enabled = true;
            this.position = pos;
            this.size2d = new Vector2( size, size);
            this.color = color;
            this.rectIndex = rectIndex;
            this.offset = Vector2.zero;
        }

        public Dot(Vector3 pos, float size, int rectIndex, Color color, Vector2 pixelOffset) {
            this.enabled = true;
            this.position = pos;
            this.size2d = new Vector2(size, size);
            this.color = color;
            this.rectIndex = rectIndex;
            this.offset = pixelOffset;
        }

        public Dot(Vector3 pos, float size, DefaultDotAtlasShape shape, int outlineWidth, Color color) {
            this.enabled = true;
            this.position = pos;
            this.color = color;
            int _shape = Mathf.Clamp((int)shape, 0, 8);
            outlineWidth = Mathf.Clamp(outlineWidth, 0, 16);
            this.rectIndex =  _shape * 16 + outlineWidth;
            this.size2d = new Vector2(size, size);
            this.offset = Vector2.zero;
        }

        public Dot(Vector3 pos, Vector2 size2d, DefaultDotAtlasShape shape, int outlineWidth, Color color) {
            this.enabled = true;
            this.position = pos;
            this.color = color;
            int _shape = Mathf.Clamp((int)shape, 0, 8);
            outlineWidth = Mathf.Clamp(outlineWidth, 0, 16);
            this.rectIndex = _shape * 16 + outlineWidth;
            this.size2d = size2d;
            this.offset = Vector2.zero;
        }

        public static implicit operator PolylineVertex(Dot d) {
            return new PolylineVertex(d.position, d.color, d.size);
        }

        public override string ToString() {
            return string.Format("enabled:{0} positions:{1} size2d:{2} rectIndex:{3} color:{4} pixelOffset:{5} \n", enabled, position, size2d, rectIndex, color, offset );
        }
    }

    [System.Serializable]  
    public struct PolylineVertex : IVector3GetSet {
 
        public Vector3 position;
        public Color color;
        public float width;
        public float textureOffset;

        public PolylineVertex(Vector3 pos, Color color, float width, float textureOffset) {
            this.position = pos;
            this.color = color;
            this.width = width;
            this.textureOffset = textureOffset;
        }

        public PolylineVertex(Vector3 pos, Color color, float width) {
            this.position = pos;
            this.color = color;
            this.width = width;
            this.textureOffset = 0;
        }

        public static implicit operator Dot(PolylineVertex pv) {
            return new Dot(pv.position, pv.width, 0, pv.color);
        }

        public override string ToString() {
            return string.Format("pos:{0} col:{1} width:{2} to:{3}", position, color, width, textureOffset);
        }

        public Vector3 vector3 {
            get {
                return position;
            }

            set {
                position = value;
            }
        }

        public PolylineVertex Interpolate(PolylineVertex other, float t) {
            Vector3 _pos = Vector3.LerpUnclamped(position, other.position, t);
            Color _col = Color.LerpUnclamped(color, other.color, t);
            float _width = Mathf.LerpUnclamped(width, other.width, t);
            float _textureOffset = Mathf.LerpUnclamped(textureOffset, other.textureOffset, t);
            return new PolylineVertex(_pos, _col, _width, _textureOffset) ;
        }
    }
 

    [System.Serializable]
    public enum DefaultDotAtlasShape {
        Round,
        Hexagon,
        Quad,
        Rhombus,
        RoundOutline,
        HexagonOutline,
        QuadOutline,
        RhombusOutline,
    }
 
    [System.Serializable]
    public struct Polygon {
        public int smoothingGroup;
        public int materialId;

        public PolygonCorner[] corners;

        public Polygon(int sg, int materialId, int cornersCount) {
            this.smoothingGroup = sg;
            this.materialId = materialId;
            corners = new PolygonCorner[cornersCount];
        }

        [System.Obsolete(" Obsolete constructor , use Polygon(int sg, int materialId, int cornersCount) instead ")]
        public Polygon(int sg, string str, int cornersCount) {
            this.smoothingGroup = sg;
            this.materialId = 0;
            corners = new PolygonCorner[cornersCount];
        }

        public Polygon( int cornersCount ) {
            smoothingGroup = 0;
            materialId = 0;
            corners = new PolygonCorner[cornersCount];
        }

        public Polygon( params PolygonCorner[] corners) {
            smoothingGroup = 0;
            materialId = 0;
            this.corners = corners;
        }

        public Polygon(int sg, int materialId, params PolygonCorner[] corners) {
            this.smoothingGroup = sg;
            this.materialId = materialId;
            this.corners = corners;
        }

        public PolygonCorner this[int idx] { 
            get {
                return corners[idx];
            }

            set {
                corners[idx] = value;
            }
        }

        public void SetCorner(int idx, int pos, int uv, int color) {
            corners[idx] = new PolygonCorner(pos, uv, color);
        }    

        public int CornersCount { 
            get {
                return corners.Length;
            }
        }

        public bool isValid {
            get {
                if (corners.Length < 3) {
                    return false;
                }
                for (int i = 0; i<corners.Length; i++) {
                    for (int j = 0; j<corners.Length; j++) {
                        if (i == j) {
                            continue;
                        }
                        if (corners[i].position == corners[j].position) {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public void ClampCornerIndices(int posLength, int colorsLength, int uvLength) {
            for (int i = 0; i<corners.Length; i++) {
                corners[i].position = Mathf.Clamp(corners[i].position, 0, posLength);
                corners[i].color = Mathf.Clamp(corners[i].color, 0, colorsLength );
                corners[i].uv = Mathf.Clamp(corners[i].uv, 0, uvLength);
            }
        }

        public override string ToString() {
            string cornersStr = "";
            for (int i = 0; i<corners.Length; i++) {
                cornersStr += corners[i].ToString();
            }
            return string.Format("sg:{0} mat:{1} corners:{2}", smoothingGroup, materialId, cornersStr);
        }
    }

    [System.Serializable]
    public struct PolygonCorner {
        public int position;
        public int uv;
        public int color;

        public PolygonCorner(int position, int uv, int color) {
            this.position = position;
            this.uv = uv;
            this.color = color;
        }

        public override string ToString() {
            return string.Format(" <{0},{1},{2}> ", position, uv, color);
        }
    }

    public interface IVector3GetSet {
         Vector3 vector3 { get; set; }
    }

    [System.Serializable]
    public class EditorIconContent {
        public string tooltip;
        [SerializeField]
        public Texture2D brightTexture;
        [SerializeField]
        public Texture2D darkTexture;

        GUIContent _bright;
        public GUIContent bright {
            get {
                if (_bright == null) {
                    _bright = new GUIContent(brightTexture, tooltip);
                }
                _bright.image = brightTexture;
                _bright.tooltip = tooltip;
                return _bright;
            }
        }

        GUIContent _dark;
        public GUIContent dark {
            get {
                if (_dark == null) {
                    _dark = new GUIContent(darkTexture, tooltip);
                }
                _dark.image = darkTexture;
                _dark.tooltip = tooltip;
                return _dark;
            }
        }
    }

}
