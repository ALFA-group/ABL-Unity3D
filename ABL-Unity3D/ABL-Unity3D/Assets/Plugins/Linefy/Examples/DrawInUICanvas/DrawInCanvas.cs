using UnityEngine;
using Linefy.Primitives;
using Linefy;

namespace LinefyExamples {
    public class DrawInCanvas : MonoBehaviour {

        public RectTransform parentRect;
        CircularPolyline circle;

        private void Start() {
            circle = new CircularPolyline(6, 1);
        }

        void Update() {
            circle.Draw(parentRect.GetCenteredWorldMatrix());
        }
    }
}
