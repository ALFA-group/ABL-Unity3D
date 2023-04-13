using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy;
using Linefy.Primitives;

namespace LinefyExamples {
    public class DrawInEditorWindow : MonoBehaviour {
        public class CustomEditorWindow : EditorWindow {

            EditorGUIViewport viewport;
            CircularPolyline circle;
            LabelsRenderer label;
            float rot;
            float rot2;

            [MenuItem("Window/Linefy Draw In Editor Window Example", false, 0)]
            public static void OpenCustomWindow() {
                EditorWindow.GetWindow(typeof(CustomEditorWindow), false, "Draw In Editor Window Example");
            }

            private void OnEnable() {
                viewport = new EditorGUIViewport();
                circle = new CircularPolyline(3, 1, new Linefy.Serialization.SerializationData_Polyline(2, Color.green, 1, true));
                autoRepaintOnSceneChange = true;
                label = new LabelsRenderer(1);
                label[0] = new Label("Draw In Editor Window Example", Vector3.zero, Vector2.zero);
                label.atlas = DotsAtlas.DefaultFont11pxShadow;
                label.size = 3;
                label.textColor = Color.yellow;
                label.transparent = true;
                label.renderOrder = 1;

            }

            private void OnGUI() {
                Repaint();
                if (Event.current.type == EventType.Repaint) {
                    Rect r = new Rect(8, 8, position.width - 16, position.height - 16);
                    float maxSize = Mathf.Min(r.width, r.height) * 0.48f;
                    viewport.SetParams(r);
                    for (int i = 0; i < 36; i++) {
                        float _rot = i % 2 == 0 ? rot : rot2;
                        Matrix4x4 tm = Matrix4x4.TRS(viewport.GUItoLocalSpace(r.center), Quaternion.Euler(0, 0, _rot + i * 120f / 36), Vector3.one * maxSize);
                        viewport.DrawLocalSpace(circle, tm);
                    }
                    viewport.DrawGUIspace(label, Matrix4x4.Translate(r.center));
                    viewport.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                    viewport.Render();
                    rot += 6f * OnSceneGUIGraphics.onScenGUIRepaintDeltaTime;
                    rot2 += 7f * OnSceneGUIGraphics.onScenGUIRepaintDeltaTime;
                }
            }

        }
    }
}



