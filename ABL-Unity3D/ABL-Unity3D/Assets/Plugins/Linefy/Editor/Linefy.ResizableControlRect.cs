using System;
using UnityEngine;
using UnityEditor;


namespace Linefy.Internal{

    public class ResizableControlRect   {
 
        int minHeight;
        int maxHeight;
        public int height;
        int controlID;
 
        Action<string> onChangesEnd;
        int dragOffset;

        Rect _guiRect;
        public Rect guiRect { 
            get {
                return _guiRect;
            }
        }
 
        public ResizableControlRect(  bool darkUI, int minHeight, int maxHeight, int height , Action<string> onChangesEnd) {
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.height = height;
            this.onChangesEnd = onChangesEnd;
        }

        public void Draw() {
            Rect r = EditorGUILayout.GetControlRect(false, height);
            GUILayout.Space(4);
            Rect separatorRect = new Rect( r.x, r.yMax , r.width, 1) ;
            Rect separatorRectInflated = separatorRect.Inflate(4);
 
            //GUI.DrawTexture(separatorRect, tex);
            OnSceneGUIGraphics.GUILayoutSeparator(separatorRect);

            Event e = Event.current;

            controlID = EditorGUIUtility.GetControlID(GetHashCode(), FocusType.Keyboard);
            if (e.type == EventType.MouseDown && separatorRectInflated.Contains(e.mousePosition)) {
                dragOffset = (int)(separatorRect.yMax - e.mousePosition.y);

                EditorGUIUtility.hotControl = controlID;
                e.Use();
            } else if (e.type == EventType.MouseDrag && EditorGUIUtility.hotControl == controlID) {
                height = Mathf.Clamp((int)(e.mousePosition.y - r.yMin) + dragOffset, minHeight, maxHeight);
                e.Use();
            } else if (e.type == EventType.MouseUp && EditorGUIUtility.hotControl == controlID) {
                EditorGUIUtility.hotControl = 0;
                if (onChangesEnd != null) {
                    onChangesEnd(string.Format("{0} = {1}", "xx", height));
                }
                e.Use();
            } else if (e.type == EventType.Repaint) {
                _guiRect = r;
                _guiRect.height -= 8;
            }

            EditorGUIUtility.AddCursorRect(separatorRectInflated, MouseCursor.ResizeVertical);
            if (EditorGUIUtility.hotControl == controlID) {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.ResizeVertical);
            }
        }
    }
}
