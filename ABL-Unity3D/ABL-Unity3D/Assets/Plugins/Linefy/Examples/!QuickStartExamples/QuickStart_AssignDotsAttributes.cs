using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_AssignDotsAttributes : MonoBehaviour {

        Dots dots;

        private void Update() {
            if (dots == null) {
                dots = new Dots(4);
                dots.widthMultiplier = 20;
                dots.transparent = true;

                dots.SetRectIndex(0, 3);
                dots.SetRectIndex(1, 3);
                dots.SetRectIndex(2, 3);
                dots.SetRectIndex(3, 3);

                dots.SetPosition(0, new Vector3(0, 0, 0));
                dots.SetPosition(1, new Vector3(1, 0, 0));
                dots.SetPosition(2, new Vector3(2, 0, 0));
                dots.SetPosition(3, new Vector3(3, 0, 0));

                dots.SetWidth(0, 1);
                dots.SetWidth(1, 1.5f);
                dots.SetWidth(2, 2);
                dots.SetWidth(3, 2.5f);

                dots.SetColor(0, Color.white);
                dots.SetColor(1, Color.Lerp(Color.white, Color.red, 0.33f));
                dots.SetColor(2, Color.Lerp(Color.white, Color.red, 0.66f));
                dots.SetColor(3, Color.red);
            }
            dots.Draw(transform.localToWorldMatrix);
        }
    }
}