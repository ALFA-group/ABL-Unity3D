using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;

namespace Linefy {

    [HelpURL("https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#DotsAtlas")]
    [CreateAssetMenuAttribute(menuName = "Linefy/Dots Atlas" )]
    public class DotsAtlas : ScriptableObject {

        [System.Serializable]
        public struct Apperance {
            [Range(0,1)]
            public float backgroundBrightness ;
            [Range(1, 2)]
            public float labelSize;
 
            public Color labelsColor;
 

            public Apperance(float labelSize) {
                backgroundBrightness = 0.5f;
                this.labelSize = labelSize;
                labelsColor = Color.white;
             }
        }

        [System.Serializable]
        public struct BackgroundIndices {
            public int topLeft;
            public int top;
            public int topRight;
            public int left;
            public int center;
            public int right;
            public int bottomLeft;
            public int bottom;
            public int bottomRight;

            public BackgroundIndices(int atlasRowLength) {
                 topLeft = 0;
                 top = 1;
                 topRight = 2;
                 left = 8;
                 center = 9;
                 right = 10;
                 bottomLeft = 16;
                 bottom = 17;
                 bottomRight = 18;
            }
        }

        [System.Serializable]
        public struct Rect {
            public Vector2 v0;
            public Vector2 v1;
            public Vector2 v2;
            public Vector2 v3;

            /// <summary>
            /// local bounds that describe the actual used pixels 
            /// </summary>
            public RectInt bounds;
        }

        [Tooltip("Atlas texture")]
        public Texture texture;


        [Range(1, 64)]
        public int xCount = 1;
        [Range(1, 64)]
        public int yCount = 1;
        public Vector2Int rectPixelSize = new Vector2Int(16,16);

        public string statisticString;
        public int modificationHash;
        public int fontSettingsHash;

        public bool flipVertical;

        [Tooltip("Visual parameters of the preview area.")]
        public Apperance apperance = new Apperance(1);

        public Rect[] rects;

        [SerializeField]
        bool fontSettingsFoldout;

        [Tooltip("Is atlas used as font?")]
        public bool isFontAtlas;

        [Tooltip("Whitespace width")]
        public int whitespaceWidth = 4;

        [Tooltip("An empty space amount between glyphs.")]
        public int horizontalSpacing = 2;

        [Tooltip("When On, glyph each occupy the same amount (defined with Horizontal Spacing property) of horizontal space.  When off, the  width of visual glyph bounds + Horizontal Spacing will be used for horizontal offset. ")]
        public bool monowidth;

        [Tooltip("Enables the following Remapping Index Table where  Unicode indices binds with Atlas rects. When off, the rect indices is identical to the Unicode decimal value.")]
        public bool enableRemapping;

        public int resetIndexOffset = 32;

        [Tooltip("The custom Unicode indices for each rect.")]
        public int[] remappingIndexTable = new int[0];

        [Tooltip("The rect indices of 9-slice scaling used to draw the background.")]
        public BackgroundIndices background9SliseIndices = new BackgroundIndices(8);

        public Dictionary<int, int> remappingDictionary { 
            get {
                if (_remappingDictionary == null) {
                    buildRemappingDictionary();
                }
                return _remappingDictionary;
            }
        }
        Dictionary<int, int> _remappingDictionary;
 
        private void OnEnable() {
            RecalculateRectsCoordinates();
        }

        void GetUVCoords(int index, ref Vector2 v0, ref Vector2 v1, ref Vector2 v2, ref Vector2 v3) {
            int xPos = index % xCount;
            int yPos = yCount - 1 - index / xCount;

            float xStep = 1f / xCount;
            float yStep = 1f / yCount;

            float minX = xPos * xStep;
            float maxX = xPos * xStep + xStep;

            float minY = yPos * yStep;
            float maxY = yPos * yStep + yStep;

            if (flipVertical) {
                v1.x = minX;
                v1.y = minY;
                v0.x = minX;
                v0.y = maxY;
                v3.x = maxX;
                v3.y = maxY;
                v2.x = maxX;
                v2.y = minY;
            } else {
                v0.x = minX;
                v0.y = minY;
                v1.x = minX;
                v1.y = maxY;
                v2.x = maxX;
                v2.y = maxY;
                v3.x = maxX;
                v3.y = minY;
            }
        }

        /// <summary>
        /// helper method for get rect index from Default Dot Atlas
        /// </summary>
        /// <returns></returns>
        public static int GetDefaultDotAtlasRectIndex(DefaultDotAtlasShape shape, int outlineWidth) {
            int maxShapesCount = System.Enum.GetValues(typeof(DefaultDotAtlasShape)).Length;
            int _shape =  Mathf.Clamp((int)shape, 0, maxShapesCount);
            outlineWidth = Mathf.Clamp(outlineWidth, 0, 16);
            return _shape * 16 + outlineWidth;
        }

        public static int GetSettingsHash(int _xCount, int _yCount, bool _flipVertical ) {
             return string.Format("{0} {1} {2}", _xCount, _yCount, _flipVertical).GetHashCode(); 
        }

        public void RecalculateRectsCoordinates() {
            int rectsCount = xCount * yCount;
            if (rects == null || rects.Length != xCount * yCount) {
                rects = new Rect[rectsCount];
            }
            
            int counter = 0;
            for (int y = 0; y<yCount; y++) {
                for (int x = 0; x<xCount; x++) {
                    Rect ri = rects[counter];
                    GetUVCoords(counter, ref ri.v0, ref ri.v1, ref ri.v2, ref ri.v3);
                    rects[counter] = ri;
                    counter++;
                }
            }
            if (texture != null) {
                rectPixelSize = new Vector2Int(texture.width/xCount, texture.height/yCount);
            }
           
        }

        public string[] ApplyFontSettings() {
            if (texture != null && texture.GetType() == typeof(Texture2D)) {
                if (texture.dimension != UnityEngine.Rendering.TextureDimension.Tex2D) {
                    string[] message = new string[2];
                    message[0] = "Apply font settings failed";
                    message[1] = string.Format("Recalculation of characters bounds requires an Tex2D texture dimension");
                    return message;
                }

                Texture2D tex = ((Texture2D)texture).GetReadableCopy();
                int oneRectWidth = tex.width / xCount;
                int oneRectHeight = tex.height / yCount;
                int indexCounter = 0;
                for (int y = 0; y < yCount; y++) {
                    for (int x = 0; x < xCount; x++) {
                        rects[indexCounter].bounds = GetCharBounds(tex, x * oneRectWidth, (yCount - 1 - y) * oneRectHeight, oneRectWidth, oneRectHeight);
                        indexCounter++;
                    }
                }
                if (enableRemapping) {
                    buildRemappingDictionary();
                }
                fontSettingsHash = Random.Range(int.MinValue, int.MaxValue);
                return null;
            } else {
                if (texture == null) {
                    string[] message = new string[2];
                    message[0] = "Apply font settings failed";
                    message[1] =  "texture is null"  ;
                    return message;
                } else {
                    string[] message = new string[2];
                    message[0] = "Apply font settings failed";
                    message[1] = string.Format("{0} has not supported texture type. Recalculation of characters bounds requires an Texture2D", texture.name);
                    return message;
                }
            }
        }

        RectInt GetCharBounds(Texture2D tex, int xPos, int yPos, int rectWidth, int rectHeight) {
            RectInt result = new RectInt();
            int minX = rectWidth;
            int minY = rectHeight;
            int maxX = 0;
            int maxY = 0;

            for (int y = 0; y < rectHeight; y++) {
                for (int x = 0; x < rectWidth; x++) {
                    Color c = tex.GetPixel(x + xPos, y + yPos);
                    if (c.a > 0.5f) {
                        minX = Mathf.Min(x, minX);
                        minY = Mathf.Min(y, minY);
                        maxX = Mathf.Max(x, maxX);
                        maxY = Mathf.Max(y, maxY);
                    }
                }
            }

            if (minX == rectWidth) {
                result = new RectInt(0, 0, rectWidth, rectHeight);
            } else {
                result.width = maxX - minX + 1;
                result.height = maxY - minY + 1;
                result.position = new Vector2Int(minX, minY);
            }
            return result;
        }

        void buildRemappingDictionary() {
            _remappingDictionary = new Dictionary<int, int>();
            for (int i = 0; i < remappingIndexTable.Length; i++) {
                _remappingDictionary[remappingIndexTable[i]] = i;
             }
        }

        public int GetRectByUtf32(int utf) {
            int result = utf;
            if (enableRemapping) {
                remappingDictionary.TryGetValue(utf, out result);
            }
            return result;
        }

        #region LOAD_FROM_RESOURCES
        public static DotsAtlas DefaultFont11px {
            get {
                string path = "Default Font 11px DotsAtlas/Default Font 11px DotsAtlas";
                DotsAtlas result = Resources.Load<DotsAtlas>(path);
                if (result == null) {
                    Debug.LogWarningFormat("'Linefy/Resources/{0}' asset not founded. Please reinstall Linefy package", path);
                    result = ScriptableObject.CreateInstance<DotsAtlas>();
                }
                return result;
            }
        }

        public static DotsAtlas DefaultFont11pxShadow {
            get {
                string path = "Default Font 11px DotsAtlas Shadow/Default Font 11px DotsAtlas Shadow";
                DotsAtlas result = Resources.Load<DotsAtlas>(path);
                if (result == null) {
                    Debug.LogWarningFormat("'Linefy/Resources/{0}' asset not founded. Please reinstall Linefy package", path);
                    result = ScriptableObject.CreateInstance<DotsAtlas>();
                }
                return result;
            }
        }

        public static DotsAtlas Default {
            get {
                string path = "Default DotsAtlas/Default DotsAtlas";
                DotsAtlas result = Resources.Load<DotsAtlas>(path);
                if (result == null) {
                    Debug.LogWarningFormat("'Linefy/Resources/{0}' asset not founded. Please reinstall Linefy package", path);
                    result = ScriptableObject.CreateInstance<DotsAtlas>();
                }
                return result;
            }
        }

        #endregion

    }
}
