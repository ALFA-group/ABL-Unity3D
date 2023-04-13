using UnityEngine;

namespace Linefy {
    public static class RectTransformExtensions {
        static Vector3[] fourWorldPoint = new Vector3[4];

        public static Matrix4x4 GetCenteredWorldMatrix(this RectTransform rt) {
            rt.GetWorldCorners(fourWorldPoint);
            Vector4 pos = Vector3.LerpUnclamped(fourWorldPoint[0], fourWorldPoint[2], 0.5f);
            pos.w = 1;
            return new Matrix4x4(fourWorldPoint[3] - fourWorldPoint[0], fourWorldPoint[1] - fourWorldPoint[0], rt.transform.forward, pos);
        }

        public static Matrix4x4 GetWorldMatrix(this RectTransform rt) {
            rt.GetWorldCorners(fourWorldPoint);
            Vector4 pos = fourWorldPoint[0];
            pos.w = 1;
            return new Matrix4x4(fourWorldPoint[3] - fourWorldPoint[0], fourWorldPoint[1] - fourWorldPoint[0], rt.transform.forward, pos);
        }
    }
}