using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace Linefy {

    public class Vector3Handle {
 
        public struct Style {
            public DefaultDotAtlasShape dotShape;

            int _fillShapeID;
            int _outlineShapeID;

            float _width;
            public float width { 
                get {
                    return _width;
                }
            }

            Color _color;
            public Color color { 
                get {
                    return _color;
                }
            }

            Color _fillColor;
            public Color fillColor { 
                get {
                    return _fillColor;
                }
            }

            int _outlineWidth;
            public int outlineWidth {
                get {
                    return _outlineWidth;
                }
            }

            public Style(float scale, Color color, Color fillColor, DefaultDotAtlasShape dotShape, int outlineWidth ) {
                this.dotShape = dotShape;
                this._width = scale;
                this._color = color;
                this._fillColor = fillColor;
                this._fillShapeID = DotsAtlas.GetDefaultDotAtlasRectIndex(dotShape, 0);
                this._outlineShapeID = DotsAtlas.GetDefaultDotAtlasRectIndex(dotShape+4, outlineWidth);
                this._outlineWidth = outlineWidth;
            }

            public void ToDots(Dots dots) {
                dots[0] = new Dot(Vector3.zero, _width, _fillShapeID, _fillColor);
                dots[1] = new Dot(Vector3.zero, _width, _outlineShapeID, _color);
            }
        }
        /// <summary>
        /// The action is fired when the dragging an handle starts where int is handle id
        /// </summary>
        public Action<int> onDragStart;

        /// <summary>
        /// The action is fired when handle dragging where int is handle id.
        /// </summary>
        public Action<int> onDragUpdate;

        /// <summary>
        /// The action is fired when the dragging an handle end where int is handle id.
        /// </summary>
        public Action<string, int, Vector3> onDragEnd;

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

            set {
                _id = value;
            }
        }

        int hashCode;

        int _controlId;
        public int controlId {
            get {
                return _controlId;
            }

            protected set {
                _controlId = value;
            }
        }

        Vector3 startDragPos;
        Vector3 dragDelta;

        Vector3 selectionDelta;
        Plane worldDragPlane;
 
        public bool drawOnTop = true;
        Dots l_dots;
        float defaultSize = 8;
        Color defaultColor = Color.white;

        GUIStyle labelStyle;
        GUIContent labelContent;
        Rect labelRect;

        public bool displayGraphics = true; 

        bool _drawIDLabel;
        bool drawIdLabel { 
            get {
                return _drawIDLabel;
            }

            set {
                if (value != _drawIDLabel) {
                    _drawIDLabel = value;
                    SetLabelContentAndRect();
                }
            }
        }

        bool _drawNameLabel;
        bool drawNameLabel { 
            get {
                return _drawNameLabel;
            }

            set {
                if (value != _drawNameLabel) {
                    _drawNameLabel = value;
                    SetLabelContentAndRect();
                }
            }
        }

        Style _style;
        public Style style { 
            get {
                 return _style;
            }

            set {
                _style = value;
                Color hoveredFill = value.fillColor;
                Color hotFill = value.fillColor;
                hoveredFill.a = hoveredFill.a + 0.1f;
                hoveredStyle = new Style(value.width, value.color, hoveredFill, value.dotShape, value.outlineWidth+1);
                hotStyle = new Style(value.width + 1, value.color, hotFill, value.dotShape, value.outlineWidth + 1);
                SetLabelStyle();
                SetLabelContentAndRect();
            }
        }

        Style _hoveredStyle;
        Style hoveredStyle {
            get {
                return _hoveredStyle;
            }

            set {
                _hoveredStyle = value;
 
            }
        }

        Style _selectedStyle;
        Style hotStyle {
            get {
                return _selectedStyle;
            }

            set {
                _selectedStyle = value;
 
            }
        }

        public Vector3Handle( int id) {
            Style s = new Style(defaultSize, defaultColor, new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.25f), DefaultDotAtlasShape.Round, 3);
            InternalCtor(string.Format("id{0}", id), id, null, null, null, s);
        }

        public Vector3Handle(string name, int id ) {
            Style s = new Style(defaultSize, defaultColor, new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.25f), DefaultDotAtlasShape.Round, 3);
            InternalCtor(name, id, null, null, null, s);
        }

        public Vector3Handle(int id, Style style) {
            InternalCtor(string.Format("id{0}", id), id, null, null, null, style);
        }

        public Vector3Handle(string name, int id, Style style) {
            InternalCtor(name, id, null, null, null, style);
        }

        public Vector3Handle(string name, int id, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Vector3> onDragEnd ) {
            Style s = new Style(defaultSize, defaultColor, new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.25f), DefaultDotAtlasShape.Round, 3);
            InternalCtor(  name, id, onDragBegin, onDragUpdate, onDragEnd, s );
        }

        public Vector3Handle(string name, int id, Style style, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Vector3> onDragEnd) {
            InternalCtor(name, id, onDragBegin, onDragUpdate, onDragEnd, style);
        }

        void InternalCtor( string name, int id, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Vector3> onDragEnd, Style style ) {
            this._name = string.IsNullOrEmpty(name) ? string.Format("id:{0}", id) : name;
            this._id = id;
            this.onDragStart = onDragBegin;
            this.onDragUpdate = onDragUpdate;
            this.onDragEnd = onDragEnd;
            hashCode = GetHashCode();
            l_dots = new Dots(2);
            l_dots.transparent = true;
            this.style = style;
            SetLabelStyle();
        }
 
        bool _hovered;
        bool hovered {
            get {
                return _hovered;
            }

            set {
                if (_hovered != value) {
                    _hovered = value;
                }
            }
        }

        bool hot {
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

        protected virtual float GetDistance(Vector3 position) {
            Vector2 _mousepos = Event.current.mousePosition;
            Vector2 guiPos = OnSceneGUIGraphics.WorldToGUIPoint(position);
            return Vector2.Distance(guiPos, _mousepos) - style.width;
        }

        protected virtual void OnMouseDown(Vector3 position) {
            Ray editorCameraRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            worldDragPlane = new Plane(-editorCameraRay.direction, ToWorldSpace(position));
            float e = 0;
            worldDragPlane.Raycast(editorCameraRay, out e);
            selectionDelta = position - ToHandleSpace(editorCameraRay.GetPoint(e));
        }

        protected Vector3 ToWorldSpace(Vector3 handlePos) {
            return Handles.matrix.MultiplyPoint3x4(handlePos);
        }

        protected Vector3 ToHandleSpace(Vector3 worldPos) {
            return Handles.inverseMatrix.MultiplyPoint3x4(worldPos);
        }

        protected virtual Vector3 OnDrag() {
            Ray editorCameraRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float e = 0;
            worldDragPlane.Raycast(editorCameraRay, out e);
            return ToHandleSpace(editorCameraRay.GetPoint(e)) + selectionDelta;
        }

        protected virtual void OnRepaint(Vector3 position) {
            if (displayGraphics) {

                if (hovered) {
                    hoveredStyle.ToDots(l_dots);
                } else if (hot) {
                    hotStyle.ToDots(l_dots);
                } else {
                    style.ToDots(l_dots);
                }

                if (drawOnTop) {
                    Vector2 guiPos = OnSceneGUIGraphics.WorldToGUIPoint(position);
                    l_dots.SetPosition(0, guiPos);
                    l_dots.SetPosition(1, guiPos);
                    OnSceneGUIGraphics.DrawGUIspace(l_dots);
                } else {
                    l_dots.SetPosition(0, position);
                    l_dots.SetPosition(1, position);
                    OnSceneGUIGraphics.DrawWorldspace(l_dots, Handles.matrix);
                }
            }
        }

        public void DrawLabel(Vector3 position, bool drawIdLabel, bool drawNameLabel) {
            this.drawIdLabel = drawIdLabel;
            this.drawNameLabel = drawNameLabel;
            if (Event.current.type == EventType.Repaint) {
                if (drawNameLabel || drawIdLabel) {
                    Vector2 guiPos = OnSceneGUIGraphics.WorldToGUIPoint(position);
                    Rect r = labelRect;
                    r.position += guiPos;
                    OnSceneGUIGraphics.drawGUIBeforeLinefyObjects += () => {
                        GUI.DrawTexture(r, OnSceneGUIGraphics.transparentBlackTexture);
                        GUI.Label(r, labelContent, labelStyle);
                    };
                }
            }
        }

        public Vector3 DrawOnSceneGUI(Vector3 position,  bool drawIdLabel, bool drawNameLabel ) {
            DrawLabel(position, drawIdLabel, drawNameLabel);
            return DrawOnSceneGUI(position);
        }

        public void forceMouseDown(Vector3 position) {
            hot = true;
            startDragPos = position;
            OnMouseDown(position);
            if (onDragStart != null) {
                onDragStart(id);
            }
        }

        public Vector3 DrawOnSceneGUI(Vector3 position) {
         
            Event e = Event.current;
 
            controlId = EditorGUIUtility.GetControlID(hashCode, FocusType.Keyboard);
            switch (e.type) {
                case EventType.Layout:
                    if (!e.alt) {
                        HandleUtility.AddControl(controlId, GetDistance(position));
                        hovered = HandleUtility.nearestControl == controlId && GUIUtility.hotControl == 0;
                    }
                    break;
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlId && e.button == 0) {
                        hot = true;
                        startDragPos = position;
                        OnMouseDown(position);
                        if (onDragStart != null) {
                            onDragStart(id);
                        }
 
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (hot) {
                        hot = false;
                        dragDelta = position - startDragPos;
                        if (onDragEnd != null) {
                            if (dragDelta.magnitude > Mathf.Epsilon) {
                                onDragEnd(name, id, dragDelta);
                            }
                        }
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (hot) {
                        position = OnDrag();
                        if (onDragUpdate != null) {
                            onDragUpdate(id);
                        }
                        e.Use();
                    }
                    break;
                case EventType.Repaint:
                    OnRepaint(position);
                    break;
            }

            return position;
        }

        void SetLabelContentAndRect() {
            labelContent = new GUIContent();
            if (drawIdLabel) {
                labelContent.text = string.Format("#{0}", id);
            }
            if (drawIdLabel && drawNameLabel) {
                labelContent.text += " ";
            }
            if (drawNameLabel) {
                labelContent.text += name;
            }
            Vector2 s = labelStyle.CalcSize(labelContent) + new Vector2(6,2);
            Vector2 positionOffset = new Vector2(-s.x / 2, style.width/3);
            labelRect = new Rect(positionOffset, s);
        }

        void SetLabelStyle( ) {
            labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;
            labelStyle.normal.textColor = style.color;
            labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        public void Dispose() {
            l_dots.Dispose();
        }
    }
}
