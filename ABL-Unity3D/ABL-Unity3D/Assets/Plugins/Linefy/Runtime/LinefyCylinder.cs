using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy.Primitives {
    
    public class Cylinder : Drawable {
        bool dSize = true;
        bool dTopology = true;

        float _radiusTop = 1;
        public float radiusTop {
            get {
                return _radiusTop;
            }

            set {
                if (_radiusTop != value) {
                    dSize = true;
                    _radiusTop = value;
                }
            }
        }

        float _radiusBottom = 1;
        public float radiusBottom {
            get {
                return _radiusBottom;
            }

            set {
                if (_radiusBottom != value) {
                    dSize = true;
                    _radiusBottom = value;
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

        Texture _tex;
        float _textureScale;
 
        Lines radials;
        Polyline topRadius;
        Polyline bottomRadius;
        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        public Cylinder() { }

        public Cylinder(float radiusTop, float radiusBottom, float height, int radiusSegments, int radialsCount, SerializationData_LinesBase wireframeProperties) {
            _radiusTop = radiusTop;
            _radiusBottom = radiusBottom;
            _height = height;
            _radiusSegments = radiusSegments;
            _radialsCount = radialsCount;
            this.wireframeProperties = wireframeProperties;
        }

        void PreDraw() {
            if (radials == null) {
                bottomRadius = new Polyline(radiusSegments, true);
                bottomRadius.capacityChangeStep = 16;
                topRadius = new Polyline(radiusSegments, true);
                topRadius.capacityChangeStep = 16;
                radials = new Lines(radialsCount);
                radials.capacityChangeStep = 8;
                radials.autoTextureOffset = true;
            }

            if (dTopology) {
                radials.count = radialsCount;
                bottomRadius.count = radiusSegments;
                topRadius.count = radiusSegments;
                for (int i = 0; i<radials.count; i++ ) {
                    radials.SetTextureOffset(i, 0, 0);
                }
                dTopology = false;
                dSize = true;
            }

            radials.LoadSerializationData(wireframeProperties);
            topRadius.LoadSerializationData(wireframeProperties);
            bottomRadius.LoadSerializationData(wireframeProperties);
            topRadius.textureScale = 1;
            bottomRadius.textureScale = 1;

            if (wireframeProperties.texture != _tex) {
                _tex = wireframeProperties.texture;
                dSize = true;
            }

            if (wireframeProperties.textureScale != _textureScale) {
                _textureScale = wireframeProperties.textureScale;
                dSize = true;
            }

            if (dSize) {
                float aStep = 6.283185f / _radiusSegments;
   
                float yBottom = -_height * _pivotOffset;
                float yTop = _height + yBottom;

                for (int i = 0; i < radiusSegments; i++) {
                    float a = i * aStep;
                    float x = Mathf.Cos(a)  ;
                    float z = Mathf.Sin(a)  ;
                    topRadius.SetPosition(i, new Vector3(x * _radiusTop, yTop, z * _radiusTop));
                    bottomRadius.SetPosition(i, new Vector3(x * _radiusBottom, yBottom, z * _radiusBottom)); 
                }
                aStep = 6.283185f / _radialsCount;
                for (int i = 0; i< _radialsCount; i++) {
                    float a = i * aStep;
                    float x = Mathf.Cos(a) ;
                    float z = Mathf.Sin(a) ;
                    Vector3 top = new Vector3(x * _radiusTop, yTop, z * _radiusTop);
                    Vector3 bottom = new Vector3(x * _radiusBottom, yBottom, z * _radiusBottom);
                    radials.SetPosition(i, top, bottom); 
                }
                if (wireframeProperties.texture != null) {
                    topRadius.RecalculateDistances(wireframeProperties.textureScale);
                    bottomRadius.RecalculateDistances(wireframeProperties.textureScale);
                }

                dSize = false;
            }
        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();
            radials.DrawNow(matrix);
            topRadius.DrawNow(matrix);
            bottomRadius.DrawNow(matrix);
        }

        public override void Draw(Matrix4x4 tm, Camera cam, int layer) {
            PreDraw();
            radials.Draw(tm, cam, layer);
            topRadius.Draw(tm, cam, layer);
            bottomRadius.Draw(tm, cam, layer);
        }

        public override void Dispose() {
            if (radials != null) {
                radials.Dispose();
            }
            if (bottomRadius != null) {
                bottomRadius.Dispose();
            }
            if (topRadius != null) {
                topRadius.Dispose();
            }

        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (radials != null) {
                totallinesCount += radials.count;
                totalPolylineVerticesCount += bottomRadius.count*2;
            }
        }
    }    

    [ExecuteInEditMode]
    public class LinefyCylinder : DrawableComponent {
        public float radiusTop = 1;
        public float radiusBottom = 1;
        public float height = 1;
        [Range(0,1)]
        public float pivotOffset = 0.5f;
        [Range(3, 256)]
        public int radiusSegments = 32;
        [Range(0, 256)]
        public int radialsCount = 8;
        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        Cylinder _cylinder;
        Cylinder cylinder {
            get {
                if (_cylinder == null) {
                    _cylinder = new Cylinder();
                }
                return _cylinder;
            }
        }

        public override Drawable drawable {
			get{
				return cylinder;
			}
		}  

        protected override void PreDraw() {
            cylinder.radiusTop = radiusTop;
            cylinder.radiusBottom = radiusBottom;
            cylinder.height = height;
            cylinder.pivotOffset = pivotOffset;
            cylinder.radiusSegments = radiusSegments;
            cylinder.radialsCount = radialsCount;
            cylinder.wireframeProperties = wireframeProperties;
        }
 
        public static LinefyCylinder CreateInstance() {
            GameObject go = new GameObject("New Cylinder");
            LinefyCylinder result = go.AddComponent<LinefyCylinder>();
            return result;
        }
    }
}
