using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    public class EditableLineTextureOffsetAnimator : MonoBehaviour {
        public EditableLines ep;
        public float speed = 1;

        public void Update() {
            ep.properties.textureOffset = Time.timeSinceLevelLoad * speed;
            ep.ApplyProperties();
        }
    }
}
