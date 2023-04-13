using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy{
    public class Edge3D  {
        public Vector3 A;
        public Vector3 B;
        Vector3 ab;
        float length;
        float length2;

        public Edge3D(Vector2 a, Vector2 b) {
            A = a;
            B = b;
            ab = B - A;
            length = ab.magnitude;
            length2 = length * length;
        }

        public float GetDistance(Vector3 point) {
            Vector3 ap = point - A;
            float u = Vector3.Dot(ap, ab) / length2;
            Vector3 nearestPoint = Vector3.zero;
            if (u < 0) {
                nearestPoint = A;
            } else if (u > 1) {
                nearestPoint = B;
            } else {
                nearestPoint = A + ab * u;
            }
            return Vector3.Distance(nearestPoint, point);
        }

        public static float LineLineDistance(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, ref Vector3 pa, ref Vector3 pb) {
            //float   d4321, d1321, d4343, d2121,  numer, denom;
 
            Vector3 p13 = a1 - b1;
            Vector3 p43 = b2 - b1;
            Vector3 p21 = a2 - a1;
            float d1343 = p13.x * p43.x + p13.y * p43.y + p13.z * p43.z;
            float d4321 = p43.x* p21.x + p43.y* p21.y + p43.z* p21.z;
            float d1321 = p13.x* p21.x + p13.y* p21.y + p13.z* p21.z;
            float d4343 = p43.x* p43.x + p43.y* p43.y + p43.z* p43.z;
            float d2121 = p21.x* p21.x + p21.y* p21.y + p21.z* p21.z;
            float denom = d2121* d4343 - d4321* d4321;
            float numer = d1343* d4321 - d1321* d4343;
            float mua = numer / denom;
            float mub = (d1343 + d4321 * (mua)) / d4343;
            mua = Mathf.Clamp01(mua);

            pa = a1 + p21 * mua;
            pb = b1 + p43 * mub;
            return Vector3.Distance(pa, pb);
        }


//        function LineLineDistance a1 a2 b1 b2 &pa &pb = (
//   local d1343, d4321, d1321, d4343, d2121, p13, p43, p21 ;
//   local numer, denom;
//        p13 = a1-b1;
//   p43 = b2 - b1;
//   p21 = a2- a1;
//   d1343 = p13.x* p43.x + p13.y* p43.y + p13.z* p43.z;

//        d4321 = p43.x* p21.x + p43.y* p21.y + p43.z* p21.z;
//        d1321 = p13.x* p21.x + p13.y* p21.y + p13.z* p21.z;
//        d4343 = p43.x* p43.x + p43.y* p43.y + p43.z* p43.z;
//        d2121 = p21.x* p21.x + p21.y* p21.y + p21.z* p21.z;
//        denom = d2121* d4343 - d4321* d4321;
//        numer = d1343* d4321 - d1321* d4343;
//        local mua = numer / denom;
//        local mub = (d1343 + d4321 * (mua)) / d4343;
//        mua = clamp mua 0 1
//   mub = clamp mub 0 1

//    pa = a1 + p21* mua;
//        pb = b1 + p43* mub;   
	   
//   return distance pa pb;
//)
    }
}
