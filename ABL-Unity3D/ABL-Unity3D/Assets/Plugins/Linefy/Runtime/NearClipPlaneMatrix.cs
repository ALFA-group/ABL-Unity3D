using UnityEngine;
using Linefy.Internal;

namespace Linefy {

    [HelpURL("https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#NearClipPlaneMatrix")]
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(0)]
    [ExecuteInEditMode]
    public class NearClipPlaneMatrix : MonoBehaviour {

        [Matrix4x4Inspector(false)]
        public Matrix4x4 gui;

        [Matrix4x4Inspector(false)]
        public Matrix4x4 screen;

        [HideInInspector]
        public Rect cameraPixelRect;
        [Range(0, 1)]
        public float offset = 0.01f;

        [HideInInspector]
        [SerializeField]
        Camera _parentCamera;
        public Camera parentCamera {
            get {
                if (_parentCamera == null) {
                    _parentCamera = GetComponent<Camera>();
                }
                return _parentCamera;
            }
        }

        void LateUpdate() {
            gui = Matrix4x4Utility.NearClipPlaneGUISpaceMatrix(parentCamera, offset);
            screen = Matrix4x4Utility.NearClipPlaneScreenSpaceMatrix(parentCamera, offset);
            cameraPixelRect = parentCamera.pixelRect;
        }

        public static Matrix4x4 GUISpace(Camera camera, float offset) {
            return Matrix4x4Utility.NearClipPlaneGUISpaceMatrix(camera, offset);
        }

        public static Matrix4x4 GUISpace(Camera camera) {
            float offset = (camera.farClipPlane - camera.nearClipPlane) * 0.001f;
            return Matrix4x4Utility.NearClipPlaneGUISpaceMatrix(camera, offset);
        }

        public static Matrix4x4 ScreenSpace(Camera camera, float offset) {
            return Matrix4x4Utility.NearClipPlaneScreenSpaceMatrix(camera, offset);
        }

        public static Matrix4x4 ScreenSpace(Camera camera) {
            float offset = (camera.farClipPlane - camera.nearClipPlane) * 0.001f;
            return Matrix4x4Utility.NearClipPlaneScreenSpaceMatrix(camera, offset);
        }
    }


}
