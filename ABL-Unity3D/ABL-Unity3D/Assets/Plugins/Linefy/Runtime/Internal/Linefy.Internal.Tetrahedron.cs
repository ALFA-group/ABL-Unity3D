using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal{

    public class Tetrahedron  {
        Vector3 a;
        Vector3 b;
        Vector3 c;
        Vector3 d;
        Triangle3D t;
        public Matrix4x4 tm;
        public Matrix4x4 tminverse;

        public Tetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            t = new Triangle3D(a, b, c);
            tm.SetColumn(0, b - a);
            tm.SetColumn(1, d - a);
            tm.SetColumn(2, c - a);
            tm.SetColumn(3, new Vector4(a.x, a.y, a.z, 1f));
            tminverse = tm.inverse;
        }

        public bool Test(Vector3 point, ref Vector4 coords) {
            Ray r = new Ray(d, point - d);
            Vector3 bary = Vector3.zero;
            Vector3 hit = Vector3.zero;
            if (t.Raycast(r, ref bary, ref hit)) {
                coords = bary;
                
                float hdDist = Vector3.Distance(d, hit);
                float pointDDist = Vector3.Distance(d, point);
 

                if (pointDDist > hdDist) {
                    return false;
                } else {
                    coords.w = pointDDist / hdDist;
                }
                return true;
            }
            return false;
        }

        public static Color CalcColor(Vector4 adress, Color ca, Color cb, Color cc, Color cd) {
            Color result = Color.LerpUnclamped( cd, ca * adress.x + cb * adress.y + cc * adress.z, adress.w);
            return result;
        }

        public void DrawDebug(Color color) {
            Debug.DrawLine(a, b, color);
            Debug.DrawLine(b, c, color);
            Debug.DrawLine(c, a, color);
            Debug.DrawLine(d, a, color);
            Debug.DrawLine(d, b, color);
            Debug.DrawLine(d, c, color);
        }

        public float GetVolume() {
 
            //Matrix4x4 tm = new Matrix4x4();
            //tm.SetColumn(0, b - a);
            //tm.SetColumn(1, d - a);
            //tm.SetColumn(2, c - a);
            //tm.SetColumn(3, new Vector4(0, 0, 0, 1f));
            return tm.determinant*0.1666666f;
        }
 
    }
}
