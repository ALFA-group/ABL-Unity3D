using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_CreateLines : MonoBehaviour {
        Lines lines;

        void Update() {
            if (lines == null) {
                lines = new Lines(2);
                //assign lines  
                lines[0] = new Line(Vector3.zero, new Vector3(1, 1, 0), Color.red, Color.yellow, 20, 20);
                lines[1] = new Line(Vector3.up, Vector3.right, Color.green, Color.cyan, 20, 20);
            }
            //actually draw a lines
            lines.Draw(transform.localToWorldMatrix);
        }
    }
}
