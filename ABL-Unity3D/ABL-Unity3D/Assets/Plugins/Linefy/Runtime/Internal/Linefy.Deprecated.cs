using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Linefy {

    [Obsolete("PolylineSerializableData is Obsolete , use Linefy.Serialization.SerializationData_Polyline and Linefy.Serialization.SerializationDataFull_Polyline instead")]
    public class PolylineSerializableData {
        public string name;
        public Vector3[] vertices;
    }


    [Obsolete("TransparentPropertyBlock is Obsolete , use proper cserializable class from Linefy.Serialization")]
    public class TransparentPropertyBlock {
        public Color colorMuliplier;
        public TransparentPropertyBlock(float a, Color b, float c, float d) {
            colorMuliplier = b;
        }
    }

    public static class Utilites {
        [System.Obsolete("NearClipPlaneGUISpaceMatrix is Obsolete , use NearClipPlaneMatrix.GUISpace(camera, offset)")]
        public static Matrix4x4 NearClipPlaneGUISpaceMatrix(Camera camera, float offset) {
            return NearClipPlaneMatrix.GUISpace(camera, offset);
        }

        [System.Obsolete("LinearToSin() is Obsolete , use MathUtility.LinearToSin(camera, offset)")]
        public static float LinearToSin(float t) {
            return 1f - (Mathf.Sin((t * 3.141592f) + 1.5708f) * 0.49999f + 0.5f);
        }
    }

 

}
