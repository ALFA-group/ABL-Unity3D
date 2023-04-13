using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class QuickStart_LabelsRenderer : MonoBehaviour {
        LabelsRenderer labelsRenderer;
 
        void Update() {
            if (labelsRenderer == null) {
                labelsRenderer = new LabelsRenderer(3);
                labelsRenderer[0] = new Label("Label One", new Vector3(-2, 0, 0), new Vector2(0,0));
                labelsRenderer[1] = new Label("Label Two", new Vector3(0, 0, 0), new Vector2(0, 20));
                labelsRenderer[2] = new Label("Label Three", new Vector3(2, 0, 0), new Vector2(0, 40));
            }

            labelsRenderer.size = 1;
            labelsRenderer.drawBackground = true;
            labelsRenderer.Draw(transform.localToWorldMatrix);
        }
    }
}
