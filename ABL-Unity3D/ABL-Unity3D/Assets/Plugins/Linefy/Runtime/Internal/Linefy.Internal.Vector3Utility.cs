using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;

namespace Linefy {

    public static class Vector3Utility {

        public static Vector3 HermitePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            return new Vector3(MathUtility.HermiteValue(p0.x, p1.x, p2.x, p3.x, t), MathUtility.HermiteValue(p0.y, p1.y, p2.y, p3.y, t), MathUtility.HermiteValue(p0.z, p1.z, p2.z, p3.z, t));
        }

        //public static Vector3 HermitePoint2(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float t) {
        //    float mu2 = t * t;
        //    float mu3 = mu2 * t;
        //    Vector3 m0, m1;
        //    Vector3 a0, a1, a2, a3;
        //    m0 = (y1 - y0) * 0.5f;
        //    m0 += (y2 - y1) / 2;
        //    m1 = (y2 - y1) / 2;
        //    m1 += (y3 - y2) / 2;
        //    a0 = 2 * mu3 - 3 * mu2 + 1;
        //    a1 = mu3 - 2 * mu2 + t;
        //    a2 = mu3 - mu2;
        //    a3 = -2 * mu3 + 3 * mu2;
        //    return (a0 * y1 + a1 * m0 + a2 * m1 + a3 * y2);
        //}

        public static Vector3 HermiteInterpolate(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu, float tension ) {
            float mu2, mu3;
            Vector3 m0, m1;

            mu2 = mu * mu;
            mu3 = mu2 * mu;
            m0 = (y1 - y0)   * (1 - tension) / 2;
            m0 += (y2 - y1)   * (1 - tension) / 2;
            m1 = (y2 - y1)   * (1 - tension) / 2;
            m1 += (y3 - y2)   * (1 - tension) / 2;
            float a0 = 2 * mu3 - 3 * mu2 + 1;
            float a1 = mu3 - 2 * mu2 + mu;
            float a2 = mu3 - mu2;
            float a3 = -2 * mu3 + 3 * mu2;

            return (a0 * y1 + a1 * m0 + a2 * m1 + a3 * y2);

        }

    }
}


 