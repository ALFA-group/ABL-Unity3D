using UnityEngine;
using Linefy.Primitives;
using Linefy.Serialization;
namespace LinefyExamples {
[ExecuteInEditMode]
    public class QuickStart_Primitives : MonoBehaviour {
        public Vector3 conePosition = new Vector3(-1,0,0);
        public Vector3 coneRotation = new Vector3(45,10,60);
        public float coneScale = 1;
        public Cone cone;

        public Vector3 boxPosition = new Vector3(1, 0, 0);
        public Vector3 boxRotation = new Vector3(15,20,30);
        public Vector3 boxScale;
        public Box box;

        private void Update() {
            if (cone == null) {
                cone = new Cone(1, 1.5f, 32, 7);
            }
            cone.Draw(transform.localToWorldMatrix * Matrix4x4.TRS(conePosition, Quaternion.Euler(coneRotation), new Vector3(coneScale, coneScale, coneScale) ));
            if (box == null) {
                box = new Box(1, 1, 1, 4, 4, 4);
            }
            box.Draw(transform.localToWorldMatrix * Matrix4x4.TRS(boxPosition, Quaternion.Euler(boxRotation), new Vector3(coneScale, coneScale, coneScale)));
        }
    }
}
