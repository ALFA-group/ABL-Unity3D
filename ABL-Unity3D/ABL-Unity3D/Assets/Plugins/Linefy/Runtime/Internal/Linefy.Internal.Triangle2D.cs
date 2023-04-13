using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {

    public struct Triangle2D {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
        Vector2 v0;
        Vector2 v1;
        float dot00;
        float dot01;
        float dot11;
        float invDenom;

        public Triangle2D(Vector2 _a, Vector2 _b, Vector2 _c) {
            a = _a;
            b = _b;
            c = _c;
            v0 = c - a;
            v1 = b - a;
            dot00 = Vector2.Dot(v0, v0);
            dot01 = Vector2.Dot(v0, v1);
            dot11 = Vector2.Dot(v1, v1);
            invDenom = 1f / (dot00 * dot11 - dot01 * dot01);

        }

        public bool PointTest(Vector2 p, ref Vector3 bary) {
            Vector2 v2 = p - a;
            float dot02 = Vector2.Dot(v0, v2);
            float dot12 = Vector2.Dot(v1, v2);
            bary.z = (dot11 * dot02 - dot01 * dot12) * invDenom; // u
            bary.y = (dot00 * dot12 - dot01 * dot02) * invDenom; // v
            bary.x = 1f - (bary.z + bary.y);
            return (bary.z >= 0) && (bary.y >= 0) && (bary.z + bary.y < 1f);
        }

        public bool PointTest(Vector2 p) {
            Vector3 _bary;
            Vector2 v2 = p - a;
            float dot02 = Vector2.Dot(v0, v2);
            float dot12 = Vector2.Dot(v1, v2);
            _bary.z = (dot11 * dot02 - dot01 * dot12) * invDenom; // u
            _bary.y = (dot00 * dot12 - dot01 * dot02) * invDenom; // v
            _bary.x = 1f - (_bary.z + _bary.y);
            return (_bary.z >= 0) && (_bary.y >= 0) && (_bary.z + _bary.y < 1f);
        }

        public static bool PointTest(Vector2 pa, Vector2 pb, Vector2 pc, Vector2 pp, ref Vector3 _bary) {
            if (pa.x < pp.x && pb.x < pp.x && pc.x < pp.x) {
                return false;
            }

            if (pa.x > pp.x && pb.x > pp.x && pc.x > pp.x) {
                return false;
            }

            if (pa.y > 0 && pb.y > 0 && pc.y > 0) {
                return false;
            }
            if (pa.y < pp.y && pb.y < pp.y && pc.y < pp.y) {
                return false;
            }

            if (((pc.x - pa.x) * (pb.y - pa.y) - (pc.y - pa.y) * (pb.x - pa.x)) < 0) {
                return false;
            }

            Vector2 _v0 = pc - pa;
            Vector2 _v1 = pb - pa;
            Vector2 _v2 = pp - pa;


            float _dot00 = Vector2.Dot(_v0, _v0);
            float _dot01 = Vector2.Dot(_v0, _v1);
            float _dot11 = Vector2.Dot(_v1, _v1);
            float _invDenom = 1f / (_dot00 * _dot11 - _dot01 * _dot01);

            float _dot02 = Vector2.Dot(_v0, _v2);
            float _dot12 = Vector2.Dot(_v1, _v2);
            _bary.z = (_dot11 * _dot02 - _dot01 * _dot12) * _invDenom;
            _bary.y = (_dot00 * _dot12 - _dot01 * _dot02) * _invDenom;
            _bary.x = 1f - (_bary.z + _bary.y);
            return (_bary.z >= 0) && (_bary.y >= 0) && (_bary.z + _bary.y < 1f);
        }

        public static bool PointTestDoublesided(Vector2 pa, Vector2 pb, Vector2 pc, Vector2 pp, ref Vector3 _bary) {
            if (pa.x < pp.x && pb.x < pp.x && pc.x < pp.x) {
                return false;
            }

            if (pa.x > pp.x && pb.x > pp.x && pc.x > pp.x) {
                return false;
            }
 
            if (pa.y < pp.y && pb.y < pp.y && pc.y < pp.y) {
                return false;
            }

            Vector2 _v0 = pc - pa;
            Vector2 _v1 = pb - pa;
            Vector2 _v2 = pp - pa;

            float _dot00 = Vector2.Dot(_v0, _v0);
            float _dot01 = Vector2.Dot(_v0, _v1);
            float _dot11 = Vector2.Dot(_v1, _v1);
            float _invDenom = 1f / (_dot00 * _dot11 - _dot01 * _dot01);

            float _dot02 = Vector2.Dot(_v0, _v2);
            float _dot12 = Vector2.Dot(_v1, _v2);
            _bary.z = (_dot11 * _dot02 - _dot01 * _dot12) * _invDenom;
            _bary.y = (_dot00 * _dot12 - _dot01 * _dot02) * _invDenom;
            _bary.x = 1f - (_bary.z + _bary.y);
            return (_bary.z >= 0) && (_bary.y >= 0) && (_bary.z + _bary.y < 1f);
        }

        public static bool PointTestDoublesided(Vector2 pa, Vector2 pb, Vector2 pc, Vector2 pp ) {
            if (pa.x < pp.x && pb.x < pp.x && pc.x < pp.x) {
                return false;
            }

            if (pa.x > pp.x && pb.x > pp.x && pc.x > pp.x) {
                return false;
            }

            if (pa.y < pp.y && pb.y < pp.y && pc.y < pp.y) {
                return false;
            }

            Vector2 _v0 = pc - pa;
            Vector2 _v1 = pb - pa;
            Vector2 _v2 = pp - pa;

            float _dot00 = Vector2.Dot(_v0, _v0);
            float _dot01 = Vector2.Dot(_v0, _v1);
            float _dot11 = Vector2.Dot(_v1, _v1);
            float _invDenom = 1f / (_dot00 * _dot11 - _dot01 * _dot01);

            float _dot02 = Vector2.Dot(_v0, _v2);
            float _dot12 = Vector2.Dot(_v1, _v2);
            float _baryz = (_dot11 * _dot02 - _dot01 * _dot12) * _invDenom;
            float _baryy = (_dot00 * _dot12 - _dot01 * _dot02) * _invDenom;
            //float _baryx = 1f - (_baryz + _baryy);
            return (_baryz >= 0) && (_baryy >= 0) && (_baryz + _baryy < 1f);
        }

        public static Vector3 InscribedCircle(Vector2 a, Vector2 b, Vector2 c) {
            Vector2 aBisectorsDir = Vector2.LerpUnclamped((c - a).normalized, (b - a).normalized, 0.5f);
            Vector2 bBisectorsDir = Vector2.LerpUnclamped((c - b).normalized, (a - b).normalized, 0.5f);
            Vector2 center = new Vector2();
            if (Edge2D.LineLineItersection(a, aBisectorsDir, b, bBisectorsDir, ref center)) {
                float lv = 0;
                float radius = Edge2D.GetDistance(a, b, center, ref lv);
                return new Vector3(center.x, center.y, radius);
            } else {
                return new Vector3(a.x, a.y, 0);
            }
        }

        public static bool IsClockwise(Vector2 pa, Vector2 pb, Vector2 pc) {
            return ((pc.x - pa.x) * (pb.y - pa.y) - (pc.y - pa.y) * (pb.x - pa.x)) >= 0;
        }

        public static int Clockwise(Vector2 pa, Vector2 pb, Vector2 pc) {
            float num = ((pc.x - pa.x) * (pb.y - pa.y) - (pc.y - pa.y) * (pb.x - pa.x));
            if (MathUtility.ApproximatelyZero(num)) {
                return 0;
            }
            if (num < 0) {
                return -1;
            } else {
                return 1;
            }
        }
    }
}