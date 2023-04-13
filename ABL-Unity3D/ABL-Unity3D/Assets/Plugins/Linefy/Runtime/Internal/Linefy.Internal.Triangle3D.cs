using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Linefy.Internal{

    public struct Triangle3D {

        Vector3 a;
        Vector3 b;
        Vector3 c;
        Vector3 e1;
        Vector3 e2;

        public Triangle3D(Vector3 a, Vector3 b, Vector3 c) {
            this.a = a;
            this.b = b;
            this.c = c;
            e1 = b - a;
            e2 = c - a;
        }

        public bool RaycastDoublesided(Ray r, ref Vector3 bary, ref Vector3 hit) {
            Vector3 pvec = Vector3.Cross(r.direction, e2);
            float det = Vector3.Dot(e1, pvec);

            //Debug.LogFormat("det:{0}", det);

            if (det.EqualsApproximately(0)) { // parallel
                return false;
            }

            float invDet = 1f / det;
            Vector3 tvec = r.origin - a;
            float u = Vector3.Dot(tvec, pvec) * invDet;

            if (u < 0 || u > 1) {
                return false;
            }

            Vector3 qvec = Vector3.Cross(tvec, e1);
            float v = Vector3.Dot(r.direction, qvec) * invDet;

            float uvSumm = u + v;
            if (v < 0 || uvSumm > 1) {
                return false;
            }

            float t = Vector3.Dot(e2, qvec) * invDet;

            bary.x = 1f - uvSumm;
            bary.y = u;
            bary.z = v;
            hit = r.GetPoint(t);
            return true;
        }

        public bool Raycast(Ray r, ref Vector3 bary, ref Vector3 hit) {
            Vector3 pvec = Vector3.Cross(r.direction, e2);
            float det = Vector3.Dot(e1, pvec);

            if (det.LessOrEqualsThan(0)) { // parallel
                return false;
            }

            float invDet = 1f / det;
            Vector3 tvec = r.origin - a;
            float u = Vector3.Dot(tvec, pvec) * invDet;

            if (u < 0 || u > 1) {
                return false;
            }

            Vector3 qvec = Vector3.Cross(tvec, e1);
            float v = Vector3.Dot(r.direction, qvec) * invDet;

            float uvSumm = u + v;
            if (v < 0 || uvSumm > 1) {
                return false;
            }

            float t = Vector3.Dot(e2, qvec) * invDet;

            bary.x = 1f - uvSumm;
            bary.y = u;
            bary.z = v;
            hit = r.GetPoint(t);
            return true;
        }

        public Vector3 GetPoint(Vector3 bary) {
            return a * bary.x + b * bary.y + bary.z * c;
        }

        public float Area() {
            return Vector3.Cross( a - b  ,  b - c  ).magnitude/2f ;
        }

        public float Area2( ) {
            float _a = Vector3.Distance(a,b);
            float _b = Vector3.Distance(b, c);
            float _c = Vector3.Distance(c, a);
            float s = (_a + _b + _c) / 2f;
            return Mathf.Sqrt(s * (s - _a) * (s - _b) * (s - _c));
        }

        public void DrawDebug(Color color) {
            Debug.DrawLine(a, b, color);
            Debug.DrawLine(b, c, color);
            Debug.DrawLine(c, a, color);
        }

        public float DistanceToPoint(Vector3 point) {
            Plane p = new Plane(a, b, c);
            return p.GetDistanceToPoint(point);
        }
    }
}
