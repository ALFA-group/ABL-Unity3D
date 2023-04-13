using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy;

namespace LinefyExamples {
    [CustomEditor(typeof(CreatePolygonalMesh))]
    public class CreatePolygonalMeshProceduralEditor : Editor {

        Vector3ArrayHandle positionHandles ;
        SerializedProperty[] positionsProperties ;

        private void OnEnable() {
            Tools.hidden = false;
            positionHandles = new Vector3ArrayHandle(0, "Positions Handles");
            positionsProperties = new SerializedProperty[0];
        }

        private void OnSceneGUI() {
            CreatePolygonalMesh t = target as CreatePolygonalMesh;
            if (t.enablePositionsHandles) {
                if (positionsProperties.Length != t.positions.Length) {
                    positionsProperties = new SerializedProperty[t.positions.Length];
                    SerializedProperty arr = serializedObject.FindProperty("positions");
                    for (int i = 0; i < positionsProperties.Length; i++) {
                        positionsProperties[i] = arr.GetArrayElementAtIndex(i);
                    }
                }
                Handles.matrix = t.transform.localToWorldMatrix;
                positionHandles.onDragUpdate = OnPositionsMove;
                positionHandles.DrawOnSceneGUI(positionsProperties);

            } else {
                Tools.hidden = false;
            }
        }

        void OnPositionsMove(List<int> movedVertices) {
            CreatePolygonalMesh t = target as CreatePolygonalMesh;
            if (t.pm != null) {
                for (int i = 0; i<movedVertices.Count; i++) {
                    t.pm.SetPosition(movedVertices[i], positionsProperties[movedVertices[i]].vector3Value);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable() {
            positionHandles.Dispose();
        }
    }
}
