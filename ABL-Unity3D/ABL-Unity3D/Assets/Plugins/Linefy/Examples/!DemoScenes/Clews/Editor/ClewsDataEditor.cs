using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy;
using Linefy.Internal;

namespace LinefyExamples {

    [CustomEditor(typeof(ClewData))]
    public class ClewsDataEditor : Editor {

        SerializedProperty prp_knotsCount;
        SerializedProperty prp_segmentsCount;

        private void OnEnable() {
            prp_knotsCount = serializedObject.FindProperty("knotsCount");
            prp_segmentsCount = serializedObject.FindProperty("segmentsCount");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(prp_knotsCount);
            EditorGUILayout.PropertyField(prp_segmentsCount);

            serializedObject.ApplyModifiedProperties();

            ClewData t = target as ClewData;

            if (GUILayout.Button("Generate clew")) {
                HermiteSplineClosed hermiteSpline = new HermiteSplineClosed(prp_knotsCount.intValue, prp_segmentsCount.intValue, false, -1);
                for (int i = 0; i < hermiteSpline.knotsCount; i++) {
                    hermiteSpline[i] = Random.insideUnitSphere * 1;
                }
                hermiteSpline.ApplyKnotsPositions();

                Edge3D[] edges;
                PolylineVertex[] vertices = new PolylineVertex[hermiteSpline.points.Length - 1];


                edges = new Edge3D[vertices.Length];
                for (int i = 0; i < vertices.Length; i++) {
                    Vector3 a = hermiteSpline.points[i];
                    Vector3 b = hermiteSpline.points[i + 1];
                    edges[i] = new Edge3D(a, b);
                }

                float[] pointsAO = new float[hermiteSpline.points.Length - 1];
                float minao = float.MaxValue;
                float maxao = float.MinValue;

                for (int p = 0; p < pointsAO.Length; p++) {
                    Vector3 point = hermiteSpline.points[p];
                    for (int e = 0; e < edges.Length; e++) {
                        pointsAO[p] += edges[e].GetDistance(point);
                    }
                    minao = Mathf.Min(minao, pointsAO[p]);
                    maxao = Mathf.Max(maxao, pointsAO[p]);
                }

                for (int i = 0; i < pointsAO.Length; i++) {
                    pointsAO[i] = Mathf.InverseLerp(minao, maxao, pointsAO[i]) * 0.8f + 0.2f;
                }


                for (int i = 0; i < vertices.Length; i++) {
                    float aoa = pointsAO[i];
                    Color ca = new Color(aoa, aoa, aoa, 1);
                    vertices[i] = new PolylineVertex(hermiteSpline.points[i], ca, 4);
                }

                Polyline polyline = new Polyline(vertices.Length);
                polyline.name = Random.Range(int.MinValue, int.MaxValue).ToString();
                polyline.isClosed = true;
                polyline.boundSize = 4;

                for (int v = 0; v < polyline.count; v++) {
                    polyline[v] = vertices[v];
                }
                polyline.SaveSerializationData( ref t.clewData );
                t.vertices = new PolylineVertex[polyline.count];
                for (int i = 0; i< polyline.count; i++) {
                    t.vertices[i] = polyline[i];
                }

                EditorUtility.SetDirty(t);
            }

            //if (t.clewData.vertices != null) {
            //    EditorGUILayout.LabelField(string.Format("{0} polyline vertices", t.clewData.vertices.Length));
            //}
            //DrawDefaultInspector();
        }
    }
}
