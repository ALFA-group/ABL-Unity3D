using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;

namespace LinefyExamples {

    [ExecuteInEditMode]
    public class LinefyDemo_EditorHandles : MonoBehaviour {
        [Matrix4x4Inspector(true)]
        public Matrix4x4 matrix0;

        [Matrix4x4Inspector(true)]
        public Matrix4x4 matrix1;

        [Matrix4x4Inspector(true)]
        public Matrix4x4 matrix2;

        [Matrix4x4Inspector(true)]
        public Matrix4x4 matrix3;
 
        [Range(1,3)]
        public float handlesSize = 1;

        public bool drawMeshes = true;

        public Vector3 point0;
        public Vector3 point1;
        public Vector3 point2;
        public Vector3 point3;
        public Vector3 point4;
        public Vector3 point5;
        public Vector3 point6;

        Polyline contourPolyline;
 


        public Mesh someMesh;
        public Material someMaterial;



        private void Update() {


            Matrix4x4 ltw = transform.localToWorldMatrix;
            if (drawMeshes) {
                Graphics.DrawMesh(someMesh, ltw * matrix0, someMaterial, 0);
                Graphics.DrawMesh(someMesh, ltw * matrix1, someMaterial, 0);
                Graphics.DrawMesh(someMesh, ltw * matrix2, someMaterial, 0);
                Graphics.DrawMesh(someMesh, ltw * matrix3, someMaterial, 0);
            }

            if (contourPolyline == null) {
                contourPolyline = new Polyline(7, true, 1, true);
                contourPolyline.widthMultiplier = 4;
            }
 

            contourPolyline.SetPosition(0, point0);
            contourPolyline.SetPosition(1, point1);
            contourPolyline.SetPosition(2, point2);
            contourPolyline.SetPosition(3, point3);
            contourPolyline.SetPosition(4, point4);
            contourPolyline.SetPosition(5, point5);
            contourPolyline.SetPosition(6, point6);
            contourPolyline.Draw(ltw);
        }

 
    }
}

