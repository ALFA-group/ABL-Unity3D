using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_TexturedLines : MonoBehaviour {

        Lines lines;
        public Texture2D texture;

        void Update() {
            if (lines == null) {
                lines = new Lines(2);
                lines.widthMultiplier = 40;
                lines.texture = texture;
                lines[0] = new Line(Vector3.zero, Vector3.up);
                lines[1] = new Line(Vector3.right, new Vector3(1, 1, 0));
                //set first line texture coordinates from zero to half of texture width
                lines.SetTextureOffset(0, 0, 0.5f);
                //set second line texture coordinates from half to full of texture width
                lines.SetTextureOffset(1, 0.5f, 1f);
            }
            lines.Draw(transform.localToWorldMatrix);
        }
    }
}
