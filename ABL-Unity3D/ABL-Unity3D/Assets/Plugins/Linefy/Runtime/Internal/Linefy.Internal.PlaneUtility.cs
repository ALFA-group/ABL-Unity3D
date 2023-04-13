using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal{

    public static class PlaneExtension{

        public static bool RaycastXYToLocal(this Matrix4x4 tm, Ray r, ref Vector2 result) {
            Plane p = new Plane(tm.GetColumn(2), tm.GetColumn(3));
            float d = 0;
            if (p.Raycast(r, out d)) {
                result = tm.inverse.MultiplyPoint3x4(r.GetPoint(d));
                return true;
            }
            return false;
        }

        public static bool RaycastDoublesided(this Plane p, Ray r, ref Vector3 hit) {
            Vector3 result = r.origin;
            float d = 0;
            if (p.Raycast(r, out d)) {
                hit = r.GetPoint(d);
                return true;
            } else {
                Plane inversePlane = p;
                inversePlane.normal = -inversePlane.normal;
                if (inversePlane.Raycast(r, out d)) {
                    hit = r.GetPoint(d);
                    return true;
                }
            }
            return false;
        }
    }
}
