using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;
using Linefy;

namespace LinefyExamples{
    [CustomEditor(typeof(LinefyPlexus))]

    public class PlexusEditor : Editor{
        SerializedProperty prp_size;
        SerializedProperty prp_cameraFocusDistance;
        Vector3Handle cornerHandle;
        Vector3Handle focusDistanceHandle;
 
        public void OnEnable() {
            prp_size = serializedObject.FindProperty("size");
            prp_cameraFocusDistance = serializedObject.FindProperty("cameraFocusDistance");
            cornerHandle = new Vector3Handle(0, new Vector3Handle.Style(16, Color.white, Color.white, DefaultDotAtlasShape.RoundOutline, 4 ) );
            focusDistanceHandle = new Vector3Handle( 1, new Vector3Handle.Style(16, Color.yellow, Color.yellow, DefaultDotAtlasShape.RhombusOutline, 4));
        }
 
        private void OnSceneGUI() {
            LinefyPlexus t = target as LinefyPlexus;
            serializedObject.Update();

            if (t.drawSizeHandles) {
                Handles.matrix = t.transform.localToWorldMatrix;
                Vector3 sizeValue = prp_size.vector3Value;
                Vector3 cornerPos = sizeValue * 0.5f;
                cornerPos.x = Mathf.Max(0, cornerPos.x);
                cornerPos.y = Mathf.Max(0, cornerPos.y);
                cornerPos.z = Mathf.Max(0, cornerPos.z);
                cornerPos = cornerHandle.DrawOnSceneGUI(cornerPos);
                prp_size.vector3Value = cornerPos * 2;
            }
              
            if (t.cam != null) {
                Handles.matrix = t.cam.transform.localToWorldMatrix;
                Vector3 focusPoint = new Vector3(0, 0, prp_cameraFocusDistance.floatValue);
                focusPoint = focusDistanceHandle.DrawOnSceneGUI(focusPoint);
                prp_cameraFocusDistance.floatValue = focusPoint.z;
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI() {
            if (Event.current.type == EventType.Layout) {
                Repaint();
            }
            DrawDefaultInspector();
            LinefyPlexus t = target as LinefyPlexus;
            if (t.plexus != null) {
                string info = string.Format("{0} connections \n {1} lines \n {2} dots", t.plexus.info_connectionsCount, t.plexus.info_linesCount, t.plexus.info_dotsCount);
                GUILayout.Label(info, EditorStyles.helpBox);
            }
        }
    }

}
