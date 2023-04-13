using UnityEditor;
using UnityEngine;

namespace Linefy{
    [CustomEditor(typeof(SerializedPolygonalMesh))]
    public class SerializedPolygonalMeshEditor : Editor  {
        bool showField;

        public override void OnInspectorGUI() {
            EditorGUILayout.HelpBox("Showing fields may slow down the editor", MessageType.Info);
            showField = EditorGUILayout.Toggle("Show fields", showField );
            if (showField) {
                DrawDefaultInspector();
            }
        }

        protected override void OnHeaderGUI() {
            base.OnHeaderGUI();
            if (Event.current.type == EventType.Repaint) {
                Rect r = GUILayoutUtility.GetLastRect();
                r.position += new Vector2(44, 22);
                GUI.Label(r, "Serialized Polygonal Mesh", EditorStyles.miniLabel);
            }
        }
    }
}
