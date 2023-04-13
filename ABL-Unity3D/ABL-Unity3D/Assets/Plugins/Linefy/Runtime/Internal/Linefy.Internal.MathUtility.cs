using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {
    
    public static class MathUtility {

        public static float HermiteValue(float y0, float y1, float y2, float y3, float t) {
            float mu2 = t * t;
            float mu3 = mu2 * t;
            float m0, m1;
            float a0, a1, a2, a3;
            m0 = (y1 - y0) / 2;
            m0 += (y2 - y1) / 2;
            m1 = (y2 - y1) / 2;
            m1 += (y3 - y2) / 2;
            a0 = 2 * mu3 - 3 * mu2 + 1;
            a1 = mu3 - 2 * mu2 + t;
            a2 = mu3 - mu2;
            a3 = -2 * mu3 + 3 * mu2;
            return (a0 * y1 + a1 * m0 + a2 * m1 + a3 * y2);
        }

        public static int RoundedArrayIdx(int idx, int arrLength) {
            if (arrLength == 0) {
                return 0;
            }
            idx = idx % arrLength;
            if (idx < 0) {
                idx = (arrLength + idx) % arrLength;
            }
            return idx;
        }

        public static float GetValue(this float[] arr, float time) {

            if (arr.Length == 0) {
                return 0;
            }

            if (time.LessOrEqualsThan(0)) {
                return arr[0];
            }

            if (time.GreaterOrEqualsThan(1)) {
                return arr[arr.Length - 1];
            }

            float step = 1f / (arr.Length - 1);
            int idx = Mathf.FloorToInt(time / step);
            float lv = (time - idx * step) / step;

            return Mathf.LerpUnclamped(arr[idx], arr[idx + 1], lv);
        }

        public static float LinearToSin(float t) {
            return 1f - (Mathf.Sin((t * 3.141592f) + 1.5708f) * 0.49999f + 0.5f);
        }

        public static float InverseLerpUnclamped(float a, float b, float value) {
            float result;
            if (a != b) {
                result = (value - a) / (b - a);
            } else {
                result = 0;
            }
            return result;
        }

        public static bool ApproximatelyZero(float f) {
            return f < 0.00001f && f > -0.00001f;
        }

        public static bool ApproximatelyEquals(float a, float b) {
            return ApproximatelyZero(a - b);
        }

        public static bool EqualsApproximately(this float a, float b) {
            return ApproximatelyZero(a - b);
        }

        public static float ToDegrees(this float f) {
            return f * Mathf.Rad2Deg;
        }

        public static float ToRadians(this float f) {
            return f * Mathf.Deg2Rad;
        }

        public static float DeltaAngleRad(float current, float target) {
            float num = Mathf.Repeat(target - current, 6.283185f);
            if (num >= Mathf.PI) {
                num -= 6.283185f;
            }
            return num;
        }

        public static int RepeatIdx(int idx, int length) {
            if (length == 0) {
                Debug.LogError("Zero length");
                return 0;
            }
            idx = idx % length;
            if (idx < 0) {
                idx = length + idx;
            }
            return idx;
        }

        public static bool LessOrEqualsThan(this float f, float val) {
            return f < val || MathUtility.ApproximatelyEquals(f, val);
        }

        public static bool GreaterOrEqualsThan(this float f, float val) {
            return f > val || MathUtility.ApproximatelyEquals(f, val);
        }
    }
}
