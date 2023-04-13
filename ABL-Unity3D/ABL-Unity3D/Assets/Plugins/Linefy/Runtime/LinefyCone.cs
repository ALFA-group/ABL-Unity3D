using UnityEngine;
using Linefy.Serialization;

namespace Linefy.Primitives {
    
    public class Cone : Drawable {
        bool dSize = true;
        bool dTopology = true;

        float _radius = 1;
        public float radius {
            get {
                return _radius;
            }

            set {
                if (_radius != value) {
                    dSize = true;
                    _radius = value;
                }
            }
        }

        float _height = 1;
        public float height {
            get {
                return _height;
            }

            set {
                if (_height != value) {
                    dSize = true;
                    _height = value;
                }
            }
        }

        float _pivotOffset = 0.5f;
        public float pivotOffset {
            get {
                return _pivotOffset;
            }

            set {
                if (_pivotOffset != value) {
                    _pivotOffset = value;
                    dSize = true;
                }
            }
        }

        int _radiusSegments = 32;
        public int radiusSegments {
            get {
                return _radiusSegments;
            }

            set {
                if (_radiusSegments != value) {
                    dTopology = true;
                    _radiusSegments = value;
                }
            }
        }

        int _radialsCount = 8;
        public int radialsCount {
            get {
                return _radialsCount;
            }

            set {
                if (_radialsCount != value) {
                    dTopology = true;
                    _radialsCount = value;
                }
            }
        }
 
        Lines wireframe;
        Polyline baseRadius;
        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        Texture _tex;
        float _textureScale;

        public Cone() { }

        public Cone(float radius, float height, int radiusSegments, int radials ) {
            _radius = radius;
            _height = height;
            _radiusSegments = radiusSegments;
            _radialsCount = radials;
        }

        public Cone(float radius, float height, int radiusSegments, int radials, SerializationData_LinesBase wireframeProperties) {
            _radius = radius;
            _height = height;
            _radiusSegments = radiusSegments;
            _radialsCount = radials;
            this.wireframeProperties = wireframeProperties;
        }

        void PreDraw() {
            if (wireframe == null) {
                wireframe = new Lines(1);
                wireframe.capacityChangeStep = 16;
                baseRadius = new Polyline( (int)radiusSegments, true);
                baseRadius.capacityChangeStep = 16;
            }
 
            baseRadius.LoadSerializationData(wireframeProperties);
            baseRadius.textureScale = 1;
            wireframe.LoadSerializationData(wireframeProperties);
            wireframe.autoTextureOffset = true;

            if (dTopology) {
                wireframe.count = (int)radialsCount;
                for (int i = 0; i< radialsCount; i++ ) {
                    wireframe.SetTextureOffset(i, 0, 0);
                }
                baseRadius.count = (int)radiusSegments;
                dTopology = false;
                dSize = true;
            }

            if (wireframeProperties.texture != _tex) {
                _tex = wireframeProperties.texture;
                dSize = true;
            }

            if (wireframeProperties.textureScale != _textureScale) {
                _textureScale = wireframeProperties.textureScale;
                dSize = true;
            }

            if (dSize) {
                float aStep = 6.283185f / radiusSegments;
                float yBottom = -_height * _pivotOffset;
                float yTop = _height + yBottom;

                for (int i = 0; i < radiusSegments; i++) {
                    float a = i * aStep;
                    float x = Mathf.Cos(a) * radius;
                    float z = Mathf.Sin(a) * radius;

                    baseRadius.SetPosition(i, new Vector3(x, yBottom, z)); 
                }

                aStep = 6.283185f / radialsCount;
                Vector3 ep = new Vector3(0, yTop, 0);
                for (int i = 0; i< radialsCount; i++) {
                    float a = i * aStep;
                    float x = Mathf.Cos(a) * radius;
                    float z = Mathf.Sin(a) * radius;
                    wireframe.SetPosition(i, new Vector3(x, yBottom, z), ep); 
                }

                if (wireframeProperties.texture != null) {
                    baseRadius.RecalculateDistances(wireframeProperties.textureScale);
                }
                dSize = false;
            }
        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();
            wireframe.DrawNow(matrix);
            baseRadius.DrawNow(matrix);
        }

        public override void Draw(Matrix4x4 tm, Camera cam, int layer) {
            PreDraw();
            wireframe.Draw(tm, cam, layer);
            baseRadius.Draw(tm, cam, layer);
        }

        public override void Dispose() {
            if (wireframe != null) {
                wireframe.Dispose();
            }
            if (baseRadius != null) {
                baseRadius.Dispose();
            }
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (wireframe != null) {
                totallinesCount += wireframe.count;
                polylinesCount++;
                totalPolylineVerticesCount += baseRadius.count;
            }
        }
    }    

    [ExecuteInEditMode]
    public class LinefyCone : DrawableComponent {

        public float radius = 1;
        public float height = 1;
        [Range(3, 256)]
        public int radiusSegments = 32;
        [Range(0, 256)]
        public int radialsCount = 8;
        [Range(0, 1)]
        public float pivotOffset = 0.5f;

        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        Cone _cone;
        Cone cone {
            get {
                if (_cone == null) { 
                    _cone = new Cone(radius, height, radiusSegments, radialsCount, wireframeProperties);
                }
                return _cone;
            }
        }

        public override Drawable drawable {
			get{
				return cone;
			}
		}  

        protected override void PreDraw() {
            cone.radius = radius;
            cone.height = height;
            cone.radiusSegments = radiusSegments;
            cone.pivotOffset = pivotOffset;
            cone.radialsCount = radialsCount;
            cone.wireframeProperties = wireframeProperties;
        }
 
        public static LinefyCone CreateInstance() {
            GameObject go = new GameObject("New Cone");
            LinefyCone result = go.AddComponent<LinefyCone>();
            return result;
        }
    }
}
