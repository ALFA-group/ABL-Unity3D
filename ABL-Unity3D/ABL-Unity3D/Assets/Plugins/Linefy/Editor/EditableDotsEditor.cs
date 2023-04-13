using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Linefy.Editors {
    [CustomEditor( typeof( EditableDots) )]
    public class EditableDotsEditor : EditableEditorBase {

 	void OnEnable(){
			OnEnableBase();
		}
		
		private void OnSceneGUI() {
            if (prp_enableOnSceneGUIEdit.boolValue) {
				if (prp_vertpositions == null || prp_vertpositions.Length != prp_itemsArray.arraySize  ) {
					prp_vertpositions = new SerializedProperty[prp_itemsArray.arraySize];
					for (int i = 0; i < prp_itemsArray.arraySize; i++) {
						prp_vertpositions[i] = prp_itemsArray.GetArrayElementAtIndex(i).FindPropertyRelative("position");
					}
				}
				if (verticesHandles == null) {
					verticesHandles = new Vector3ArrayHandle(prp_vertpositions.Length, "EditableDots");
					verticesHandles.onDragUpdate = OnVerticesHandlesMove;
				}

				EditableDots t = target as EditableDots;
				Handles.matrix = t.worldMatrix;
				serializedObject.Update();
				verticesHandles.DrawOnSceneGUI(prp_vertpositions);
			}
        }
		
		void OnVerticesHandlesMove(List<int> selected) {
            UpdateRuntimeObject();
        }

        public override void OnInspectorGUI() {
            OnInspectorGUIBase(OnSceneGUIGraphics.readme.onSceneGUIEditModeEditableLines);
        }

        private void OnDestoy() {
			if(verticesHandles != null){
				verticesHandles.Dispose();
			}
        }

        private void OnDisable() {
			if(verticesHandles != null){
				verticesHandles.Dispose();
			}
        }

        [MenuItem("GameObject/3D Object/Linefy/EditableDots", false, 3)]
        public static void Create(MenuCommand menuCommand) {
             GameObject go = EditableDots.CreateInstance().gameObject;
             postCreate(go, menuCommand);
        }

    }

}
