using Linefy;
using UnityEditor;
using UnityEngine;

namespace LinefyExamples {
    [CustomEditor(typeof(LinefyDemo_EditorHandles))]
    public class LinefyDemo_EditorHandlesEditor : Editor {

        SerializedPropertyMatrix4x4Handle[] matrixHandles;
        SerializedPropertyVector3Handle[] pointHandles;
        SerializedProperty prp_handlesSize;
        SerializedProperty prp_drawMeshes;
        SerializedProperty prp_mesh;
        SerializedProperty prp_material;

        bool drawIDLabels = true;
        bool drawNameLabels;
        PolygonalMesh fill;

        public bool drawMatrixHandles = true;
        public bool drawVectorHandles = true;


        private void OnEnable() {
            Matrix4x4Handle jj = new Matrix4x4Handle("ff", 1);
             SerializedObject so = serializedObject;
            matrixHandles = new SerializedPropertyMatrix4x4Handle[4];
            for (int i = 0; i < matrixHandles.Length; i++) {
                SerializedProperty sp = so.FindProperty(string.Format("matrix{0}", i));
                matrixHandles[i] = new SerializedPropertyMatrix4x4Handle(sp, i, OnDragMatrixBegin, null, OnDragMatrixEnd);
            }
            Vector3Handle.Style vector3HandleStyle = new Vector3Handle.Style(12, new Color(0, 1, 0, 1), new Color(0, 1, 0, 0.4f), DefaultDotAtlasShape.Hexagon, 4);
            pointHandles = new SerializedPropertyVector3Handle[7];
            for (int i = 0; i < pointHandles.Length; i++) {
                SerializedProperty sp = so.FindProperty(string.Format("point{0}", i));
                pointHandles[i] = new SerializedPropertyVector3Handle(sp, i, vector3HandleStyle, OnDragPointBegin, null, OnDragPointEnd);
            }

            prp_handlesSize = serializedObject.FindProperty("handlesSize");
            prp_drawMeshes = serializedObject.FindProperty("drawMeshes");
            prp_mesh = serializedObject.FindProperty("someMesh");
            prp_material = serializedObject.FindProperty("someMaterial");

            Polygon fillPolygon = new Polygon(0, 0, pointHandles.Length);
            for (int i = 0; i < fillPolygon.CornersCount; i++) {
                fillPolygon[i] = new PolygonCorner(i, 0, 0);
            }
            fill = PolygonalMesh.BuildProcedural(new Vector3[pointHandles.Length], null, new Color[] { new Color(0, 1, 0, 0.2f) }, new Polygon[] { fillPolygon });
            fill.transparent = true;
            fill.lighingMode = LightingMode.Unlit;
            fill.dynamicTriangulationThreshold = 4;
        }

        void OnDragMatrixBegin(int id) {
            Debug.LogFormat("matrix {0} begin drag", id);
        }

        void OnDragMatrixEnd(string name, int id, Matrix4x4 delta) {
            Vector3 posDelta = delta.GetColumn(3);
            if (posDelta.magnitude > 0.001f) {
                Debug.LogFormat("{0} drag performed. Position delta {1} ", name, posDelta.ToString("F2") );
                return;
            }
            Vector3 eulerDelta =  Quaternion.LookRotation( delta.GetColumn(2), delta.GetColumn(1)).eulerAngles;
            if (eulerDelta.magnitude > 0.001f) {
                Debug.LogFormat("{0} drag performed. Euler angles delta {1}", name, eulerDelta.ToString("F1"));
                return;
            }

            Vector3 scaleDelta =  new Vector3(delta.GetColumn(0).magnitude, delta.GetColumn(1).magnitude, delta.GetColumn(2).magnitude ) - Vector3.one;
            if (scaleDelta.magnitude > 0.001f) {
                Debug.LogFormat("{0} drag performed. Scale delta {1}", name, scaleDelta.ToString("F2"));
            }
 
        }

        void OnDragPointBegin(int id) {
 
        }

        void OnDragPointEnd(string name, int id, Vector3 delta) {
            Debug.LogFormat("{0} drag performed. Position delta {1}", name, delta.ToString("F2"));
        }



        private void OnSceneGUI() {
            LinefyDemo_EditorHandles t = target as LinefyDemo_EditorHandles;
            serializedObject.Update();
            Handles.matrix = t.transform.localToWorldMatrix;
            if (drawMatrixHandles) {
                for (int i = 0; i < matrixHandles.Length; i++) {
                    matrixHandles[i].DrawPropertyHandle(t.handlesSize, prp_drawMeshes.boolValue, drawIDLabels, drawNameLabels);
                }
            }

            for (int i = 0; i<pointHandles.Length; i++) {
                pointHandles[i].DrawPropertyHandle( drawIDLabels, drawNameLabels );
            }

            for (int i = 0; i<fill.positionsCount; i++) {
                fill.SetPosition(i, pointHandles[i].vector3property.vector3Value);
            }

            OnSceneGUIGraphics.DrawWorldspace(fill, Handles.matrix);
 
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(prp_mesh);
            EditorGUILayout.PropertyField(prp_material);

            EditorGUILayout.PropertyField(prp_handlesSize);
            EditorGUILayout.PropertyField(prp_drawMeshes);
            OnSceneGUIGraphics.showStatistic = EditorGUILayout.Toggle("show OnSceneGUIGraphics statistic", OnSceneGUIGraphics.showStatistic);
            
            drawIDLabels = EditorGUILayout.Toggle("Draw ID labels", drawIDLabels);
            drawNameLabels = EditorGUILayout.Toggle("Draw Name labels", drawNameLabels);

            for (int i = 0; i < matrixHandles.Length; i++) {
                EditorGUILayout.PropertyField(matrixHandles[i].matrixProperty);
            }

            for (int i = 0; i<pointHandles.Length; i++) {
                EditorGUILayout.PropertyField(pointHandles[i].vector3property);
            }
  
            serializedObject.ApplyModifiedProperties();

            drawMatrixHandles = EditorGUILayout.Toggle("draw matrices handles", drawMatrixHandles);
            drawVectorHandles = EditorGUILayout.Toggle("draw Vector handles", drawVectorHandles);
        }


        private void OnDisable() {
            for (int i = 0; i< matrixHandles.Length; i++) {
                matrixHandles[i].Dispose();
            }

            for (int i = 0; i < pointHandles.Length; i++) {
                pointHandles[i].Dispose();
            }

            fill.Dispose();
 
        }
    }
}
