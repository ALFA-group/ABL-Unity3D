using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;

namespace Linefy {
    public static partial class RectUtility {

        public static Rect Inflate(this Rect r, float inflateFactor) {
            float halfv = inflateFactor;
            r.xMin -= halfv;
            r.xMax += halfv;
            r.yMin -= halfv;
            r.yMax += halfv;
            return r;
        }

        public static Vector2 Point0(this Rect r) {
            return r.min;
        }

        public static Vector2 Point1(this Rect r) {
            return new Vector2(r.min.x, r.max.y);
        }

        public static Vector2 Point2(this Rect r) {
            return r.max;
        }

        public static Vector2 Point3(this Rect r) {
            return new Vector2(r.max.x, r.min.y);
        }

        public static Vector2 Clamp(this Rect r, Vector2 v) {
            v.x = Mathf.Clamp(v.x, r.xMin, r.xMax);
            v.y = Mathf.Clamp(v.y, r.yMin, r.yMax);
            return v;
        }

        public static Rect Multiply(this Rect a, Rect b) {
            Vector2 resultPos = Rect.NormalizedToPoint(a, b.position);
            Vector2 resultScale = Rect.NormalizedToPoint(a, b.max) - resultPos;
            return new Rect(resultPos, resultScale);
        }

        public static Rect Scale(this Rect a, float scaleFactor) {
            return new Rect(a.x * scaleFactor, a.y * scaleFactor, a.width * scaleFactor, a.height * scaleFactor);
        }

        public static Rect Offset(this Rect a, float offsetX, float offsetY) {
            Rect result = a;
            result.x += offsetX;
            result.y += offsetY;
            return result;
        }

        public static RectInt Scale(this RectInt a, float scaleFactor) {
            return new RectInt((int)(a.x * scaleFactor), (int)(a.y * scaleFactor), (int)(a.width * scaleFactor), (int)(a.height * scaleFactor));
        }

        public static Rect GetRect(this RectInt a) {
            return new Rect(a.position, a.size);
        }

        public static float Distance(this Rect r, Vector2 point) {
            if (r.Contains(point)) {
                return 0;
            }
            float minDistance = float.MaxValue;
            minDistance = Mathf.Min(Edge2D.GetDistance(r.Point0(), r.Point1(), point), minDistance);
            minDistance = Mathf.Min(Edge2D.GetDistance(r.Point1(), r.Point2(), point), minDistance);
            minDistance = Mathf.Min(Edge2D.GetDistance(r.Point2(), r.Point3(), point), minDistance);
            minDistance = Mathf.Min(Edge2D.GetDistance(r.Point3(), r.Point0(), point), minDistance);
            return minDistance;
        }
    }
}
