using UnityEngine;

namespace LinefyExamples {
    public class DrawGizmosExample : MonoBehaviour {

        public Color gizmoColor = Color.white;
        public Color selectedGizmoColor = Color.red;
        public Texture gizmosTexture;

        Linefy.Primitives.CircularPolyline gizmoCircle;
        Linefy.Primitives.CircularPolyline gizmoSelectedCircle;


        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (gizmoCircle == null) {
                gizmoCircle = new Linefy.Primitives.CircularPolyline(64, 1);
                gizmoCircle.wireframeProperties.widthMultiplier = 5;
            }
            gizmoCircle.wireframeProperties.colorMultiplier = gizmoColor;
            gizmoCircle.DrawNow(Gizmos.matrix);
        }

        private void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (gizmoSelectedCircle == null) {
                gizmoSelectedCircle = new Linefy.Primitives.CircularPolyline(64, 1.1f);
                gizmoSelectedCircle.wireframeProperties.widthMultiplier = 3;
            }
            gizmoSelectedCircle.wireframeProperties.colorMultiplier = selectedGizmoColor;
            gizmoSelectedCircle.wireframeProperties.textureScale = 40;
            gizmoSelectedCircle.wireframeProperties.texture = gizmosTexture;
            gizmoSelectedCircle.DrawNow(Gizmos.matrix);
        }

        private void OnDisable() {
            if (gizmoCircle != null) {
                gizmoCircle.Dispose();
                gizmoCircle = null;
            }
        }
    }
}
