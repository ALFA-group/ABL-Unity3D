using UnityEngine;
using Linefy.Primitives;

namespace UnityExamples {
    public class DrawRuntime : MonoBehaviour {
        Box box;

        private void Start() {
            box = new Box();
        }

        private void Update() {
            box.Draw();
        }
    }
}
