using System;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;

namespace Linefy.Editors.Internal {

    public class MoveAxis : Matrix4x4HandleElementBase {
        
        float centerPixelRadius;
        Plane dragPlane;
        Matrix4x4 dragMatrix;
        Matrix4x4 dragMatrixInverse;
        float selectionDelta;
        bool isDragabble;

        Vector2 guiAxisBegin;
        Vector2 guiAxisEnd;
        Vector2 guiHeadBaseA;
        Vector2 guiHeadBaseB;
        Vector2 guiHeadApex;

        Edges2DArray edges = new Edges2DArray(4);

        Polyline l_arrowPolyline;

        bool ArrowPointTest(Vector2 mousePos) {
            return Triangle2D.PointTestDoublesided(guiHeadBaseA, guiHeadBaseB, guiHeadApex, mousePos);
        }

        public MoveAxis(string name, int id, int axis, Color color, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd)
            : base(name, id, axis, color, onDragBegin, onDragUpdate, onDragEnd) {
            centerPixelRadius = 12;
            l_arrowPolyline = new Polyline(6, true, 1, false);
            this.color = color;
        }

        protected override float GetDistance() {
            if (!isDragabble) {
                return float.MaxValue;
            }
            Vector2 mousePosition = Event.current.mousePosition;
            bool inArrow = Triangle2D.PointTestDoublesided(guiHeadBaseA, guiHeadBaseB, guiHeadApex, mousePosition);
            if (inArrow) {
                return 0;
            } else {
                return edges.GetDistanceToPoint(mousePosition);
            }
 
        }

        protected override void OnRepaint(Matrix4x4 tm, float handleSize) {
            
            if (UpdateGUIPositions(tm, handleSize)) {
                if (isHovered) {
                    l_arrowPolyline.widthMultiplier = styleThickness * 2.5f;
                    l_arrowPolyline.colorMultiplier = Color.white;
                 } else if (isHot) {
                    l_arrowPolyline.widthMultiplier = styleThickness * 2.5f;
                    l_arrowPolyline.colorMultiplier = new Color32(255,171, 25, 255);
                } else {
                    l_arrowPolyline.widthMultiplier = styleThickness * 2f;
                    l_arrowPolyline.colorMultiplier = color;
                }

                OnSceneGUIGraphics.DrawGUIspace(l_arrowPolyline);
                isDragabble = true;
            } else {
                isDragabble = false;
            }
        }

        protected override void OnMouseDown(Matrix4x4 tm, float handleSize) {
            Ray editorCameraRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Vector3 worldPos = Handles.matrix.MultiplyPoint3x4(tm.GetColumn(3));
            Vector3 worldForward = Handles.matrix.MultiplyVector(tm.GetColumn(axis));
            Vector3 worldUp = editorCameraRay.direction;
            dragMatrix = Matrix4x4.TRS(worldPos, Quaternion.LookRotation(worldForward, worldUp), Vector3.one);
            dragMatrixInverse = dragMatrix.inverse;
            dragPlane = new Plane(dragMatrix.GetColumn(1), worldPos);
            Vector3 intersection = Vector3.zero;
            float f = 0;
            dragPlane.Raycast(editorCameraRay, out f);
            intersection = editorCameraRay.GetPoint(f);
            Vector3 intersectionLocal = dragMatrixInverse.MultiplyPoint3x4(intersection);
            selectionDelta = intersectionLocal.z;
        }

        protected override void OnDrag(ref Matrix4x4 tm) {
            Ray editorCameraRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Vector3 intersection = Vector3.zero;
            float f = 0;
            dragPlane.Raycast(editorCameraRay, out f);
            intersection = editorCameraRay.GetPoint(f);
            Vector3 intersectionLocal = dragMatrixInverse.MultiplyPoint3x4(intersection);
            float z = intersectionLocal.z - selectionDelta;
            Vector3 nWorldPosition = dragMatrix.MultiplyPoint3x4(new Vector3(0, 0, z));
            Vector4 column3 = Handles.inverseMatrix.MultiplyPoint3x4(nWorldPosition);
            column3.w = 1;
            tm.SetColumn(3, column3);
         }

        bool UpdateGUIPositions( Matrix4x4 tm, float handleSize) {
            Vector3 tmPos = tm.GetColumn(3);
            Vector3 axisDir = tm.GetColumn(axis).normalized * handleSize;
            Vector3 axisEnd = tmPos + axisDir;
            Vector2 guiTmPos = OnSceneGUIGraphics.WorldToGUIPoint(tmPos);
            guiHeadApex = OnSceneGUIGraphics.WorldToGUIPoint(axisEnd );
            Vector2 guiDirToEnd =  guiHeadApex - guiTmPos;
            float length = guiDirToEnd.magnitude;
            if (length <= centerPixelRadius) {
                return false;
            }
            float alv = Mathf.InverseLerp(0, length, centerPixelRadius);
            float blv = Mathf.Clamp(0.75f, alv, 1f);
            guiAxisBegin = Vector3.LerpUnclamped(guiTmPos, guiHeadApex, alv);
            guiAxisEnd = Vector3.LerpUnclamped(guiTmPos, guiHeadApex, blv);
 
            Vector2 ortho = new Vector2(guiDirToEnd.y, -guiDirToEnd.x) / length *5f;
            guiHeadBaseA =  (Vector2)guiAxisEnd - ortho;
            guiHeadBaseB = (Vector2)guiAxisEnd + ortho;
            l_arrowPolyline.SetPosition(0, guiAxisBegin);
            l_arrowPolyline.SetPosition(1, guiAxisEnd);
            l_arrowPolyline.SetPosition(2, guiHeadBaseA);
            l_arrowPolyline.SetPosition(3, guiHeadApex);
            l_arrowPolyline.SetPosition(4, guiHeadBaseB);
            l_arrowPolyline.SetPosition(5, guiAxisEnd);

            l_arrowPolyline.SetWidth(6,   handleSize);

            edges.SetEdge(0, guiAxisBegin, guiAxisEnd);
            edges.SetEdge(1, guiHeadBaseA, guiHeadApex);
            edges.SetEdge(2, guiHeadBaseB, guiHeadApex);
            edges.SetEdge(3, guiHeadBaseB, guiHeadBaseA);
            return true;
        }

        public override void Dispose() {
            l_arrowPolyline.Dispose();
        }

    }
}
