using UnityEditor;
using UnityEngine;
using Linefy;
using Linefy.Primitives;

namespace LinefyExamples {
    [CustomEditor(typeof(DrawInSceneViewExample))]
    public class DrawInSceneViewExampleEditor : Editor {
        CircularPolyline screenMouseCircle;
        CircularPolyline screenObjectCircle;
        CircularPolyline worldCircle;

        void OnEnable() {
            screenMouseCircle = new CircularPolyline(6, 50, Color.green);
            screenObjectCircle = new CircularPolyline(7, 100, Color.blue);
            worldCircle = new CircularPolyline(8, 1, Color.red);
        }

        private void OnSceneGUI() {
            DrawInSceneViewExample t = target as DrawInSceneViewExample;
            OnSceneGUIGraphics.DrawWorldspace(worldCircle, t.transform.localToWorldMatrix);
            OnSceneGUIGraphics.DrawGUIspace(screenMouseCircle, Matrix4x4.Translate(Event.current.mousePosition));
            Vector2 objectGuiPoint = OnSceneGUIGraphics.WorldToGUIPoint(t.transform.position);
            OnSceneGUIGraphics.DrawGUIspace(screenObjectCircle, Matrix4x4.Translate((Vector2)objectGuiPoint));
          
        }
    }
}
