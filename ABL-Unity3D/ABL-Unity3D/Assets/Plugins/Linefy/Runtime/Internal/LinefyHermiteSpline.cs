using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy.Internal {

 
    public class HermiteSpline : Drawable {

        protected Vector3[] knots;
        public readonly Vector3[] points;
        float[] distances;
        Vector3[] constantPoints;

        int segments;
        public int segmentsCount { 
            get {
                return segments;
            }
        }
 
        public int knotsCount{
			get{
				return knots.Length;
			}
		}
 
        internal float step = 0;
        protected Vector3 minusKnot;
        protected Vector3 plusKnot;
        float normalizedPointsStep;
        protected int sectorsCount;
        protected bool knotsIsModified = true;
        protected bool polylineIsModified = true;
        Polyline _polyline;
        public  SerializationData_Polyline properties = new SerializationData_Polyline(  2, Color.green, 1, false);
        public readonly bool constantSpeed;

        float _tension = 0;
        public float tension {
            get {
                return _tension;
            }

            set {
                if (value != _tension) {
                    _tension = value;
                    knotsIsModified = true;
                    polylineIsModified = true;
                }
                
            }
        }


        public float splineLength {
            get {
                return _splineLength;
            }
        }
        float _splineLength = 0;

        public HermiteSpline (int _knotsCount, int segmentsCount, bool constantSpeed, float tension) {
            if (_knotsCount < 2) {
                Debug.LogErrorFormat("HermiteSpline.ctor() knots count {0}<2", knotsCount);
                _knotsCount = 2;
            }
 
            if (segmentsCount < 1) {
                Debug.LogErrorFormat("HermiteSpline.ctor() segments count {0}<1", this.segmentsCount);
                segmentsCount = 1;
            }
            this._tension = tension;
            this.constantSpeed = constantSpeed;
            this.segments = segmentsCount;
            knots = new Vector3[ _knotsCount ];
            int pointsCount = getPointCount;
            points = new Vector3[pointsCount];
            step = 1f / this.segmentsCount;
            int edgesCount = pointsCount - 1;
            normalizedPointsStep = 1f / edgesCount;
            sectorsCount = _knotsCount - 1;
            if (constantSpeed) {
                distances = new float[pointsCount];
                constantPoints = new Vector3[pointsCount];
            }
        }

        public void SetKnots(Vector3[] knotsPositions) {
            if (knotsPositions == null) {
                return;
            }    

            if ( knotsPositions.Length != knots.Length) {
                return;
            }

            knotsPositions.CopyTo(knots, 0);
            knotsIsModified = true;
            polylineIsModified = true;
            ApplyKnotsPositions();
        }

        protected virtual int getPointCount { 
            get {
                return (this.knotsCount - 1) * segmentsCount + 1;
            }
        }

        protected virtual Vector3 getMinusKnot { 
            get { 
                return knots[0] - (knots[1] - knots[0]); 
            }
        }

        protected virtual Vector3 getPlusKnot {
            get {
                return knots[knots.Length - 1] + (knots[knots.Length - 1] - knots[knots.Length - 2]);
            }
        }

        protected virtual Vector3 getLastPoint { 
            get {
                return this[knots.Length - 1];
            }
        }

        public virtual void ApplyKnotsPositions() {
            if (knotsIsModified) {
                _splineLength = 0; 
                minusKnot = getMinusKnot;
                plusKnot = getPlusKnot;
                int pointsCounter = 0;
                for (int k = 0; k < sectorsCount; k++) {
                    points[pointsCounter] = knots[k];
                    pointsCounter++;
                    Vector3 hp0 = this[k - 1];
                    Vector3 hp1 = this[k];
                    Vector3 hp2 = this[k + 1];
                    Vector3 hp3 = this[k + 2];
                    for (int s = 1; s < segmentsCount; s++) {
                        points[pointsCounter] = Vector3Utility.HermiteInterpolate(hp0, hp1, hp2, hp3, s * step, tension);
 
                        pointsCounter++;
                    }
                }
                points[pointsCounter] = getLastPoint;
                
                if (constantSpeed) {
                    for (int v = 1; v < points.Length; v++) {
                       _splineLength += Vector3.Distance(points[v], points[v - 1]);
                       distances[v] = _splineLength;
                    }

                    constantPoints[0] = points[0];
                    int findedIndex = 1;
                    for (int i = 1; i<constantPoints.Length; i++) {
                        constantPoints[i] = GetConstantPoint(i * normalizedPointsStep, ref findedIndex);
                    }
                    constantPoints[constantPoints.Length - 1] = getLastPoint;
                 }
                knotsIsModified = false;
            }
        }

        Vector3 GetConstantPoint(float persentage, ref int findedIndex) {
            float targetDist = persentage * _splineLength;
            for (int i = findedIndex; i < distances.Length; i++) {
                float a = distances[i - 1];
                float b = distances[i];
                if (a <= targetDist && b > targetDist) {
                    float lv = (targetDist - a) / (b - a);
                    findedIndex = i;
                    return Vector3.LerpUnclamped(points[i - 1], points[i], lv);
                }
            }
            return points[0];
        }

        public virtual Vector3 this[int knotIdx] {
            get {
                knotIdx = MathUtility.RepeatIdx(knotIdx, knots.Length);
                return knots[knotIdx];
            }

            set {
                knotIdx = MathUtility.RepeatIdx(knotIdx, knots.Length);
                knots[knotIdx] = value;
                polylineIsModified = true;
                knotsIsModified = true;
            }
        }

        public void DrawDebugEndPoints(float size) {
            //Extension.DebugDrawPoint(minusKnot, size, Color.red);
            //Extension.DebugDrawPoint(plusKnot, size, Color.green);
        }

        public void DrawDebug(Color c) {
            for (int i = 0; i < points.Length - 1; i++) {
                Debug.DrawLine(points[i], points[i + 1], c);
            }
        }

        public void DrawDebug(Color c, Matrix4x4 tm) {
            for (int i = 0; i < points.Length - 1; i++) {
                Debug.DrawLine(tm.MultiplyPoint3x4( points[i]),  tm.MultiplyPoint3x4( points[i + 1] ), c);
            }
        }

        public virtual Vector3 GetPoint(float t) {
            if (t >= 1f) {
                return points[points.Length-1];
            }

            int pointsSegmentIdx = Mathf.FloorToInt(t / normalizedPointsStep);
            float lv = (t - (normalizedPointsStep * pointsSegmentIdx)) / normalizedPointsStep;
            if (constantSpeed) {
                return Vector3.LerpUnclamped(constantPoints[pointsSegmentIdx], constantPoints[pointsSegmentIdx + 1], lv);
            } else {
                return Vector3.LerpUnclamped(points[pointsSegmentIdx], points[pointsSegmentIdx + 1], lv);
            }
        }

        void PreDraw() {
            if (polylineIsModified) {
                ApplyKnotsPositions();
                if (_polyline == null) {
                    _polyline = new Polyline(points.Length);
                }

                _polyline.LoadSerializationData(properties);

                if (isClosed) {
                    _polyline.isClosed = true;
                    _polyline.count = points.Length-1;
                    if (constantSpeed) {
                        for (int i = 0; i < points.Length-1; i++) {
                            _polyline.SetPosition(i, constantPoints[i]);
                        }
                    } else {
                        for (int i = 0; i < points.Length-1; i++) {
                            _polyline.SetPosition(i, points[i]);
                        }
                    }
                } else {
                    _polyline.isClosed = false;
                    _polyline.count = points.Length;
                    if (constantSpeed) {
                        for (int i = 0; i < points.Length; i++) {
                            _polyline.SetPosition(i, constantPoints[i]);
                        }
                    } else {
                        for (int i = 0; i < points.Length; i++) {
                            _polyline.SetPosition(i, points[i]);
                        }
                    }
                }
                polylineIsModified = false;
            }

        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();
            _polyline.DrawNow(matrix);
        }

        public override void Draw(Matrix4x4 matrix, Camera cam, int layer) {
            PreDraw();
            _polyline.Draw(matrix, cam, layer);
            _polyline.isClosed = isClosed;
        }

        public override void Dispose() {
            throw new System.NotImplementedException();
        }

        public virtual bool isClosed {
            get {
                return false;
            }
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (_polyline != null) {
                polylinesCount += 1;
                totalPolylineVerticesCount += _polyline.count;
            }
        }

        [System.Obsolete]
        public int PointsCount;

        [System.Obsolete]
        public Vector3[] Points;

    }

    public class HermiteSplineClosed : HermiteSpline {

        public HermiteSplineClosed( int knotsCount, int segmentsCount ) : base(knotsCount, segmentsCount, false, 1) {
            sectorsCount = knotsCount;
        }

        public HermiteSplineClosed(int knotsCount, int segmentsCount, bool constantSpeed, float tension):base(knotsCount, segmentsCount, constantSpeed, tension) {
            sectorsCount = knotsCount;
        }

        protected override Vector3 getMinusKnot {
            get {
                return knots[knots.Length - 2];
            }
        }

        protected override Vector3 getPlusKnot { 
            get {
                return knots[0];
            }
        }

        protected override int getPointCount { 
            get {
               return  this.knotsCount * segmentsCount + 1;
            }
        }

        protected override Vector3 getLastPoint { 
            get {
                return points[0];
            }
        }

        public override bool isClosed {
            get {
                return true;
            }
        }

    }
    
}
