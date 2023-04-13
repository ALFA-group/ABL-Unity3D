using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;

namespace Linefy.Primitives {

    public class CircularPolyline : Drawable {
        bool dSize = true;
        bool dTopology = true;
        int _segments = 32;
        public int segments {
            get {
                return _segments;
            }

            set {
                if (_segments != value) {
                    dTopology = true;
                    _segments = value;
                }
            }
        }

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
        
        Texture _tex;
        float _textureScale;

        Polyline baseRadius;
        public SerializationData_Polyline wireframeProperties = new SerializationData_Polyline(3, Color.white, 1, true);

        public CircularPolyline(int segments, float radius) {
            this.segments = segments;
            this.radius = radius;
        }

        public CircularPolyline(int segments, float radius, SerializationData_Polyline wireframeProperties) {
            this.segments = segments;
            this.radius = radius;
            this.wireframeProperties = wireframeProperties;
        }

        public CircularPolyline(int segments, float radius, Color color) {
            this.segments = segments;
            this.radius = radius;
            this.wireframeProperties.colorMultiplier = color;
        }

        void PreDraw() {
            if (baseRadius == null) {
                baseRadius = new Polyline(_segments);

            }
            baseRadius.LoadSerializationData(wireframeProperties);
 
            if (dTopology) {
                baseRadius.count = segments;
                float uvxStep = 1f / (float)_segments;
                for (int i = 0; i < segments; i++) {
                    baseRadius.SetTextureOffset(i, i * uvxStep);
                }
                baseRadius.lastVertexTextureOffset = 1;
                dSize = true;
                dTopology = false;
            }

            if (dSize) {
                float aStep = 6.283185f / _segments;

                for (int i = 0; i < segments; i++) {
                    float a = i * aStep;
                    float x = Mathf.Cos(a) * radius;
                    float y = Mathf.Sin(a) * radius;
                    baseRadius.SetPosition(i, new Vector3(x, y, 0));
                }

                dSize = false;
            }
        }

        public override void Dispose() {
            if (baseRadius != null) {
                baseRadius.Dispose();
                baseRadius.capacityChangeStep = 8;
            }
        }

        public override void Draw(Matrix4x4 matrix, Camera cam, int layer) {
            PreDraw();
            baseRadius.Draw(matrix, cam, layer);
        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();
            baseRadius.DrawNow(matrix);
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (baseRadius != null) {
                polylinesCount++;
                totalPolylineVerticesCount += baseRadius.count;
            }
        }

    }

    [ExecuteInEditMode]
    public class LinefyCircularPolyline : DrawableComponent {
       
        int _segmentsCount = 64;
        [Range(3, 256)]
        public int segmentsCount = 64;

        float _radius;
        public float radius = 1;

        float _angle = 360;
        public float angle = 360;

        float _offsetAngle = 0;
        public float offsetAngle = 0;

        Polyline _polyline;
        public Polyline polyline {
            get {
                if (_polyline == null) {
                    _polyline = new Polyline(segmentsCount, true);
                }
                return _polyline;
            }
        }

        public SerializationData_Polyline polylineProperties = new SerializationData_Polyline();

        public override Drawable drawable {
			get{
				return polyline;
			}
		} 

        float _widthCurveTiling = 1;

        [Header("Width")]
        public float widthCurveTiling = 1;
        public AnimationCurve widthCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [Header("Transparency")]
        public float transparencyCurveTiling = 1;
        public AnimationCurve transparencyCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
 
        [Header("Auto animated value")]
        public float textureOffsetSpeed;
        public float angleOffsetSpeed;

        protected override void PreDraw() {
            bool formIsDirty = Application.isPlaying == false;
 
            if (_segmentsCount != segmentsCount) {
                _segmentsCount = segmentsCount;
                polyline.count = segmentsCount;
                formIsDirty = true;
            }

            if (_offsetAngle != offsetAngle) {
                formIsDirty = true;
                _offsetAngle = offsetAngle;
            }

            if (_angle != angle) {
                formIsDirty = true;
                _angle = angle;
            }

            if (_radius != radius) {
                formIsDirty = true;
                _radius = radius;
            }

            if (_widthCurveTiling != widthCurveTiling) {
                formIsDirty = true;
                _widthCurveTiling = widthCurveTiling;
            }

            if (formIsDirty) {
                float step = 1f / (float)segmentsCount;
                float offsetRad = offsetAngle * Mathf.Deg2Rad;
                float angleRad = (angle * Mathf.Deg2Rad);
                for (int i = 0; i < segmentsCount; i++) {
                    float pers = i * step;
                    float a = offsetRad + pers * angleRad;
                    float x = Mathf.Cos(a) * radius;
                    float y = Mathf.Sin(a) * radius;
                    float alpha = transparencyCurve.Evaluate((pers * transparencyCurveTiling) % 1f);
                    polyline[i] = new PolylineVertex(new Vector3(x, y), new Color(1,1,1,alpha), widthCurve.Evaluate( (pers*widthCurveTiling)%1f ), step * i);
                }
                polyline.lastVertexTextureOffset = 1;
            }
            polyline.LoadSerializationData(polylineProperties);
            if (Application.isPlaying) {
                polyline.textureOffset += Time.timeSinceLevelLoad * textureOffsetSpeed;
                offsetAngle  = Time.timeSinceLevelLoad * angleOffsetSpeed;
            }  
        }

        public static LinefyCircularPolyline CreateInstance() {
            GameObject go = new GameObject("New CircularPolyline");
            LinefyCircularPolyline result = go.AddComponent<LinefyCircularPolyline>();
            return result;
        }
    }


}
