using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 
namespace Linefy.Editors {
	public class EditableEditorBase : MonoBehaviourEditorsBase{
		protected Vector3ArrayHandle verticesHandles;
        protected SerializedProperty prp_serializedProperties;
        protected SerializedProperty prp_itemsArray;
        protected SerializedProperty[] prp_vertpositions;
        protected SerializedProperty prp_propertiesModificationId;
        protected SerializedProperty prp_itemsModificationId;
        protected SerializedProperty prp_enableOnSceneGUIEdit;
		protected SerializedProperty prp_updatePropertiesAlways;
		protected SerializedProperty prp_updateItemsAlways;
		protected int hashCode;
		
		protected void OnEnableBase(){
			prp_itemsArray = serializedObject.FindProperty("items");
            prp_serializedProperties = serializedObject.FindProperty("properties");
            prp_propertiesModificationId = serializedObject.FindProperty("propertiesModificationId");
            prp_itemsModificationId = serializedObject.FindProperty("itemsModificationId");
            prp_enableOnSceneGUIEdit = serializedObject.FindProperty("enableOnSceneGUIEdit");
			prp_updatePropertiesAlways = serializedObject.FindProperty("updatePropertiesAlways");
			prp_updateItemsAlways = serializedObject.FindProperty("updateItemsAlways");
            hashCode = GetHashCode();
		}
		
		protected void OnDisableBase(){
			if (verticesHandles != null) {
                verticesHandles.Dispose();
            }
		}
		
        protected void UpdateRuntimeObject() {
            prp_itemsModificationId.intValue++;
			prp_propertiesModificationId.intValue++;
            serializedObject.ApplyModifiedProperties();
        }
		
		protected  void OnInspectorGUIBase(EditorIconContent editButtonContent) {
            serializedObject.Update();
            float buttonScale = 32 * EditorGUIUtility.pixelsPerPoint;
            float buttonMargin = buttonScale*0.1f;
            Rect r = EditorGUILayout.GetControlRect(false, buttonScale + buttonMargin*2);
            r = new Rect(buttonMargin *4 , r.y + buttonMargin, buttonScale, buttonScale);
            prp_enableOnSceneGUIEdit.boolValue = OnSceneGUIGraphics.DrawToggle(r, editButtonContent, prp_enableOnSceneGUIEdit.boolValue);
 
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prp_itemsArray, true);
              
            if (EditorGUI.EndChangeCheck()) {
                prp_itemsModificationId.intValue++;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prp_serializedProperties, true);
            if (EditorGUI.EndChangeCheck()) {
                prp_propertiesModificationId.intValue++;
            }
			
			EditorGUILayout.PropertyField(prp_updatePropertiesAlways);
			EditorGUILayout.PropertyField(prp_updateItemsAlways);
            serializedObject.ApplyModifiedProperties();
        }
	}
	
    public class MonoBehaviourEditorsBase : Editor {

        protected static void postCreate(GameObject go, MenuCommand menuCommand) {
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}
