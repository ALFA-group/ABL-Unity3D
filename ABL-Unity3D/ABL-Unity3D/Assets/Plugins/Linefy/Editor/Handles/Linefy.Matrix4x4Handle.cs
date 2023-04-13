using System;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;
using Linefy.Editors.Internal;

namespace Linefy {

    public class Matrix4x4Handle {

        class ShiftMode {
            public Matrix4x4 onStartTM;
            public Matrix4x4 onStartWorldShiftTM;
            public Matrix4x4 localInShiftTM;
            public Matrix4x4 delta;

            public ShiftMode(Matrix4x4 initialTM, Camera c) {
                onStartTM = initialTM;
                Matrix4x4 worldSpaceTM = Handles.matrix * initialTM;
                onStartWorldShiftTM = Matrix4x4.TRS(worldSpaceTM.GetColumn(3), c.transform.rotation, Vector3.one);
                localInShiftTM = onStartWorldShiftTM.inverse * worldSpaceTM;
            }

            public Matrix4x4 Result {
                get {
                    Matrix4x4 result = Handles.inverseMatrix * (onStartWorldShiftTM * localInShiftTM);
                    delta = onStartTM.inverse * result;
                    return result;
                }
            }
        }

        string _name;
        public string name {
            get {
                return _name;
            }
        }

        int _id;
        public int id {
            get {
                return _id;
            }
        }

        public bool enableShiftMode;

        Vector3Handle freeMoveCenter;

        Color[] axisColors = { Color.red, Color.green, new Color32(64, 140, 255, 255) };

        MoveAxis moveXAxis;
        MoveAxis moveYAxis;
        MoveAxis moveZAxis;
 
        RotationAxis rotationXaxis;
        RotationAxis rotationYaxis;
        RotationAxis rotationZaxis;

        ScaleAxis scaleXaxis;
        ScaleAxis scaleYaxis;
        ScaleAxis scaleZaxis;

        ShiftMode shiftMode;
        public Action<int> onDragStart;
        public Action<int> onDragUpdate;
        public Action<string, int,Matrix4x4> onDragEnd;

        Lines axisLines;

        bool drawIdLabel;
        bool drawNameLabel;
        public bool displayFreemoveCenterGraphics = true;

        int _segments = 48;
        public int segments {
            get {
                return _segments;
            }

            set {
                if (_segments != value) {
                    _segments = value;
                    rotationXaxis = new RotationAxis(this._name + ".RotateX", 0, 0, axisColors[0], segments, onDragStart, onDragUpdate, OnDragEndInternal);
                    rotationYaxis = new RotationAxis(this._name + ".RotateY", 0, 1, axisColors[1], segments, onDragStart, onDragUpdate, OnDragEndInternal);
                    rotationZaxis = new RotationAxis(this._name + ".RotateZ", 0, 2, axisColors[2], segments, onDragStart, onDragUpdate, OnDragEndInternal);
                }
            }
        }

        float _styleThickness = 0;
        public float styleThickness {
            get {
                return _styleThickness;
            }

            set {
                if (_styleThickness != value) {
                    moveXAxis.styleThickness = value;
                    moveYAxis.styleThickness = value;
                    moveZAxis.styleThickness = value;

                    rotationXaxis.styleThickness = value;
                    rotationYaxis.styleThickness = value;
                    rotationZaxis.styleThickness = value;

                    scaleXaxis.styleThickness = value;
                    scaleYaxis.styleThickness = value;
                    scaleZaxis.styleThickness = value;

                    _styleThickness = value;
                }
            }
        }

        public Matrix4x4Handle(string name, int id  ) {
            InternalCtor(name, id, null, null, null);
        }

        public Matrix4x4Handle( string name, int id, Action<int> onDragStart, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd ) {
            InternalCtor(name, id, onDragStart, onDragUpdate, onDragEnd);
        }

        void InternalCtor(string name, int id, Action<int> onDragStart, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd) {
            this._name = name;
            this._id = id;
            this.onDragStart = onDragStart;
            this.onDragUpdate = onDragUpdate;
            this.onDragEnd = onDragEnd;

            freeMoveCenter = new Vector3Handle(this._name, id, onDragStart, onDragUpdate, OnDragEndFreemoveInternal);
            freeMoveCenter.style = new Vector3Handle.Style(10, Color.white, new Color(1, 1, 1, 0.0f), DefaultDotAtlasShape.Round, 3);

            freeMoveCenter.drawOnTop = true;

            scaleXaxis = new ScaleAxis(this._name + ".ScaleXAxis", id, 0, axisColors[0], onDragStart, onDragUpdate, OnDragEndInternal);
            scaleYaxis = new ScaleAxis(this._name + ".ScaleYAxis", id, 1, axisColors[1], onDragStart, onDragUpdate, OnDragEndInternal);
            scaleZaxis = new ScaleAxis(this._name + ".ScaleZAxis", id, 2, axisColors[2], onDragStart, onDragUpdate, OnDragEndInternal);

            moveXAxis = new MoveAxis(this._name + ".MoveXAxis", id, 0, axisColors[0], onDragStart, onDragUpdate, OnDragEndInternal);
            moveYAxis = new MoveAxis(this._name + ".MoveYAxis", id, 1, axisColors[1], onDragStart, onDragUpdate, OnDragEndInternal);
            moveZAxis = new MoveAxis(this._name + ".MoveZAxis", id, 2, axisColors[2], onDragStart, onDragUpdate, OnDragEndInternal);

            rotationXaxis = new RotationAxis(this._name + ".RotateX", id, 0, axisColors[0], segments, onDragStart, onDragUpdate, OnDragEndInternal);
            rotationYaxis = new RotationAxis(this._name + ".RotateY", id, 1, axisColors[1], segments, onDragStart, onDragUpdate, OnDragEndInternal);
            rotationZaxis = new RotationAxis(this._name + ".RotateZ", id, 2, axisColors[2], segments, onDragStart, onDragUpdate, OnDragEndInternal);

            axisLines = new Lines("Axis", 3, true, 1);
            axisLines[0] = new Line(2, axisColors[0]);
            axisLines[1] = new Line(2, axisColors[1]);
            axisLines[2] = new Line(2, axisColors[2]);

            styleThickness = 1;
        }

        void OnDragEndFreemoveInternal(string name, int id, Vector3 delta) {
            if (onDragEnd != null) {
                Matrix4x4 deltaMatrix = Matrix4x4.Translate(delta);
                onDragEnd(name, id, deltaMatrix);
 
            }
        }

        void OnDragEndInternal(string name, int id, Matrix4x4 delta) {
            if (onDragEnd != null) {
                if (shiftMode == null) {
                    onDragEnd(name, id, delta);
                } else {
                    onDragEnd(name, id, shiftMode.delta);
                }
            }
        }

        public void DrawOnSceneGUI(ref Matrix4x4 matrix, float sizeMultiplier, bool active ) {
            float handleSize = HandleUtility.GetHandleSize(matrix.GetColumn(3)) * sizeMultiplier ;
            Event e = Event.current;
            
            if (enableShiftMode && e.shift != (shiftMode != null)) {
                if (e.shift) {
                    Camera c = Camera.current;
                    if (c != null) {
                        shiftMode = new ShiftMode(matrix, c);
                    }
                } else {
                    shiftMode = null;
                }
            }

            freeMoveCenter.displayGraphics = displayFreemoveCenterGraphics;
            if (Tools.current == Tool.Move && active) {
                DrawPositionHandles(ref matrix, handleSize);
            } else if (Tools.current == Tool.Rotate && active) {
                DrawRotationHandles(ref matrix, handleSize);
            } else if (Tools.current == Tool.Scale && active) {
                DrawScaleHandles(ref matrix, handleSize);
            } else if (Tools.current == Tool.Rect && active) {
                Vector3 pos = matrix.GetColumn(3);
                pos = freeMoveCenter.DrawOnSceneGUI(pos);
                matrix.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
            } else {
                Vector3 worldOrigin = matrix.GetColumn(3);
                Vector2 center = HandleUtility.WorldToGUIPoint(worldOrigin);
                for (int i = 0; i < 3; i++) {
                    Vector3 axisEnd = worldOrigin + (Vector3)matrix.GetColumn(i) * handleSize * 0.5f;
                    Vector2 guiAxisEnd = HandleUtility.WorldToGUIPoint(axisEnd);
                    axisLines.SetPosition(i, center, guiAxisEnd);
                }
                OnSceneGUIGraphics.DrawGUIspace(axisLines);
            }

            if (drawNameLabel || drawIdLabel) {
                freeMoveCenter.DrawLabel(matrix.GetColumn(3), drawIdLabel, drawNameLabel);
            }
            
        }

        public void DrawOnSceneGUI(ref Matrix4x4 matrix, float sizeMultiplier, bool active, bool drawIdLabel, bool drawNameLabel) {
            this.drawIdLabel = drawIdLabel;
            this.drawNameLabel = drawNameLabel;
            DrawOnSceneGUI(ref matrix, sizeMultiplier, active);
        }

        void DrawPositionHandles(ref Matrix4x4 tm, float size) {
 
            if (shiftMode != null) {
                using (new Handles.DrawingScope(Matrix4x4.identity)) {
                    Vector3 pos = shiftMode.onStartWorldShiftTM.GetColumn(3);
                    pos = freeMoveCenter.DrawOnSceneGUI(pos);
                    shiftMode.onStartWorldShiftTM.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
                    moveXAxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                    moveYAxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                    moveZAxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                }
                tm = shiftMode.Result;
            } else {
                Vector3 pos = tm.GetColumn(3);
                pos = freeMoveCenter.DrawOnSceneGUI(pos);
                tm.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
                moveXAxis.Draw(ref tm, size);
                moveYAxis.Draw(ref tm, size);
                moveZAxis.Draw(ref tm, size);
            }
        }

        void DrawRotationHandles(ref Matrix4x4 tm, float size) {
             if ( shiftMode != null) {
                using (new Handles.DrawingScope(Matrix4x4.identity)) {
                    rotationXaxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                    rotationYaxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                    rotationZaxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                }
                tm = shiftMode.Result;
            } else {
                rotationXaxis.Draw(ref tm, size);
                rotationYaxis.Draw(ref tm, size);
                rotationZaxis.Draw(ref tm, size);
            }
        }

        void DrawScaleHandles(ref Matrix4x4 tm, float size) {
            if (shiftMode != null) {
                using (new Handles.DrawingScope(Matrix4x4.identity)) {
                    Vector3 pos = shiftMode.onStartWorldShiftTM.GetColumn(3);
                    //pos = center.Draw(pos);
                    shiftMode.onStartWorldShiftTM.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
                    scaleXaxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                    scaleYaxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                    scaleZaxis.Draw(ref shiftMode.onStartWorldShiftTM, size);
                }
                tm = shiftMode.Result;
            } else {
                Vector3 pos = tm.GetColumn(3);
                tm.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
                scaleXaxis.Draw(ref tm, size);
                scaleYaxis.Draw(ref tm, size);
                scaleZaxis.Draw(ref tm, size);
            }
        }

        public bool EqualsControlID(int id) {
            return freeMoveCenter.controlId == id || moveXAxis.controlId == id || moveYAxis.controlId == id || moveZAxis.controlId == id
            || rotationXaxis.controlId == id || rotationYaxis.controlId == id || rotationZaxis.controlId == id
            || scaleXaxis.controlId == id || scaleYaxis.controlId == id || scaleZaxis.controlId == id;
        }
 
 
        public int freemoveCenterControlID { 
            get {
                return freeMoveCenter.controlId;
            }
        }

        public void forceMouseDownOnFreemoveCenter(Vector3 position) {
            freeMoveCenter.forceMouseDown(position);
        }

        public void Dispose() {
            freeMoveCenter.Dispose();
            moveXAxis.Dispose();
            moveYAxis.Dispose();
            moveZAxis.Dispose();
            rotationXaxis.Dispose();
            rotationYaxis.Dispose();
            rotationZaxis.Dispose();
            scaleXaxis.Dispose();
            scaleYaxis.Dispose();
            scaleZaxis.Dispose();
            axisLines.Dispose();
        }
    }


}
