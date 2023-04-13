using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Linefy.Editors {
    [CustomEditor( typeof( EditableLines) )]

    public class EditableLinesEditor : EditableEditorBase {
 		
		void OnEnable(){
			OnEnableBase();
		}
		
		private void OnSceneGUI() {
            if (prp_enableOnSceneGUIEdit.boolValue) {
				if (prp_vertpositions == null || prp_vertpositions.Length != prp_itemsArray.arraySize*2  ) {
					prp_vertpositions = new SerializedProperty[prp_itemsArray.arraySize*2];
					for (int i = 0; i < prp_itemsArray.arraySize; i++) {
						int pi = i*2;
						prp_vertpositions[pi] = prp_itemsArray.GetArrayElementAtIndex(i).FindPropertyRelative("positionA");
						prp_vertpositions[pi+1] = prp_itemsArray.GetArrayElementAtIndex(i).FindPropertyRelative("positionB");
					}
				}
				if (verticesHandles == null) {
					verticesHandles = new Vector3ArrayHandle(prp_vertpositions.Length, "EditablePolyline");
					verticesHandles.onDragUpdate = OnVerticesHandlesMove;
				}

				EditableLines t = target as EditableLines;
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

        [MenuItem("GameObject/3D Object/Linefy/EditableLines", false, 0)]
        public static void CreateGrid(MenuCommand menuCommand) {
            GameObject go = EditableLines.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }

    }

}
