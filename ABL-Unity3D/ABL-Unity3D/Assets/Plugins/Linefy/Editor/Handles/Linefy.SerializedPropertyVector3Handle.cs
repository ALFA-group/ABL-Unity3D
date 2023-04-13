using System.Collections;
using UnityEditor;
using UnityEngine;
using System;

namespace Linefy {
    public class SerializedPropertyVector3Handle : Vector3Handle {
        public SerializedProperty vector3property;

        public SerializedPropertyVector3Handle(SerializedProperty vector3property, int id ) : base(vector3property.name, id) {
            this.vector3property = vector3property;
        }

        public SerializedPropertyVector3Handle(SerializedProperty vector3property, int id, Style style) : base(vector3property.name, id, style) {
            this.vector3property = vector3property;
        }

        public SerializedPropertyVector3Handle(SerializedProperty vector3property,  int id,  Style style, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Vector3> onDragEnd ) : base(vector3property.name, id, style, onDragBegin, onDragUpdate, onDragEnd) {
            this.vector3property = vector3property;
        }

        public void DrawPropertyHandle() {
            vector3property.vector3Value = DrawOnSceneGUI(vector3property.vector3Value);
        }

        public void DrawPropertyHandle( bool drawIdLabel, bool drawNameLabel ) {
            vector3property.vector3Value = DrawOnSceneGUI(vector3property.vector3Value, drawIdLabel, drawNameLabel );
        }
    }
}
