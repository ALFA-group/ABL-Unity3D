using Linefy;
using UnityEngine;
using Linefy.Serialization;

namespace Linefy.Primitives {

    public class SineWaves : Drawable {

        int _segmentsCount = 32;
        public int segmentsCount {
            get {
                return _segmentsCount;
            }

            set {
                if (_segmentsCount != value) {
                    _segmentsCount = value;
                    dTopology = true;
                }
            }
        }

        int _itemsCount = 4;
        public int itemsCount {
            get {
                return _itemsCount;
            }

            set {
                if (_itemsCount != value) {
                    _itemsCount = value;
                    dTopology = true;
                }
            }
        }

        float _width = 1;
        public float width {
            get {
                return _width;
            }

            set {
                if (_width != value) {
                    _width = value;
                    dSize = true;
                }
            }
        }

        float _heightSpacing = 0.2f;
        public float heightSpacing {
            get {
                return _heightSpacing;
            }

            set {
                if (_heightSpacing != value) {
                    _heightSpacing = value;
                    dSize = true;
                }
            }
        }

        float _waveHeight = 0.04f;
        public float waveHeight {
            get {
                return _waveHeight;
            }

            set {
                if (_waveHeight != value) {
                    _waveHeight = value;
                    dSize = true;
                }
            }
        }

        float _waveLength = 0.3f;
        public float waveLength {
            get {
                return _waveLength;
            }

            set {
                if (_waveLength != value) {
                    _waveLength = value;
                    dSize = true;
                }
            }
        }

        float _waveOffset;
        public float waveOffset {
            get {
                return _waveOffset;
            }

            set {
                if (_waveOffset != value) {
                    _waveOffset = value;
                    dSize = true;
                }
            }
        }


        bool _centerPivot = true;
        public bool centerPivot {
            get {
                return _centerPivot;
            }

            set {
                if (_centerPivot != value) {
                    _centerPivot = value;
                    dSize = true;
                }
            }
        }

        bool dTopology = true;
        bool dSize = true;

        Polyline polyline;
        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        void PreDraw() {
            int verticesCount = (_segmentsCount + 1 +2) * _itemsCount  ;

            if (polyline == null) {
                polyline = new Polyline(verticesCount);
            }

            if (dTopology) {
                polyline.count = verticesCount;
                int pointCounter = 0;
                for (int i = 0; i < _itemsCount; i++) {
                    polyline.SetWidth(pointCounter, 0);
                    pointCounter++;
                    float ro = Random.value;
                    for (int s = 0; s <= segmentsCount; s++) {
                        polyline.SetWidth(pointCounter, 1);
                        polyline.SetTextureOffset( pointCounter, ro + s / (float)segmentsCount);
                        pointCounter++;
                    }
  
                    polyline.SetWidth(pointCounter, 0);
                    pointCounter++;
                }

                dTopology = false;
                dSize = true;
            }


            if (dSize) {

                float yStart = 0;
                float xStart = 0;

                if (centerPivot) {
                    yStart = -(_heightSpacing * (_itemsCount - 1)) / 2f;
                    xStart = -_width / 2f;
                }

                int pointCounter = 0;
                float perSegment = _width / _segmentsCount;
                for (int i = 0; i < _itemsCount; i++) {
                    bool doubleFirst = false;
                    bool doubleLast = false;
                    int s = 0;
                    while (s <= _segmentsCount ) {
                        Vector3 pos = new Vector3(xStart + s * perSegment, yStart + _heightSpacing * i, 0);
                        float a = (_waveOffset + pos.x) / waveLength * 6.283185f;
                        float sin = Mathf.Sin(a) * _waveHeight;
                        pos.y += sin;
                        polyline.SetPosition(pointCounter, pos);
                        pointCounter++;
 
                        if (s == 0 && doubleFirst == false) {
                            doubleFirst = true;
                        } else if (s == segmentsCount && doubleLast == false) {
                            doubleLast = true;
                        } else {
                            s++;
                        }
                    }
                }

                dSize = false;
            }
            polyline.LoadSerializationData(wireframeProperties);
        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();

            polyline.DrawNow(matrix);
        }

        public override void Draw(Matrix4x4 tm, Camera cam, int layer) {
            PreDraw();

            polyline.Draw(tm, cam, layer);
        }

        public override void Dispose() {
            if (polyline != null) {
                polyline.Dispose();
            }
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            //if (wireframe != null) {
            //    totallinesCount += wireframe.count;
            //    totalPolylineVerticesCount += baseRadius.count;
            //}
        }
    }

    [ExecuteInEditMode]
    public class LinefySineWaves : DrawableComponent {
        [Range(1, 256)]
        public int segmentsCount = 32;
        [Range(1, 128)]
        public int itemsCount = 4;
        public float width = 1;
        public float heightSpacing = 0.2f;
        public float waveHeight = 0.04f;
        public float waveLength = 0.3f;
        public float waveOffset;
        public bool centerPivot = true;

        SineWaves _sineWaves;
        SineWaves sineWaves {
            get {
                if (_sineWaves == null) {
                    _sineWaves = new SineWaves();
                }
                return _sineWaves;
            }
        }

        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        public override Drawable drawable {
			get{
				return sineWaves;
			}
		}

        protected override void PreDraw() {
            sineWaves.segmentsCount = segmentsCount;
            sineWaves.itemsCount = itemsCount;
            sineWaves.width = width;
            sineWaves.heightSpacing = heightSpacing;
            sineWaves.waveHeight = waveHeight;
            sineWaves.waveLength = waveLength;
            sineWaves.waveOffset = waveOffset;
            sineWaves.centerPivot = centerPivot;
            sineWaves.wireframeProperties = wireframeProperties;
        }

        public static LinefySineWaves CreateInstance() {
            GameObject go = new GameObject("New SineWaves");
            LinefySineWaves result = go.AddComponent<LinefySineWaves>();
            return result;
        }

    }
}
