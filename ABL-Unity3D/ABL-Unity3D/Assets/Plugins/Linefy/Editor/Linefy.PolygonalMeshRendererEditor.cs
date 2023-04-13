using UnityEditor;
using UnityEngine;


namespace Linefy.Editors {

    [CustomEditor(typeof(PolygonalMeshRenderer))]
    [CanEditMultipleObjects]
    public class PolygonalMeshRendererEditor : Editor {

        SerializedProperty prp_polygonalMeshAsset;
        SerializedProperty prp_polygonalMeshProperties;
        SerializedProperty prp_drawDefault;
        SerializedProperty prp_wireframeEnabled;
        SerializedProperty prp_autoWireframeViewOffset;
        SerializedProperty prp_wireframeProperties;
 
        void OnEnable() {
            prp_polygonalMeshAsset = serializedObject.FindProperty("polygonalMeshAsset");
            prp_polygonalMeshProperties = serializedObject.FindProperty("polygonalMeshProperties");
            prp_drawDefault = serializedObject.FindProperty("drawDefault");
            prp_wireframeEnabled = serializedObject.FindProperty("wireframeEnabled");
            prp_autoWireframeViewOffset = serializedObject.FindProperty("autoWireframeViewOffset");
            prp_wireframeProperties = serializedObject.FindProperty("wireframeProperties");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(prp_polygonalMeshAsset);
            EditorGUILayout.PropertyField(prp_polygonalMeshProperties, true);
            EditorGUILayout.PropertyField(prp_drawDefault);
            EditorGUILayout.PropertyField(prp_wireframeEnabled);
            EditorGUI.BeginDisabledGroup(!prp_wireframeEnabled.boolValue);
            EditorGUILayout.PropertyField(prp_autoWireframeViewOffset);
            EditorGUILayout.PropertyField(prp_wireframeProperties, true);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

    }
}
