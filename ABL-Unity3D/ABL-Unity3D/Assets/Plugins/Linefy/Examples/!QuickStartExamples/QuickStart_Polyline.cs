using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_Polyline : MonoBehaviour {
        Polyline polyline;

        private void Update() {
            if (polyline == null) {
                polyline = new Polyline(4);
                polyline.transparent = true;
                polyline.feather = 2;
                polyline.widthMultiplier = 20;
                polyline.isClosed = true;

                polyline[0] = new PolylineVertex(new Vector3(0, 0.5f, 0), Color.red, 1);
                polyline[1] = new PolylineVertex(new Vector3(0.5f, 1, 0), Color.yellow, 1);
                polyline[2] = new PolylineVertex(new Vector3(1, 0.5f, 0), Color.blue, 1);
                polyline[3] = new PolylineVertex(new Vector3(0.5f, 0, 0), Color.cyan, 1);
            }

            polyline.Draw(transform.localToWorldMatrix);
        }
    }
}