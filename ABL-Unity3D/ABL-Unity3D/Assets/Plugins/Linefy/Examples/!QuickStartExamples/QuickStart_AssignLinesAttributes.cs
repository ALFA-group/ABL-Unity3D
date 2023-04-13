using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_AssignLinesAttributes : MonoBehaviour {
        Lines lines;

        void Update() {
            if (lines == null) {
                //create Lines instance with capacity 2
                lines = new Lines(2);

                //assign Lines properties
                lines.transparent = true;
                lines.feather = 3;
                lines.widthMultiplier = 20;

                //set lines position, color, width
                lines.SetPosition(0, Vector3.zero, Vector3.up);
                lines.SetPosition(1, Vector3.right, new Vector3(1, 1, 0));
                lines.SetColor(0, Color.yellow, Color.red);
                lines.SetColor(1, Color.black, Color.white);
                lines.SetWidth(0, 1, 2);
                lines.SetWidth(1, 2, 1);
            }
            lines.Draw(transform.localToWorldMatrix);
        }
    }
}
