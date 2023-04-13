using UnityEngine;
using System.Collections.Generic;


namespace Linefy.Internal {

    public static partial class Utility {

        public static bool RayCircleInterction(Ray2D ray, Vector2 circleCenter, float circleRadius, ref float i0, ref float i1) {
            Vector2 projectedCenter = new Vector2();
            float distToCenter = ray.DistanceToPoint(circleCenter, ref projectedCenter);
            if (distToCenter < circleRadius) {
                Vector2 toProjectedCenter = projectedCenter - ray.origin;
                float sign = Mathf.Sign( Vector2.Dot(toProjectedCenter, ray.direction));
                float alongRayDist = toProjectedCenter.magnitude;
                float halfHorde = Mathf.Sqrt(circleRadius * circleRadius - distToCenter * distToCenter);
                i0 = (alongRayDist - halfHorde)*sign;
                i1 = (alongRayDist + halfHorde)*sign;
                return true;
            } else {
                return false;
            }
        }

        public static float DistanceToPoint(this Ray2D r, Vector2 point, ref Vector2 nearestPoint) {
            Vector2 dir = r.direction;
            Vector2 orthoDir = new Vector2(-dir.y, dir.x);
            Edge2D.LineLineItersection(r.origin, r.direction, point, orthoDir, ref nearestPoint);
            
            return Vector2.Distance(point, nearestPoint);
        }

        public static Ray Multiply(this Ray r, Matrix4x4 m) {
            Vector3 dir = m.MultiplyVector(r.direction);
            Vector3 pos = m.MultiplyPoint3x4(r.origin);
            return new Ray(pos, dir);
        }
 
        public static float TicksPerMilliseconds {
            get {
                return System.TimeSpan.TicksPerMillisecond;
            }
        }

        public static float Milliseconds(this System.Diagnostics.Stopwatch sw) {
            return sw.ElapsedTicks / TicksPerMilliseconds;
        }

        public static bool IsNotValid(this Matrix4x4 tm) {
 
            for (int i = 0; i<4; i++) {
                Vector4 column = tm.GetColumn(i);
                if (column.magnitude.EqualsApproximately(0)) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNotValid(this Quaternion q) {
            float summ = q.x + q.y + q.z + q.w;
            return summ.EqualsApproximately(0);
        }

        public static Vector3 HermitePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float s) {
            return new Vector3(HermiteValue(p0.x, p1.x, p2.x, p3.x, s), HermiteValue(p0.y, p1.y, p2.y, p3.y, s), HermiteValue(p0.z, p1.z, p2.z, p3.z, s));
        }

        public static Vector2 HermitePoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float s) {
            return new Vector3(HermiteValue(p0.x, p1.x, p2.x, p3.x, s), HermiteValue(p0.y, p1.y, p2.y, p3.y, s));
        }

        public static float HermiteValue(float y0, float y1, float y2, float y3, float s) {
            float mu2 = s * s;
            float mu3 = mu2 * s;
            float m0, m1;
            float a0, a1, a2, a3;
            m0 = (y1 - y0) / 2;
            m0 += (y2 - y1) / 2;
            m1 = (y2 - y1) / 2;
            m1 += (y3 - y2) / 2;
            a0 = 2 * mu3 - 3 * mu2 + 1;
            a1 = mu3 - 2 * mu2 + s;
            a2 = mu3 - mu2;
            a3 = -2 * mu3 + 3 * mu2;
            return (a0 * y1 + a1 * m0 + a2 * m1 + a3 * y2);
        }

        public static bool EqualsApproximately(this Vector2 a, Vector2 b) {
            return MathUtility.ApproximatelyZero(a.x - b.x) && MathUtility.ApproximatelyZero(a.y - b.y);
        }

        public static bool EqualsApproximately(this Vector3 a, Vector3 b) {
            return MathUtility.ApproximatelyZero(a.x - b.x) && MathUtility.ApproximatelyZero(a.y - b.y) && MathUtility.ApproximatelyZero(a.z - b.z);
        }

        public static Vector3 XYtoXyZ(this Vector2 v) {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector3 XYtoXyZ(this Vector2 v, float y) {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector2 XyZtoXY(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }

        public static void DebugDrawPoint( Vector3 point, float size, Color color) {
            float hs = size * 0.5f;
            Vector3 top = new Vector3(point.x, point.y + hs, point.z);
            Vector3 bottom = new Vector3(point.x, point.y - hs, point.z);
            Vector3 c0 = new Vector3(point.x - hs, point.y, point.z);
            Vector3 c1 = new Vector3(point.x, point.y, point.z + hs );
            Vector3 c2 = new Vector3(point.x + hs, point.y, point.z);
            Vector3 c3 = new Vector3(point.x, point.y , point.z - hs);

            Debug.DrawLine(c0, top, color);
            Debug.DrawLine(c1, top, color);
            Debug.DrawLine(c2, top, color);
            Debug.DrawLine(c3, top, color);

            Debug.DrawLine(c0, bottom, color);
            Debug.DrawLine(c1, bottom, color);
            Debug.DrawLine(c2, bottom, color);
            Debug.DrawLine(c3, bottom, color);

            Debug.DrawLine(c0, c1, color);
            Debug.DrawLine(c1, c2, color);
            Debug.DrawLine(c2, c3, color);
            Debug.DrawLine(c3, c0, color);
        }

        public static void DrawCircleXY(Vector2 center, float radius, Color color, int segments) {
            float step = 1f / segments;
            for (int i = 0; i<segments; i++) {
                float a0 = step * i * Mathf.PI * 2;
                float a1 = step * (i+1) * Mathf.PI * 2;
                Vector2 posA = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0))*radius + center ;
                Vector2 posB = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius + center;
                Debug.DrawLine(posA, posB, color);
            }

        }

        public static void Resize<T>(this List<T> list, int size, T element = default(T)) {
            int count = list.Count;

            if (size < count) {
                list.RemoveRange(size, count - size);
            } else if (size > count) {
                if (size > list.Capacity) {
                    list.Capacity = size;
                }

                list.AddRange(System.Linq.Enumerable.Repeat(element, size - count));
            }
        }
    }

}
