using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Linefy.Internal {
    public static class Vector2Unility {
        public static float SignedAngle(Vector2 dirA, Vector2 dirB) {
            dirA.Normalize();
            dirB.Normalize();
            float sign = Vector2.Dot(new Vector2(dirA.y, -dirA.x), dirB);
            float a = Mathf.Acos(Vector2.Dot(dirA, dirB)) * Mathf.Rad2Deg;
            return sign < 0 ? 360f - a : a;

        }

        public static Vector2 HermitePoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
            return new Vector3(MathUtility.HermiteValue(p0.x, p1.x, p2.x, p3.x, t), MathUtility.HermiteValue(p0.y, p1.y, p2.y, p3.y, t));
        }
    }

}


 