using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_PolylineAttributes : MonoBehaviour {

        Polyline polyline;
        public Texture2D texture;

        public float widthMult = 20f;

        private void Update() {
            if (polyline == null) {
                polyline = new Polyline(4);
                polyline.transparent = true;
                polyline.feather = 2;
                polyline.widthMultiplier = 20;
                polyline.isClosed = false;
                polyline.SetPosition(0, new Vector3(0, 0f, 0));
                polyline.SetPosition(1, new Vector3(1, 1f, 0));
                polyline.SetPosition(2, new Vector3(2, 0f, 0));
                polyline.SetPosition(3, new Vector3(3, 1f, 0));
                polyline.SetColor(0, new Color32(93, 255, 0, 255));
                polyline.SetColor(1, new Color32(92, 140, 255, 255));
                polyline.SetColor(2, new Color32(0, 255, 223, 255));
                polyline.SetColor(3, new Color32(178, 0, 255, 255));
                polyline.SetTextureOffset(0, 0.03f);
                polyline.SetTextureOffset(1, 0.33f);
                polyline.SetTextureOffset(2, 0.66f);
                polyline.SetTextureOffset(3, 0.97f);
                polyline.SetWidth(0, 2f);
                polyline.SetWidth(3, 2f);
            }
            polyline.texture = texture;
            polyline.widthMultiplier = widthMult;
            polyline.Draw(transform.localToWorldMatrix);

        }
    }
}