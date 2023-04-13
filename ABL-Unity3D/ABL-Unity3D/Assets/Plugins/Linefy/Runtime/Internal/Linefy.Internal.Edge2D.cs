using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {

    public struct Edge2D {
        public Vector2 a;
        public Vector2 b;
        public Vector2 ab;
        public float length;
        float lengthSquare;

        public Edge2D(Vector2 a, Vector2 b) {
            this.a = a;
            this.b = b;
            ab = this.b - this.a;
            length = ab.magnitude;
            lengthSquare = length * length;
        }

        public float GetDistance(Vector2 point ) {
            Vector2 ap = point - a;
            float u = Vector2.Dot(ap, ab) / lengthSquare;
            Vector3 nearestPoint = Vector3.zero;
            if (u < 0) {
                 nearestPoint = a;
            } else if (u > 1) {
                nearestPoint = b;
            } else {
                nearestPoint = a + ab * u;
            }
            return Vector2.Distance(nearestPoint, point);
        }

        public float GetDistance(Vector2 point, ref float lv) {
            Vector2 ap = point - a;
            float u = Vector2.Dot(ap, ab) / lengthSquare;
            Vector3 nearestPoint = Vector3.zero;
            if (u < 0) {
                lv = 0;
                nearestPoint = a;
            } else if (u > 1) {
                lv = 1f;
                nearestPoint = b;
            } else {
                lv = u;
                nearestPoint = a + ab * u;
            }
            return Vector2.Distance(nearestPoint, point);
        }

        public float GetLV(Vector2 point ) {
            Vector2 ap = point - a;
            return   Vector2.Dot(ap, ab) / lengthSquare;
 
        }

        public static float GetDistance(Vector2 a, Vector2 b, Vector2 point, ref float lv) {
            if (a.EqualsApproximately(b)) {
                lv = 0;
                return Vector2.Distance(a, point);
            }
            Vector2 _ab = b - a;
            float _length = _ab.magnitude;
            float _length2 = _length * _length;
            Vector2 ap = point - a;
            float u = Vector2.Dot(ap, _ab) / _length2;
            Vector3 nearestPoint = Vector3.zero;
            if (u < 0) {
                nearestPoint = a;
                lv = 0;
            } else if (u > 1) {
                nearestPoint = b;
                lv = 1f;
            } else {
                nearestPoint = a + _ab * u;
                lv = u;
            }
            return Vector2.Distance(nearestPoint, point);
        }

        public static float GetDistance(Vector2 a, Vector2 b, Vector2 point ) {
            Vector2 _ab = b - a;
            float _length = _ab.magnitude;
            float _length2 = _length * _length;
            Vector2 ap = point - a;
            float u = Vector2.Dot(ap, _ab) / _length2;
            Vector3 nearestPoint = Vector3.zero;
            if (u < 0) {
                nearestPoint = a;
             } else if (u > 1) {
                nearestPoint = b;
            } else {
                nearestPoint = a + _ab * u;
            }
            return Vector2.Distance(nearestPoint, point);
        }

        public static float GetDistance(Vector2 a, Vector2 b, Vector2 point, ref float lv, ref float slope) {
            Vector2 _ab = b - a;
            float _length = _ab.magnitude;
            float _length2 = _length * _length;
            Vector2 ap = point - a;
            float u = Vector2.Dot(ap, _ab) / _length2;
            Vector2 nearestPoint = Vector2.zero;
            if (u < 0) {
                nearestPoint = a;
                lv = 0;
            } else if (u > 1) {
                nearestPoint = b;
                lv = 1f;
            } else {
                nearestPoint = a + _ab * u;
                lv = u;
            }

            slope = Mathf.Abs(Vector2.Dot((nearestPoint - point).normalized, _ab / _length));
            return Vector2.Distance(nearestPoint, point);
        }

        public static float RotationAngle(Vector2 a, Vector2 b) {
            Vector2 ab = b - a;
            return Mathf.Atan2(ab.y, ab.x);
        }

        public static float RotationAngle(Vector2 dir) {
            return Mathf.Atan2(dir.y, dir.x);
        }

        public static Vector2 Rotate90(Vector2 vector) {
            return new Vector2(vector.y, -vector.x);
        }

        public static bool LineLineItersection(Vector2 redOrigin, Vector2 redDir, Vector2 greenOrigin, Vector2 greenDir, ref Vector2 intersection) {
            bool redVertical = MathUtility.ApproximatelyEquals(redDir.x, 0);
            bool redHorizontal = MathUtility.ApproximatelyEquals(redDir.y, 0);
            bool greenVertical = MathUtility.ApproximatelyEquals(greenDir.x, 0);
            bool greenHorizontal = MathUtility.ApproximatelyEquals(greenDir.y, 0);

            if (redHorizontal && greenHorizontal) {
                return false;
            }

            if (redVertical && greenVertical) {
                return false;
            }

            if (redHorizontal) {
                intersection.x = greenOrigin.x + (redOrigin.y - greenOrigin.y) * (greenDir.x / greenDir.y);
                intersection.y = redOrigin.y;
                return true;
            }

            if (redVertical) {
                intersection.x = redOrigin.x;
                intersection.y = greenOrigin.y + (redOrigin.x - greenOrigin.x) * (greenDir.y / greenDir.x);
                return true;
            }

            if (greenHorizontal) {
                intersection.x = redOrigin.x + (greenOrigin.y - redOrigin.y) * (redDir.x / redDir.y);
                intersection.y = greenOrigin.y;
                return true;
            }

            if (greenVertical) {
                intersection.x = greenOrigin.x;
                intersection.y = redOrigin.y + (greenOrigin.x - redOrigin.x) * (redDir.y / redDir.x);
                return true;
            }

            float slope0 = redDir.y / redDir.x;
            float slope1 = greenDir.y / greenDir.x;

            if (MathUtility.ApproximatelyEquals(slope0, slope1)) {
                return false;
            }

            float yi0 = redOrigin.y - slope0 * redOrigin.x;
            float yi1 = greenOrigin.y - slope1 * greenOrigin.x;
            intersection.x = (yi1 - yi0) / (slope0 - slope1);
            intersection.y = slope0 * intersection.x + yi0;
            return true;
        }

        public static bool LineLineItersection(Ray2D r0, Ray2D r1, ref Vector2 intersection) {
            return LineLineItersection(r0.origin, r0.direction, r1.origin, r1.direction, ref intersection);
        }

        public static float SignedAngle(Vector2 dirA, Vector2 dirB) {
            dirA.Normalize();
            dirB.Normalize();
            float sign = Vector2.Dot(new Vector2(dirA.y, -dirA.x), dirB);
            float a = Mathf.Acos(Vector2.Dot(dirA, dirB)) * Mathf.Rad2Deg;
            return sign < 0 ? 360f - a : a;
        }
    }
}