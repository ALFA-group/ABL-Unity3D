using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinefyExamples {
 
    public class TransformRotator : MonoBehaviour {
        public enum Mode { 
            SingleAxis,
            Euler,
            RandomEuler
        }

        public Mode mode = Mode.SingleAxis;

        public float offset;
        public int axis;
        public float speed = 10;
        Quaternion initialRot;

        public bool mode2 ;
        public Vector3 mode2rot = new Vector3(100,100,100);
        Vector3 randomPerlin;

        private void Start() {
            initialRot = transform.rotation;
            randomPerlin = Random.insideUnitSphere * 100;
        }

        void Update() {
            if (mode == Mode.Euler) {
                transform.Rotate(mode2rot * Time.deltaTime);
            } else if (mode == Mode.SingleAxis) {
                Vector3 euler = new Vector3();
                euler[axis] = offset + Time.timeSinceLevelLoad * speed;
                transform.rotation = Quaternion.Euler(euler) * initialRot;
            } else {
                Vector3 euler =  new Vector3(Mathf.PerlinNoise(Time.timeSinceLevelLoad, randomPerlin.x) , Mathf.PerlinNoise(Time.timeSinceLevelLoad, randomPerlin.y), Mathf.PerlinNoise(Time.timeSinceLevelLoad, randomPerlin.z)).normalized  ;
                euler *= speed;
                transform.rotation = Quaternion.Euler(euler) * transform.rotation;
            }

        }
    }
}
