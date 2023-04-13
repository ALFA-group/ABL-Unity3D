using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_Dots : MonoBehaviour {

        Dots dots;

        private void Update() {
            if (dots == null) {
                dots = new Dots(4);
                dots.widthMultiplier = 50;
                dots.transparent = true;
                dots[0] = new Dot(new Vector3(0, 0.5f, 0), 1, 3, Color.red);
                dots[1] = new Dot(new Vector3(0.5f, 1, 0), 1, 19, Color.green);
                dots[2] = new Dot(new Vector3(1, 0.5f, 0), 1, 35, Color.blue);
                dots[3] = new Dot(new Vector3(0.5f, 0, 0), 1, 51, Color.white);
            }
            dots.Draw(transform.localToWorldMatrix);
        }
    }
}