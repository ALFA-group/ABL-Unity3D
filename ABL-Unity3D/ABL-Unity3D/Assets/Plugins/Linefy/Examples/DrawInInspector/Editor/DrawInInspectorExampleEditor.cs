using Linefy;
using Linefy.Primitives;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;

namespace LinefyExamples {
    [CustomEditor(typeof(DrawInInspectorExample))]
    public class DrawInInspectorExampleEditor : Editor {

        LabelsRenderer _labels;
        LabelsRenderer labels {
            get {
                if (_labels == null) {
                    _labels = new LabelsRenderer(3);
                    _labels[0] = new Label("Mouse", Vector3.zero, new Vector2(0, 25));
                    _labels[1] = new Label("Local zero");
                    _labels[2] = new Label("Draw in inspector example");
                    _labels.atlas = DotsAtlas.DefaultFont11pxShadow;
                    _labels.textColor = Color.cyan;
                    _labels.transparent = true;
                    _labels.size = 1;
                    _labels.renderOrder = 8;
					_labels.pixelPerfect = true;
                }
                return _labels;

            }
        }

        EditorGUIViewport _viewport;
        EditorGUIViewport viewport {
            get {
                if (_viewport == null) {
                    _viewport = new EditorGUIViewport();
                }
                return _viewport;
            }
        }

        CircularPolyline _triangle;
        CircularPolyline triangle {
            get {
                if (_triangle == null) {
                    _triangle = new CircularPolyline(3, 70);
                    _triangle.wireframeProperties.transparent = true;
                    _triangle.wireframeProperties.feather = 1;
                    _triangle.wireframeProperties.widthMultiplier = 3;
                }
                return _triangle;
            }
        }

        CircularPolyline _mouseCursor;
        CircularPolyline mouseCursor {
            get {
                if (_mouseCursor == null) {
                    _mouseCursor = new CircularPolyline(32, 10, new Linefy.Serialization.SerializationData_Polyline(4, Color.green, 1, true));
                    _mouseCursor.wireframeProperties.renderOrder = 9;
                }
                return _mouseCursor;
            }
        }

        Grid2d _localGrid;
        Grid2d localGrid {
            get {
                if (_localGrid == null) {
                    _localGrid = new Grid2d(100, 100, 10, 10, false, new Linefy.Serialization.SerializationData_LinesBase(1, new Color(0, 0, 0, 0.5f), 0));
                }
                return _localGrid;
            }
        }

        Grid2d _guigrid;
        Grid2d guigrid {
            get {
                if (_guigrid == null) {
                    _guigrid = new Grid2d(100, 100, 1, 1, false, new Linefy.Serialization.SerializationData_LinesBase(4, Color.black, 0));
                    _guigrid.wireframeProperties.renderOrder = 10;
                }
                return _guigrid;
            }
        }

        ResizableControlRect _resizable;
        ResizableControlRect resizable {
            get {
                if (_resizable == null) {
                    _resizable = new ResizableControlRect(false, 100, 2000, 300, null);
                }
                return _resizable;
            }
        }

        float rv;

        public override void OnInspectorGUI() {
            DrawInInspectorExample t = target as DrawInInspectorExample;
            Repaint();
            //Rect inspectorRect = EditorGUILayout.GetControlRect(false, 300).Inflate(-8);

            resizable.Draw();
            Rect inspectorRect = resizable.guiRect;

            if (Event.current.type == EventType.Repaint) {
                viewport.backgroundColor = t.backgroundColor;
                viewport.SetParams(inspectorRect, t.zoom, t.pan);
                viewport.DrawLocalSpace(localGrid, Matrix4x4.identity);
                guigrid.width = inspectorRect.width - 2;
                guigrid.height = inspectorRect.height - 2;
                Vector2 mousePosition = Event.current.mousePosition;
                labels[0] = new Label(string.Format("mouse {0}", mousePosition.ToString()), viewport.GUItoLocalSpace(Event.current.mousePosition), new Vector2(0, 20));
                labels.SetPosition(2, viewport.GUItoLocalSpace(new Vector3(inspectorRect.center.x, inspectorRect.yMin + 20)));
                viewport.DrawLocalSpace(labels);
                viewport.DrawGUIspace(guigrid, Matrix4x4.Translate(inspectorRect.center));
                viewport.DrawGUIspace(mouseCursor, Matrix4x4.Translate(Event.current.mousePosition));
                for (int i = 0; i < 20; i++) {
                    viewport.DrawLocalSpace(triangle, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rv + i * 18)));
                }
                viewport.Render();
                rv += 7 * OnSceneGUIGraphics.onScenGUIRepaintDeltaTime;
            }
            DrawDefaultInspector();
        }

        private void OnDestroy() {
            if (_viewport != null) {
                _viewport.Dispose();
                _viewport = null;
            }

            if (_triangle != null) {
                _triangle.Dispose();
                _triangle = null;
            }

            if (_mouseCursor != null) {
                _mouseCursor.Dispose();
                _mouseCursor = null;
            }

            if (_labels != null) {
                _labels.Dispose();
                _labels = null;
            }

            if (_guigrid != null) {
                _guigrid.Dispose();
                _guigrid = null;
            }

            if (_localGrid != null) {
                _localGrid.Dispose();
                _localGrid = null;
            }

 
        }
    }
}


