using System;
using UnityEngine;
using UnityEditor;

namespace Linefy.Editors.Internal {
 
    public class Matrix4x4HandleElementBase {
 
        string name;
        public string Name {
            get {
                return name;
            }
        }

        int id;
        public int Id {
            get {
                return id;
            }
        }

        protected int axis;
        protected int hashCode;

        int _controlId;
        public int controlId { 
            get {
                return _controlId;
            }

            protected set {
                _controlId = value;
            }
        }

        public float styleThickness;
 
 
        internal Matrix4x4 startDragMatrix;
        Matrix4x4 deltaMatrix;
        public Action<int> a_onDragBegin;
        public Action<int> a_onDragUpdate;
        public Action<string, int, Matrix4x4> a_onDragEnd;
        protected Color color;
        public bool displayOneUnitScaleScale;

        public Matrix4x4HandleElementBase(string name, int id, int axis, Color color, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd) {
            this.name = name;
            this.id = id;
            this.axis = axis;
            this.color = color;
            hashCode = GetHashCode();
            this.a_onDragBegin = onDragBegin;
            this.a_onDragUpdate = onDragUpdate;
            this.a_onDragEnd = onDragEnd;
        }

        bool pHover;
        protected bool isHovered {
            get {
                return pHover;
            }

            set {
                if (pHover != value) {
                    pHover = value;
                }
            }
        }

        protected bool isHot {
            get {
                return GUIUtility.hotControl == controlId;
            }

            set {
                if (value) {
                    GUIUtility.hotControl = controlId;
                } else {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }
        }

        protected virtual float GetDistance() {
            return float.MaxValue;
        }

        protected virtual void OnMouseDown(Matrix4x4 tm, float size) {

        }

        protected virtual void OnMouseUp( ) {

        }

        protected virtual void OnDrag(ref Matrix4x4 tm) {
            //return tm;
        }

        protected virtual void OnRepaint(Matrix4x4 tm, float size) {

        }

        public void Draw(ref Matrix4x4 tm, float handleSize) {
            Event e = Event.current;
            controlId = EditorGUIUtility.GetControlID(hashCode, FocusType.Keyboard);
            switch (e.type) {
                case EventType.Layout:
                    if (!e.alt) {
                        HandleUtility.AddControl(controlId, GetDistance());
                        isHovered = HandleUtility.nearestControl == controlId && GUIUtility.hotControl == 0;
                    }
                    break;
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlId && e.button == 0) {
                        isHot = true;
                        startDragMatrix = tm;
                        OnMouseDown(tm, handleSize);
                        if (a_onDragBegin != null) {
                            a_onDragBegin(id);
                        }
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (isHot) {
                        isHot = false;
                        deltaMatrix = startDragMatrix.inverse * tm;
                        if (!deltaMatrix.isIdentity && a_onDragEnd != null) {
                            a_onDragEnd(name, id, deltaMatrix);
                        }
                        OnMouseUp();
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (isHot) {
                        OnDrag(ref tm);
                        if (a_onDragUpdate != null) {
                            a_onDragUpdate(id);
                        }
                        e.Use();
                    }
                    break;
                case EventType.Repaint:
                    OnRepaint(tm, handleSize);
                    break;
            }
 
        }

        protected Matrix4x4 unscaledMatrix(Matrix4x4 tm) {
            Vector3 cx = tm.GetColumn(0).normalized;
            Vector3 cy = tm.GetColumn(1).normalized;
            Vector3 cz = tm.GetColumn(2).normalized;
            tm.SetColumn(0, cx);
            tm.SetColumn(1, cy);
            tm.SetColumn(2, cz);
            return tm;
        }

        public virtual void Dispose() {
            Debug.LogWarning("Dispose() not implemented");
        }

 

    }
}