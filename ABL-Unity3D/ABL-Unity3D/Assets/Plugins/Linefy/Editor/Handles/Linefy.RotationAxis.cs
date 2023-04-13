using System;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;

namespace Linefy.Editors.Internal {
    class RotationAxis : Matrix4x4HandleElementBase {
    
        Matrix4x4 startDragUnscaledTM;
        Matrix4x4 guidragBegin;
        Vector3[] sourceCirclePoints;
        Vector2[] guiSpaceCirclePoints;
        Vector3 guiSpaceOrigin;
        Vector3 guiSpaceAxisEnd;
        Vector3 startDragScale;
        Polyline l_circlePolyline;
        Lines l_axisLine;
        Edges2DArray guiEdges;

        public RotationAxis( string name, int id, int axis, Color color, int segments, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd) 
            : base(  name, id, axis, color, onDragBegin, onDragUpdate, onDragEnd) {
            segments = Mathf.Clamp(segments, 4, 256);
            sourceCirclePoints = new Vector3[segments];
            float step = 1f / sourceCirclePoints.Length;
            int verticalAxis = (axis + 1) % 3;
            int horizontalAxis = (axis + 2) % 3;
            for (int i = 0; i<sourceCirclePoints.Length; i++) {
                float a = Mathf.PI * 2 * i * step;
                sourceCirclePoints[i][horizontalAxis] = Mathf.Cos(a);
                sourceCirclePoints[i][verticalAxis] = Mathf.Sin(a);
            }
 
            guiSpaceCirclePoints = new Vector2[sourceCirclePoints.Length];
            l_circlePolyline = new Polyline( sourceCirclePoints.Length, true, 1, true );
            guiEdges = new Edges2DArray(segments);
 
            this.axis = axis;
            l_axisLine = new Lines( 1, true );
        }

        void UpdateGUIPositions(Matrix4x4 tm,  float handleSize) {
 
            Vector3 tmPos = tm.GetColumn(3);
            Vector3 axisEnd = tmPos + (Vector3)tm.GetColumn(axis).normalized * 0.5f * handleSize;
            guiSpaceOrigin = OnSceneGUIGraphics.WorldToGUIPoint(tmPos);
            guiSpaceAxisEnd = OnSceneGUIGraphics.WorldToGUIPoint(axisEnd);
            l_axisLine.SetPosition(0, (Vector2)guiSpaceOrigin, (Vector2)guiSpaceAxisEnd);
            Matrix4x4 unscaledTM = unscaledMatrix(tm);
            for (int i = 0; i < guiSpaceCirclePoints.Length; i++) {
                Vector3 worldPoint = unscaledTM.MultiplyPoint3x4( sourceCirclePoints[i] * handleSize );
                Vector3 guispacePoint = OnSceneGUIGraphics.WorldToGUIPoint(worldPoint);
                guiSpaceCirclePoints[i] = guispacePoint;
                l_circlePolyline.SetPosition( i, (Vector2)guispacePoint);
            }
            for (int i = 0; i<guiEdges.length-1; i++) {
                 guiEdges.SetEdge(i, guiSpaceCirclePoints[i], guiSpaceCirclePoints[i + 1]);
            }
            guiEdges.SetEdge(guiEdges.length - 1, guiSpaceCirclePoints[guiEdges.length - 1], guiSpaceCirclePoints[0]);
        }

        protected override float GetDistance() {
            Vector2 nearestA = Vector2.zero;
            Vector2 nearestB = Vector2.zero;
            return guiEdges.GetDistanceToPoint(Event.current.mousePosition) ;
        }

        protected override void OnMouseDown(Matrix4x4 tm, float handleSize) {
            startDragUnscaledTM = tm;
            Vector3 _right = tm.GetColumn(0);
            Vector3 _up = tm.GetColumn(1);
            Vector3 _fwd = tm.GetColumn(2);
            startDragScale = new Vector3(_right.magnitude, _up.magnitude, _fwd.magnitude);
            startDragUnscaledTM.SetColumn(0, _right.normalized);
            startDragUnscaledTM.SetColumn(1, _up.normalized);
            startDragUnscaledTM.SetColumn(2, _fwd.normalized);
            Vector2 nearestA = Vector2.zero;
            Vector2 nearestB = Vector2.zero;
            Vector2 mousepos = Event.current.mousePosition;
            guiEdges.GetDistanceToPoint(ref nearestA, ref nearestB, mousepos);
            Vector2 dirX =  ( nearestA - nearestB).normalized;
            Vector2 dirY = new Vector2(-dirX.y, dirX.x);
            guidragBegin = Matrix4x4.identity;
            guidragBegin.SetColumn(3, new Vector4(mousepos.x, mousepos.y, 0, 1));
            guidragBegin.SetColumn(0, dirX);
            guidragBegin.SetColumn(1, dirY);
            guidragBegin = guidragBegin.inverse;
        }
 
        protected override void OnDrag(ref Matrix4x4 tm) {
            Vector2 pixelMouse = Event.current.mousePosition;
            Vector3 localInDragTM = guidragBegin.MultiplyPoint(pixelMouse);
            float pixelDist = localInDragTM.x;
            Vector3 rotationEuler = Vector3.zero;
            rotationEuler[axis] = pixelDist;
            Matrix4x4 r =  startDragUnscaledTM * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rotationEuler), Vector3.one);
            r.SetColumn(0, r.GetColumn(0) * startDragScale.x);
            r.SetColumn(1, r.GetColumn(1) * startDragScale.y);
            r.SetColumn(2, r.GetColumn(2) * startDragScale.z);
            tm = r;
        }

        protected override void OnRepaint(Matrix4x4 tm, float handleSize) {
            UpdateGUIPositions(tm, handleSize);
            if (isHot) {
                l_circlePolyline.widthMultiplier = styleThickness * 2.5f;
                l_circlePolyline.colorMultiplier = new Color32(255, 171, 25, 255);
                l_axisLine.widthMultiplier = styleThickness * 2.5f;
                l_axisLine.colorMultiplier = new Color32(255, 171, 25, 255);
            } else if (isHovered) {
                l_circlePolyline.widthMultiplier = styleThickness * 2.5f;
                l_circlePolyline.colorMultiplier = Color.white;
                l_axisLine.widthMultiplier = styleThickness * 2.5f;
                l_axisLine.colorMultiplier = Color.white;
 
            } else {
                l_circlePolyline.widthMultiplier = styleThickness * 2f;
                l_circlePolyline.colorMultiplier = color;
                l_axisLine.widthMultiplier = styleThickness * 2f;
                l_axisLine.colorMultiplier = color;
            }
 
            OnSceneGUIGraphics.DrawGUIspace( l_axisLine );
            OnSceneGUIGraphics.DrawGUIspace( l_circlePolyline );
        }

        public override void Dispose() {
            l_axisLine.Dispose();
            l_circlePolyline.Dispose();
        }

    }
}
