using UnityEngine;
 
namespace LinefyExamples {
    public class DrawInInspectorExample : MonoBehaviour {
        [Range(0.1f, 10)]
        public float zoom = 1;
        public Vector2 pan;
        public Color backgroundColor = Color.clear;
    }
}
