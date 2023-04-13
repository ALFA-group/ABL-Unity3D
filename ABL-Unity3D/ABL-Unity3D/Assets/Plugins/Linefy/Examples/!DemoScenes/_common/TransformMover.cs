using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LinefyExamples {
    public class TransformMover : MonoBehaviour {

        public int axis;
        public float amplitude = 1;
        public float speed = 1;



        // Update is called once per frame
        void Update() {
            float a = Time.timeSinceLevelLoad * Mathf.PI;
            Vector3 p = new Vector3();
            p[axis] = Mathf.Sin(a) * amplitude;
            transform.position = p;
        }
    }
}
