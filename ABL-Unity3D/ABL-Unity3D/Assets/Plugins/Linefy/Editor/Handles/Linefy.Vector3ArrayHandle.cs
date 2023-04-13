using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;
using Linefy.Editors.Internal;

namespace Linefy{
    public class Vector3ArrayHandle {

        string name = "";
        enum InputType {
            Vector3Array,
            IVector3GetSetArray,
            SerializedProperty
        }

        LabelsRenderer d_labelRenderer;
        Matrix4x4Handle d_tmHandle;
        Dots d_pointsDots;
        RectangularSelection d_rectangularSelection;

        public class RectangularSelection {
            public bool shift;
            public bool control;

            Vector2 startPoint;
            Vector2 currentPoint;
            public Polyline pl;
            Texture2D dashedTexture;
            float minX;
            float maxX;
            float minY;
            float maxY;

            public RectangularSelection(Vector2 startPos, bool shiftKey, bool controlKey, Color selectedColor, Color unselectedColor) {
                this.startPoint = startPos;
                this.currentPoint = startPoint;
                this.shift = shiftKey;
                this.control = controlKey;
                pl = new Polyline(4, true, 0, true);
                pl.widthMultiplier = 1f;
                pl.colorMultiplier = controlKey ? unselectedColor : selectedColor;
                dashedTexture = new Texture2D(5, 1);
                dashedTexture.hideFlags = HideFlags.HideAndDontSave;
                dashedTexture.SetPixel(0, 0, Color.white);
                dashedTexture.SetPixel(1, 0, Color.white);
                dashedTexture.SetPixel(2, 0, Color.white);
                dashedTexture.SetPixel(3, 0, new Color(1, 1, 1, 0));
                dashedTexture.SetPixel(4, 0, new Color(1, 1, 1, 0));
                dashedTexture.Apply();
                pl.texture = dashedTexture;
            }

            public void SetCurrentMousePosition(Vector2 pos) {
                currentPoint = pos;
                pl.SetPosition(0, new Vector3((int)startPoint.x, (int)startPoint.y));
                pl.SetPosition(1, new Vector3((int)currentPoint.x, (int)startPoint.y));
                pl.SetPosition(2, new Vector3((int)currentPoint.x, (int)currentPoint.y));
                pl.SetPosition(3, new Vector3((int)startPoint.x, (int)currentPoint.y));
                pl.RecalculateDistances(0.2f);
                minX = Mathf.Min(startPoint.x, currentPoint.x);
                maxX = Mathf.Max(startPoint.x, currentPoint.x);
                minY = Mathf.Min(startPoint.y, currentPoint.y);
                maxY = Mathf.Max(startPoint.y, currentPoint.y);
            }

            public bool Contains(Vector2 guiPos) {
                return guiPos.x >= minX && guiPos.x <= maxX && guiPos.y >= minY && guiPos.y <= maxY;
            }

            public override string ToString() {
                return string.Format("x {0}-{1}  y:{2}-{3}", minX, maxX, minY, maxY);
            }

            public void Dispose() {
                pl.Dispose();
                UnityEngine.Object.DestroyImmediate(dashedTexture);
            }
        }

        struct Item {
            public int index;
            public Vector3 localPos;
            public Vector2 guiPos;
            public bool selected;
            public Vector3 localInTm;

            public Item(int idx, Vector3ArrayHandle parent) {
                localPos = Vector3.zero;
                guiPos = Vector3.zero;
                index = idx;
                selected = false;
                localInTm = Vector3.zero;
            }

            public void AssignPositionOnDrag() { 
            
            }

            public Vector3 GetWorldPosition() {
                return Handles.matrix.MultiplyPoint3x4( localPos );
            }
        }

        public Action<List<int>> onDragStart;
        public Action<List<int>> onDragUpdate;
        public Action<List<int>> onDragEnd;
        public Action<List<int>> onSelectionChanged;

        int hashCode;
        int pointsControlId;
        int hovered;
		Item[] points;
        Matrix4x4 worldTransformMatrix;

        bool dirtyTransformSpace;

        SerializedInEditorPrefs_vector2 floaterRectPos = new SerializedInEditorPrefs_vector2( "vector3arrayHandlesFloaterPosition", new Vector2(100,100), null);
        Rect floaterRect {
            get {
                Vector2 size = showPreferences ? new Vector2(295, 260) : new Vector2(180, 48);  
                return new Rect( floaterRectPos.vector2Value, size);
            }
        }  

        bool showPreferences;
        SerializedInEditorPrefs_color unselectedColor  ;
        SerializedInEditorPrefs_color selectedColor ;
        SerializedInEditorPrefs_int pointSize;
        SerializedInEditorPrefs_int pointOutline;
        SerializedInEditorPrefs_int pointShape;
        SerializedInEditorPrefs_int drawLabels;
        SerializedInEditorPrefs_int drawLabelsBackground;
        SerializedInEditorPrefs_int drawOnTop;
        SerializedInEditorPrefs_int transformSpace;
        SerializedInEditorPrefs_float labelsSize;
        SerializedInEditorPrefs_float matrixHandleSize;
        SerializedInEditorPrefs_float matrixHandleThicknesss;

        int floaterHash;
        int floaterControlId;
        int floaterDragAreaHash;
        int floaterDragAreaControlId;
        InputType inputType;
 
        GUIContent[] transformSpaceNames = new GUIContent[3] { new GUIContent( "Local", "Handles.matrix orientation"), new GUIContent("World", "World orientation"), new GUIContent("Screen", "Screen orientation") };
        GUIContent[] pointShapeNames = new GUIContent[8] { new GUIContent("Round" ), new GUIContent("Hexagon"), new GUIContent("Quad"), new GUIContent("Rhombus"), new GUIContent("RoundOutline"), new GUIContent("HexagonOutline"), new GUIContent("QuadOutline"), new GUIContent("RhombusOutline") };

        Rect dragAreaRect;
 
        Vector2 floaterDragOffset;
        bool floaterDrag;
  
        Vector3[] positions;
        IVector3GetSet[] ipositions;
        SerializedProperty[] prp_positions;
        Matrix4x4 cachedHandlesMatrix;
        Matrix4x4 cachedHandlesMatrixInverse;
        List<int> selectedIndices = new List<int>();
        GUIContent headerContent;
        float headerContentWidth;

        public Vector3ArrayHandle(int count, string name) {
            InternalCtor(count, name);
        }

        void InternalCtor(int count, string name) {
            if (string.IsNullOrEmpty(name)) {
                name = "Linefy.Vector3ArrayHandles";
            } else {
                this.name = name;
            }
            headerContent =  new GUIContent(name);

            unselectedColor = new SerializedInEditorPrefs_color("vector3arrayHandles_unselectedColor", Color.white, null);
            selectedColor = new SerializedInEditorPrefs_color("vector3arrayHandles_selectedColor", Color.red, null);
            pointSize = new SerializedInEditorPrefs_int("vector3arrayHandles_pointSize", 16, OnChangePoins);
            pointOutline = new SerializedInEditorPrefs_int("vector3arrayHandles_pointOutline", 2, OnChangePoins);
            pointShape = new SerializedInEditorPrefs_int("vector3arrayHandles_pointShape", 0, OnChangePoins);
            drawLabels = new SerializedInEditorPrefs_int("vector3arrayHandles_drawLabels", 1, null);
            drawLabelsBackground = new SerializedInEditorPrefs_int("vector3arrayHandles_drawLabelsBackground", 1, null);
            labelsSize = new SerializedInEditorPrefs_float("vector3arrayHandles_labelsSize", 1, OnChangeLabelsPixelPos);
            drawOnTop = new SerializedInEditorPrefs_int("vector3arrayHandles_drawOnTop", 1, OnDrawOnTopChanged);
            matrixHandleSize = new SerializedInEditorPrefs_float("vector3arrayHandles_matrixHandleSize", 1, null);
            matrixHandleThicknesss = new SerializedInEditorPrefs_float("vector3arrayHandles_matrixHandleThicknesss", 1, null);
            transformSpace = new SerializedInEditorPrefs_int("vector3arrayHandles_transformSpace", 1, OnTranformSpaceChanged);

            d_pointsDots = new Dots(count,  true);
  
            for (int d = 0; d < d_pointsDots.count; d++) {
                d_pointsDots[d] = new Dot(Vector3.zero, 1, 1, Color.white);
            }

            points = new Item[count];
            for (int i = 0; i< points.Length; i++) {
                points[i] = new Item(i, this); 
            }

            d_labelRenderer = new LabelsRenderer(count);
            d_labelRenderer.backgroundExtraSize = new Vector2(-8, -10);
            for (int i = 0; i< d_labelRenderer.count; i++) {
                d_labelRenderer.SetText(i, i.ToString());
            }
			d_labelRenderer.pixelPerfect = true;
            OnChangeLabelsPixelPos();
            OnDrawOnTopChanged();
            OnChangePoins();
            hashCode = GetHashCode();
            d_tmHandle = new Matrix4x4Handle("h", 0, onTMHandleDragBegin, onTMHandleDrag, onTMHandleDragEnd);
            floaterHash = hashCode - 1;
            Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(undoCallback));
           // Tools.hidden = true;
        }

        void undoCallback() {
            if (inputType == InputType.SerializedProperty) {
                if (prp_positions != null && prp_positions.Length > 0) {
                    if (prp_positions[0].serializedObject  != null) {
 
                        prp_positions[0].serializedObject.Update();
                    }
                    dirtyTransformSpace = true;
                    if (onDragUpdate != null) {
                        onDragUpdate(selectedIndices);
                    }
 
                }
            }
            OnVerticesSelectionChanged("undo");
        }

        bool hot {
            get {
                return GUIUtility.hotControl == pointsControlId;
            }

            set {
                if (value) {
                    GUIUtility.hotControl = pointsControlId;
                } else {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }
        }

        void OnVerticesSelectionChanged(string reason) {
 
            Vector3 center = Vector3.zero;
            selectedIndices.Clear();
 
            for (int i = 0; i<points.Length; i++) {
                if (points[i].selected) {
                    center += points[i].GetWorldPosition();
                    selectedIndices.Add(i);
                }
            }
            center /= (float)selectedIndices.Count;
 
            if (transformSpace.intValue == 0) { // Local
                worldTransformMatrix = cachedHandlesMatrix.ToUnscaled();
            } else if (transformSpace.intValue == 1) { //World
                worldTransformMatrix = Matrix4x4.identity;
            } else { //Screen
                if (Camera.current == null) {
                    //Debug.Log("camera == null");
                } else {
                    worldTransformMatrix =  Camera.current.transform.localToWorldMatrix;
                }
            }
           
            worldTransformMatrix.SetColumn(3, new Vector4(center.x, center.y, center.z, 1));
            Matrix4x4 itm = worldTransformMatrix.inverse ;

            for (int i = 0; i< selectedIndices.Count; i++) {
                int idx = selectedIndices[i];
                Vector3 worldPos = Handles.matrix.MultiplyPoint3x4(points[idx].localPos);
                points[idx].localInTm = itm.MultiplyPoint3x4(worldPos);
            }
            if (onSelectionChanged!=null) {
                onSelectionChanged(selectedIndices);
            }
            SceneView.RepaintAll();
        }

        float GetDistanceToPoint(ref int nearest) {
            nearest = -1;
            Vector2 _mousepos = Event.current.mousePosition;
            float minDist = float.MaxValue;
            for (int i = 0; i< points.Length; i++) {
                float dist = Mathf.Max(1, Vector2.Distance(points[i].guiPos, _mousepos) - pointSize.intValue/2);
                if (dist < minDist) {
                    minDist = dist;
                    nearest = i;
                }
            }
            return minDist;
        }

        void OverrideSelection(int selected) {
            for (int i = 0; i<points.Length; i++) {
                points[i].selected = false;
            }

            if (selected >= 0) {
                points[selected].selected = true;
            }
            OnVerticesSelectionChanged("Override");
        }

        void AddToSelection(int selected) {
            if (selected >= 0) {
                points[selected].selected = true;
            }
            OnVerticesSelectionChanged("add");
        }

        void RemoveFromSelection(int selected) {
            if (selected >= 0) {
                points[selected].selected = false;
            }
            OnVerticesSelectionChanged("remove");
        }

        void onTMHandleDragBegin(int id) {
            if (onDragStart != null) {
                onDragStart(selectedIndices);
            }
        }

        void onTMHandleDrag(int handleID) {
            for (int i = 0; i<points.Length; i++) {
                if (points[i].selected) {
                    Vector3 worldpos = worldTransformMatrix.MultiplyPoint3x4(points[i].localInTm);
                    Vector3 localPos = cachedHandlesMatrix.inverse.MultiplyPoint3x4(worldpos);
                    points[i].localPos = localPos;

                    if (inputType == InputType.Vector3Array) {
                        positions[i] = localPos;
                    } else if (inputType == InputType.IVector3GetSetArray) {
                        ipositions[i].vector3 = localPos;
                    } else if (inputType == InputType.SerializedProperty) {
                        prp_positions[i].vector3Value = points[i].localPos;
                        prp_positions[i].serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            if (onDragUpdate!=null) {
                onDragUpdate(selectedIndices);
            }
        }

        void onTMHandleDragEnd(string str, int id, Matrix4x4 tm) {
            if (onDragEnd != null) {
                onDragEnd(selectedIndices);
            }
        }

        #region DrawOnSceneGUI_Overloads

        public void DrawOnSceneGUI( SerializedProperty[] prp_positions ) {
            inputType = InputType.SerializedProperty;
            this.prp_positions = prp_positions;
            if (prp_positions.Length != points.Length) {
                InternalCtor(prp_positions.Length, name);
            }
            Event e = Event.current;
            if (e.type == EventType.Layout) {
                for (int i = 0; i < points.Length; i++) {
                    Vector3 localPos = prp_positions[i].vector3Value;
                    points[i].localPos = localPos;
                    points[i].guiPos = OnSceneGUIGraphics.WorldToGUIPoint(localPos);
                    d_labelRenderer.SetPosition(i, localPos);
                    d_pointsDots.SetPosition(i, localPos);
                }
            }
            core(e);
        }

        public void DrawOnSceneGUI(IVector3GetSet[] ipositions) {
            inputType = InputType.IVector3GetSetArray;
            this.ipositions = ipositions;
            if (ipositions.Length != points.Length) {
                InternalCtor(ipositions.Length, name);
            }
            Event e = Event.current;
            if (e.type == EventType.Layout) {
                for (int i = 0; i < points.Length; i++) {
                    Vector3 localPos = ipositions[i].vector3;
                    points[i].localPos = localPos;
                    points[i].guiPos = OnSceneGUIGraphics.WorldToGUIPoint(localPos);
                    d_labelRenderer.SetPosition(i, localPos);
                    d_pointsDots.SetPosition(i, localPos);
                }
            }
            core(e);
        }

        public void DrawOnSceneGUI(Vector3[] positions) {
            inputType = InputType.Vector3Array;
            this.positions = positions;
            if (positions.Length != points.Length) {
                InternalCtor(positions.Length, name);
            }
            Event e = Event.current;
            if (e.type == EventType.Layout) {
                for (int i = 0; i < points.Length; i++) {
                    Vector3 localPos = positions[i];
                    points[i].localPos = localPos;
                    points[i].guiPos = OnSceneGUIGraphics.WorldToGUIPoint(localPos);
                    d_labelRenderer.SetPosition(i, localPos);
                    d_pointsDots.SetPosition(i, localPos);
                }
            }
            core(e);
        }

        #endregion

        void core(Event e) {
            OnSceneGUIGraphics.drawGUIAfterLinefyObjects += DrawFloater;
 
           switch (e.type) {
               case EventType.Layout:
                   float floaterDistance = floaterRect.Distance(e.mousePosition);
                   HandleUtility.AddControl(floaterControlId, floaterDistance);
                   break;
               case EventType.MouseDown:
                   if (HandleUtility.nearestControl == floaterControlId) {
                       if (!e.alt && (dragAreaRect.Inflate(3)).Contains(e.mousePosition)) {
                           floaterDrag = true;
                           floaterDragOffset = floaterRect.position - e.mousePosition;
                       }
                   }
                   break;
               case EventType.MouseUp:
                   floaterDrag = false;
                   break;
               case EventType.MouseDrag:
                   if (floaterDrag) {
                       floaterRectPos.vector2Value = e.mousePosition + floaterDragOffset;
                   }
                   break;
                }
                Tools.hidden = false;
   
            Tools.hidden = true;

            if (e.alt && e.type == EventType.MouseDrag) {
                OnVerticesSelectionChanged("alt drag");
            }

            if (dirtyTransformSpace) {
                OnVerticesSelectionChanged("dirtyTransformSpace");
                dirtyTransformSpace = false;
            }

            d_tmHandle.displayFreemoveCenterGraphics = selectedIndices.Count > 1;
            d_tmHandle.styleThickness = matrixHandleThicknesss.floatValue;


            using (new Handles.DrawingScope(Matrix4x4.identity)) {
                d_tmHandle.DrawOnSceneGUI(ref worldTransformMatrix, matrixHandleSize.floatValue, true);
            }

            pointsControlId = EditorGUIUtility.GetControlID(hashCode, FocusType.Passive);
            floaterControlId = EditorGUIUtility.GetControlID(floaterHash, FocusType.Passive);
 
            switch (e.type) {
                case EventType.Layout:
                    if (d_rectangularSelection != null) {
                        d_rectangularSelection.SetCurrentMousePosition(e.mousePosition);
                    } else {
                        float floaterDistance = floaterRect.Distance(e.mousePosition);
                        HandleUtility.AddControl(floaterControlId, floaterDistance);
                        cachedHandlesMatrix = Handles.matrix;
                        cachedHandlesMatrixInverse = Handles.inverseMatrix;
                        if (!e.alt && floaterDistance >= 1) {
                            float distanceToPoint = GetDistanceToPoint(ref hovered);
                            if (Mathf.Approximately( distanceToPoint , 1 ))  {
                                HandleUtility.AddControl(pointsControlId, distanceToPoint);
                            }
                        }
                    }
 
                break;
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == floaterControlId) {
                        if ((dragAreaRect.Inflate(3)).Contains(e.mousePosition)) {
                            if (e.alt) {
                                e.type = EventType.Ignore;
                            }
                            floaterDrag = true;
                            floaterDragOffset = floaterRect.position - e.mousePosition;
 
                        }
                    } else {
                        if (!e.alt && e.button == 0) {
 
                            if (d_tmHandle.EqualsControlID(HandleUtility.nearestControl)) {
                    
                            } else {
                                if (HandleUtility.nearestControl == pointsControlId && e.button == 0) {
                                    hot = true;
                                    if (e.shift) {
                                        AddToSelection(hovered);
                                    } else if (e.control) {
                                        RemoveFromSelection(hovered);
                                    } else {
                                        OverrideSelection(hovered);
                                        GUIUtility.hotControl = d_tmHandle.freemoveCenterControlID;
                                        d_tmHandle.forceMouseDownOnFreemoveCenter(cachedHandlesMatrixInverse.MultiplyPoint3x4( worldTransformMatrix.GetColumn(3) ));
                                    }
                                } else {
                                    GUIUtility.hotControl = pointsControlId;
                                    e.Use();
                                    d_rectangularSelection = new RectangularSelection(e.mousePosition, e.shift, e.control, selectedColor.colorValue, unselectedColor.colorValue);
                                }
                            }
                        }
                    }
                    break;
 
   
                case EventType.MouseUp:
                    if (d_rectangularSelection != null) {
                        if (d_rectangularSelection.shift) {
                            for (int i = 0; i < points.Length; i++) {
                                if (d_rectangularSelection.Contains(points[i].guiPos)) {
                                    points[i].selected = true;
                                }
                            }
                        } else if (d_rectangularSelection.control) {
                            for (int i = 0; i < points.Length; i++) {
                                if (d_rectangularSelection.Contains(points[i].guiPos)) {
                                    points[i].selected = false;
                                }  
                            }
                        } else {
                            for (int i = 0; i < points.Length; i++) {
                                if (d_rectangularSelection.Contains(points[i].guiPos)) {
                                    points[i].selected = true;
                                } else {
                                    points[i].selected = false;
                                }
                            }
                        }
                        d_rectangularSelection.Dispose();
                        d_rectangularSelection = null;
                        OnVerticesSelectionChanged("rectangular selection end");
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    floaterDrag = false;
     
                    break;
                case EventType.MouseDrag:
                    if (floaterDrag) {
                        floaterRectPos.vector2Value = e.mousePosition + floaterDragOffset;
                    }
                    break;
                case EventType.Repaint:
                    d_labelRenderer.drawBackground = drawLabelsBackground.intValue == 1 ;
                    d_labelRenderer.size = labelsSize.floatValue;
                    OnSceneGUIGraphics.DrawWorldspace(d_pointsDots, Handles.matrix);

                    if (drawLabels.intValue == 1) {
                        OnSceneGUIGraphics.DrawWorldspace(d_labelRenderer, Handles.matrix);
                    }
                    break;
            }

            for (int i = 0; i < points.Length; i++) {
                d_pointsDots.SetColor(i, points[i].selected ? selectedColor.colorValue : unselectedColor.colorValue);
            }
            
            if (d_rectangularSelection != null) {
                OnSceneGUIGraphics.DrawGUIspace(d_rectangularSelection.pl);
            }

            if (e.shift) {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowPlus);
            } else if (e.control) {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowMinus);
            }
        }
 
        Color drawColorPicker(Rect position, GUIContent content, Color color) {
#if UNITY_2018_4_OR_NEWER
            return EditorGUI.ColorField(position, content, color, false, false, false);
#else
                   return EditorGUI.ColorField(position, content, color, false, false, false, null);
#endif
        }

        void DrawFloater() {
            GUI.Box(floaterRect, GUIContent.none, OnSceneGUIGraphics.sceneViewFloaterBackgrond);
            GUI.BeginGroup(floaterRect);
            headerContentWidth = EditorStyles.centeredGreyMiniLabel.CalcSize(headerContent).x;
            Rect _rect = new Rect(floaterRect.width/2 - headerContentWidth/2, 0, headerContentWidth, 20);
            GUI.Label(_rect, headerContent, EditorStyles.centeredGreyMiniLabel);
            _rect = new Rect(6, 22, 60, 26);
            transformSpace.intValue = EditorGUI.Popup(_rect.Offset(0,1), transformSpace.intValue, transformSpaceNames);
            _rect.x += _rect.width + 8;
			
	 
			_rect.width = 22;
			_rect.height = 22;
            drawLabels.intValue = OnSceneGUIGraphics.DrawToggle( _rect, OnSceneGUIGraphics.readme.drawLabelsToggle, drawLabels.intValue == 1)?1:0;
			_rect.x += 6;
			_rect.x += _rect.width;
            showPreferences = GUI.Toggle(_rect, showPreferences, new GUIContent("", "expand preferences"), EditorStyles.foldout);

            if (showPreferences) {
                _rect = new Rect(6, 58, 280, 16);
                selectedColor.colorValue = drawColorPicker(_rect, new GUIContent("Selected color", ""), selectedColor.colorValue);
                _rect.y += 19;
                unselectedColor.colorValue = drawColorPicker(_rect, new GUIContent("Unselected color", ""), unselectedColor.colorValue);
                _rect.y += 19;
                pointSize.intValue = EditorGUI.IntSlider(_rect, new GUIContent("Point Size", ""), pointSize.intValue,   1, 32);
                _rect.y += 19;
                pointOutline.intValue = EditorGUI.IntSlider(_rect, new GUIContent("Point Outline", ""), pointOutline.intValue, 0, 8);
                _rect.y += 19;
                pointShape.intValue = EditorGUI.Popup(_rect, new GUIContent("Point Shape", ""), pointShape.intValue, pointShapeNames);
                _rect.y += 19;
                drawLabelsBackground.intValue = EditorGUI.Toggle(_rect, new GUIContent("Draw labels background", ""), drawLabelsBackground.intValue == 1) ? 1 : 0;
                _rect.y += 19;
                labelsSize.floatValue = EditorGUI.Slider(_rect,  new GUIContent("Labels Size", ""), labelsSize.floatValue, 1, 4);
                _rect.y += 19;
                drawOnTop.intValue = EditorGUI.Toggle(_rect, new GUIContent("Draw on top", ""), drawOnTop.intValue == 1) ? 1 : 0;
                _rect.y += 19;
                matrixHandleSize.floatValue = EditorGUI.Slider(_rect, new GUIContent("Matrix Size", ""), matrixHandleSize.floatValue, 1, 4);
                _rect.y += 19;
                matrixHandleThicknesss.floatValue = EditorGUI.Slider(_rect, new GUIContent("Matrix Thickness", ""), matrixHandleThicknesss.floatValue, 1, 4);
            }
   
            GUI.EndGroup();
            dragAreaRect = floaterRect;
            dragAreaRect.height = 20;
        }

        void OnChangePoins() {
            if (d_pointsDots != null) {
                int rectIndex = DotsAtlas.GetDefaultDotAtlasRectIndex((DefaultDotAtlasShape)(pointShape.intValue ), pointOutline.intValue);
                for (int i = 0; i<d_pointsDots.count; i++) {
                    d_pointsDots.SetRectIndex(i, rectIndex);
                }
                d_pointsDots.widthMultiplier = pointSize.intValue;
                OnChangeLabelsPixelPos();
            }
        }

        void OnDrawOnTopChanged() {
            UnityEngine.Rendering.CompareFunction cf = (drawOnTop.intValue == 1) ? UnityEngine.Rendering.CompareFunction.Always : UnityEngine.Rendering.CompareFunction.LessEqual;
            if (d_labelRenderer != null) {
                d_labelRenderer.zTest = cf;
            }
            if (d_pointsDots != null) {
                d_pointsDots.zTest = cf;
            }
        }

        void OnChangeLabelsPixelPos() {
            for (int i = 0; i < d_labelRenderer.count; i++) {
                d_labelRenderer.SetOffset(i, new Vector2Int(0, 6 + (int)(pointSize.intValue * 0.4f) + (int)(6 * labelsSize.floatValue) ) );
            }
        }


        void OnTranformSpaceChanged() {
            dirtyTransformSpace = true;
        }

        public int count {
            get {
                return points.Length;
            }
        }

        public void Dispose() {
            Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(undoCallback));
            d_labelRenderer.Dispose();
            d_pointsDots.Dispose();
            if (d_rectangularSelection != null) {
                d_rectangularSelection.Dispose();
            }
            d_tmHandle.Dispose();
            Tools.hidden = false;
        }
    }
}
