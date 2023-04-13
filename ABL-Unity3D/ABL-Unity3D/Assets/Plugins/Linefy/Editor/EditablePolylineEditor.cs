using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;

namespace Linefy.Editors {
 
    [CustomEditor( typeof( EditablePolyline) )]
    public class EditablePolylineEditor : EditableEditorBase {

        Polyline gizmoPolyline;
        Dots gizmoPoints;
		SerializedProperty prp_isClosed;
        int nearestEdge;
        float nearestEdgeLv;
        float nearestEdgeMouseDistance;
        int nearestPoint;
        float nearestPointMouseDistance;
        static float gizmoPointsRadius = 7;
        bool allowAddPoint;
        bool allowRemovePoint;
 
         private void OnEnable() {
			OnEnableBase();
			prp_isClosed = prp_serializedProperties.FindPropertyRelative("isClosed");
        }

        void OnVerticesHandlesMove(List<int> selected) {
            UpdateRuntimeObject();
        }

        private void OnSceneGUI() {
            if (prp_enableOnSceneGUIEdit.boolValue) {
 
                Handles.matrix = GetTransformMatrix();
                int editMode = 0;
                if (Tools.current == Tool.Rect) {
                    editMode = 1;
                }

                if (editMode == 0) {
                    Draw_EditPositions_OnSceneGUI();
                } else {
                    if (verticesHandles != null) {
                        verticesHandles.Dispose();
                        verticesHandles = null;
                    }
                }

                if (editMode == 1) {
                    Draw_EditTopology_OnSceneGUI();
                }
            } else {
                if (verticesHandles != null) {
                    verticesHandles.Dispose();
                    verticesHandles = null;
                }

                if (gizmoPolyline != null) {
                    gizmoPolyline.Dispose();
                    gizmoPolyline = null;
                }

                if (gizmoPoints != null) {
                    gizmoPoints.Dispose();
                    gizmoPoints = null;
                }
            }
        }

        public override void OnInspectorGUI() {
			OnInspectorGUIBase(OnSceneGUIGraphics.readme.onSceneGUIEditModeEditablePolyline);
        }

        void Update_prp_vertpositions() {
            if (prp_vertpositions == null || prp_vertpositions.Length != prp_itemsArray.arraySize  ) {
                prp_vertpositions = new SerializedProperty[prp_itemsArray.arraySize];
                for (int i = 0; i < prp_vertpositions.Length; i++) {
                    prp_vertpositions[i] = prp_itemsArray.GetArrayElementAtIndex(i).FindPropertyRelative("position");
                }
            }
        }

        void Draw_EditPositions_OnSceneGUI() {
            Update_prp_vertpositions();
            if (verticesHandles == null) {
                verticesHandles = new Vector3ArrayHandle(prp_vertpositions.Length, "EditablePolyline");
                verticesHandles.onDragUpdate = OnVerticesHandlesMove;
            }
 
            serializedObject.Update();
            verticesHandles.DrawOnSceneGUI(prp_vertpositions);
        }

        Matrix4x4 GetTransformMatrix() {
            return (target as EditablePolyline).worldMatrix;
        }

        void Draw_EditTopology_OnSceneGUI() {
  
            serializedObject.Update();
            Event e = Event.current;
            int controlId = EditorGUIUtility.GetControlID(hashCode, FocusType.Passive);
            switch (e.type) {
                case EventType.Layout:
                    Update_prp_vertpositions();
                    if (gizmoPolyline == null) {
                        gizmoPolyline = new Polyline(prp_itemsArray.arraySize);
                    }
                    if (gizmoPoints == null) {
                        gizmoPoints = new Dots(prp_itemsArray.arraySize);
                        gizmoPoints.transparent = true;
                    }

                    gizmoPolyline.count = prp_vertpositions.Length;
                    gizmoPolyline.isClosed = prp_isClosed.boolValue;
                    gizmoPoints.count = prp_vertpositions.Length;

                    for (int i = 0; i < prp_vertpositions.Length; i++) {
                        Vector3 sp = HandleUtility.WorldToGUIPoint(prp_vertpositions[i].vector3Value);
                        gizmoPolyline.SetPosition(i, sp);
                        gizmoPoints.SetPosition(i, sp);
                    }
                    nearestEdgeMouseDistance = gizmoPolyline.GetDistanceXY(e.mousePosition, ref nearestEdge, ref nearestEdgeLv);
                    nearestPoint = gizmoPoints.GetNearestXY(e.mousePosition, ref nearestPointMouseDistance);
                    nearestPointMouseDistance -= gizmoPointsRadius;
                    HandleUtility.AddControl(controlId, nearestPointMouseDistance);
                    HandleUtility.AddControl(controlId, nearestEdgeMouseDistance);
                    allowAddPoint = nearestEdgeMouseDistance < nearestPointMouseDistance;
                    allowRemovePoint = nearestPointMouseDistance <= 0;
                    break;
                case EventType.MouseDown:
                    if( !e.alt && e.button == 0){
                        if (HandleUtility.nearestControl == controlId) {
                            if (e.control) {
                                if (allowRemovePoint) {
                                    prp_itemsArray.DeleteArrayElementAtIndex(nearestPoint);
                                }
                            } else {
                                if (allowAddPoint) {
                                    AddPoint(nearestEdge, nearestEdgeLv);
                                }
                            }
                            e.Use();
                            serializedObject.ApplyModifiedProperties();
                            UpdateRuntimeObject();
                        }
                    }
                    break;
                case EventType.Repaint:
                    OnSceneGUIGraphics.DrawGUIspace(gizmoPolyline);
                    OnSceneGUIGraphics.DrawGUIspace(gizmoPoints);
                    break;
            }

            if (HandleUtility.nearestControl == controlId) {
                if (e.control) {
                    if (allowRemovePoint) {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowMinus);
                    }
                } else {
                    if (allowAddPoint) {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ArrowPlus);
                    }
                }

            }
        }

        void AddPoint(int segmentIdx, float lv) {
            PolylineVertex a = this[segmentIdx];
            PolylineVertex b = this[segmentIdx+1];
            PolylineVertex newpv = a.Interpolate(b, lv);
            prp_itemsArray.InsertArrayElementAtIndex(segmentIdx);
            this[segmentIdx + 1] = newpv;
        }

        PolylineVertex this[int idx] {
            get {
                PolylineVertex pv;
                SerializedProperty prp_pv = prp_itemsArray.GetArrayElementAtIndex(Linefy.Internal.MathUtility.RepeatIdx(idx, prp_itemsArray.arraySize));
                pv.position = prp_pv.FindPropertyRelative("position").vector3Value;
                pv.color = prp_pv.FindPropertyRelative("color").colorValue;
                pv.width = prp_pv.FindPropertyRelative("width").floatValue;
                pv.textureOffset = prp_pv.FindPropertyRelative("textureOffset").floatValue;
                return pv;
            }

            set {
                prp_itemsArray.GetArrayElementAtIndex(idx).FindPropertyRelative("position").vector3Value = value.position;
                prp_itemsArray.GetArrayElementAtIndex(idx).FindPropertyRelative("color").colorValue = value.color;
                prp_itemsArray.GetArrayElementAtIndex(idx).FindPropertyRelative("width").floatValue = value.width;
                prp_itemsArray.GetArrayElementAtIndex(idx).FindPropertyRelative("textureOffset").floatValue = value.textureOffset;
            }
        }

        private void OnDisable() {
			OnDisableBase();
        }

        [MenuItem("GameObject/3D Object/Linefy/EditablePolyline", false, 1)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = EditablePolyline.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }

}
