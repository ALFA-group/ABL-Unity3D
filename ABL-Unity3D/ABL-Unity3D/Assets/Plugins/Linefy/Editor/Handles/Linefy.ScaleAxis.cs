using System;
using UnityEngine;
using UnityEditor;

namespace Linefy.Editors.Internal {

    public class ScaleAxis : Matrix4x4HandleElementBase {
        float centerPixelRadius;
        Matrix4x4 dragMatrix;
        Matrix4x4 dragMatrixInverse;
        float selectionDelta;
        bool isDragabble;

        Vector2 guiOrigin;
        Vector2 guiEnd;
        Vector2 guiDir;

        Lines l_axisLine;
        Dots l_dots;
        Dot dot = new Dot(Vector3.zero, 1, DotsAtlas.GetDefaultDotAtlasRectIndex(DefaultDotAtlasShape.Round, 2), Color.white);

        float onStartDragMagnitude;
        Vector3 onStartDragDir;
        float onStartDragZ;
        Matrix4x4 startDragPixelTM;

        bool duringDrag;

        public ScaleAxis(string name, int id, int axis, Color color, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd)
            : base(name, id, axis, color, onDragBegin, onDragUpdate, onDragEnd) {
            centerPixelRadius = 12;
            l_axisLine = new Lines(1,true);
            dot.enabled = true;
            l_dots = new Dots(1, true);
            l_dots.renderOrder = 2;
            l_dots.widthMultiplier = 16;
        }

 
        protected override float GetDistance() {
            if (isDragabble) {
                Vector2 mousePosition = Event.current.mousePosition;
                return Vector2.Distance(guiEnd, mousePosition)  - l_dots.widthMultiplier * styleThickness * 0.4f;
            } else {
                return float.MaxValue;
            }
        }

        protected override void OnRepaint(Matrix4x4 tm, float handleSize) {
            if (UpdateGUIPositions(tm, handleSize)) {
                if (isHovered) {
                    l_axisLine.widthMultiplier = styleThickness * 2.5f;
                    l_axisLine.colorMultiplier = Color.white;
                    dot.color = Color.white;
                    dot.size = styleThickness;
                } else if (isHot) {
                    l_axisLine.widthMultiplier = styleThickness * 2.5f;
                    l_axisLine.colorMultiplier = new Color32(255, 171, 25, 255);
                    dot.color = new Color32(255, 171, 25, 255);
                    dot.size = styleThickness;
                } else {
                    l_axisLine.widthMultiplier = styleThickness * 2f;
                    l_axisLine.colorMultiplier = color;
                    dot.color = color;
                    dot.size = styleThickness;
                }
                l_dots[0] = dot;
                OnSceneGUIGraphics.DrawGUIspace(l_dots);
                OnSceneGUIGraphics.DrawGUIspace(l_axisLine);
                isDragabble = true;
            } else {
                isDragabble = false;
            }
        }

        protected override void OnMouseDown(Matrix4x4 tm, float handleSize) {
            Vector2 guiup = new Vector2(guiDir.y, -guiDir.x);
            dragMatrix = Matrix4x4.identity;
            dragMatrix.SetColumn(0, new Vector4(0, 0, 1, 0));
            dragMatrix.SetColumn(1, guiup.normalized);
            dragMatrix.SetColumn(2, guiDir.normalized);
            dragMatrix.SetColumn(3, new Vector4(guiOrigin.x, guiOrigin.y, 0, 1));
            dragMatrixInverse = dragMatrix.inverse;
            Vector2 mp = Event.current.mousePosition;
            Vector3 localMousePos = dragMatrixInverse.MultiplyPoint3x4( mp );
            onStartDragZ = localMousePos.z;
            Vector3 axisVector = tm.GetColumn(axis);
            onStartDragDir = axisVector.normalized;
            onStartDragMagnitude = axisVector.magnitude;
            duringDrag = true;
        }

        protected override void OnMouseUp() {
            duringDrag = false;
            duringDragScale = 1;
        }

        float duringDragScale = 1;
        protected override void OnDrag(ref Matrix4x4 tm) {
            Vector2 mp = Event.current.mousePosition;
            Vector3 localMousePos = dragMatrixInverse.MultiplyPoint3x4(mp);
            duringDragScale =  localMousePos.z/ onStartDragZ;
            Vector3 currentAxis = onStartDragDir * onStartDragMagnitude * duringDragScale;
            tm.SetColumn(axis, currentAxis);
        }

        bool UpdateGUIPositions(Matrix4x4 tm, float handleSize) {
            Vector3 axisCenter = tm.GetColumn(3);
            Vector3 axisEnd = axisCenter + (Vector3)tm.GetColumn(axis).normalized * handleSize * duringDragScale;
            if (duringDrag) {
                axisEnd = axisCenter + onStartDragDir * handleSize * duringDragScale;
            }
            guiOrigin = HandleUtility.WorldToGUIPoint(axisCenter);
            guiEnd = HandleUtility.WorldToGUIPoint(axisEnd);
            guiDir = guiEnd - guiOrigin;
            float length = guiDir.magnitude;
            if (length <= centerPixelRadius) {
                return false;
            }
            dot.position = guiEnd;
            l_axisLine.SetPosition(0, guiOrigin, this.guiEnd);
            return true;
        }

        public override void Dispose() {
            l_axisLine.Dispose();
            l_dots.Dispose();
        }
    }
}
