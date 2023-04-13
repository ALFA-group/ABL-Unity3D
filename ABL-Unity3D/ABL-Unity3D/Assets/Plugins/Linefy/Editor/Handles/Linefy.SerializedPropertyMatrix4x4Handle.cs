 
using UnityEditor;
using UnityEngine;
using System;

namespace Linefy{

    public class SerializedPropertyMatrix4x4Handle : Matrix4x4Handle {
        SerializedProperty[] arr;
        public SerializedProperty matrixProperty;

        public SerializedPropertyMatrix4x4Handle(SerializedProperty matrixProperty, int id, Action<int> onDragBegin, Action<int> onDragUpdate, Action<string, int, Matrix4x4> onDragEnd) :base(matrixProperty.name, id, onDragBegin, onDragUpdate, onDragEnd){
            this.matrixProperty = matrixProperty;
            SerializedProperty m00 = matrixProperty.FindPropertyRelative("e00");
            SerializedProperty m01 = matrixProperty.FindPropertyRelative("e01");
            SerializedProperty m02 = matrixProperty.FindPropertyRelative("e02");
            SerializedProperty m03 = matrixProperty.FindPropertyRelative("e03");

            SerializedProperty m10 = matrixProperty.FindPropertyRelative("e10");
            SerializedProperty m11 = matrixProperty.FindPropertyRelative("e11");
            SerializedProperty m12 = matrixProperty.FindPropertyRelative("e12");
            SerializedProperty m13 = matrixProperty.FindPropertyRelative("e13");

            SerializedProperty m20 = matrixProperty.FindPropertyRelative("e20");
            SerializedProperty m21 = matrixProperty.FindPropertyRelative("e21");
            SerializedProperty m22 = matrixProperty.FindPropertyRelative("e22");
            SerializedProperty m23 = matrixProperty.FindPropertyRelative("e23");

            SerializedProperty m30 = matrixProperty.FindPropertyRelative("e30");
            SerializedProperty m31 = matrixProperty.FindPropertyRelative("e31");
            SerializedProperty m32 = matrixProperty.FindPropertyRelative("e32");
            SerializedProperty m33 = matrixProperty.FindPropertyRelative("e33");
            
            arr = new SerializedProperty[] { m00, m10, m20, m30, m01, m11, m21, m31, m02, m12, m22, m32, m03, m13, m23, m33 };
        }
 
        public Matrix4x4 matrix4x4Value {
            get {
                Matrix4x4 r = new Matrix4x4();
                for (int i = 0; i < 16; i++) {
                    r[i] = arr[i].floatValue;
                }
                return r;
            }

            set {
                for (int i = 0; i < 16; i++) {
                    arr[i].floatValue = value[i];
                }
            }
        }

        public void DrawPropertyHandle(float size, bool active) {
            Matrix4x4 matrix = matrix4x4Value;
            base.DrawOnSceneGUI( ref matrix, size, active);
            matrix4x4Value = matrix;
        }

        public void DrawPropertyHandle(float size, bool active, bool drawIdLabel, bool drawNameLabel) {
            Matrix4x4 matrix = matrix4x4Value;
            base.DrawOnSceneGUI(ref matrix, size, active, drawIdLabel, drawNameLabel);
            matrix4x4Value = matrix;
        }
    }
}
