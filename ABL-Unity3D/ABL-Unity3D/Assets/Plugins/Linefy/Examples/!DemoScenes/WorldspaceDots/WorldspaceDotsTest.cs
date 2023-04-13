using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class WorldspaceDotsTest : MonoBehaviour {
        Dots dots;
        public int count = 10;
        public float spacing = 1;
        public float width;
        public DotsAtlas atlas;
         


        void Update() {


            if (dots == null || dots.count != count) {
                dots = new Dots(count);
                dots.widthMode = WidthMode.WorldspaceBillboard;
                for (int i = 0; i < dots.count; i++) {
                    dots[i] = new Dot(Vector3.zero, 1, Random.Range(0, 9), Color.white);
                }
                int dimension = Mathf.CeilToInt(Mathf.Sqrt(count));
                int counter = 0;
                float posOffset = dimension * spacing * -0.5f;
 
                for (int x = 0; x < dimension && counter < count; x++) {
                    for (int y = 0; y < dimension && counter < count; y++) {
                        dots.SetPosition(counter, new Vector3(x * spacing + posOffset, y * spacing + posOffset, 0));
                        counter++;
                    }
                }
            }


            dots.atlas = atlas;
            dots.widthMultiplier = width;
            dots.Draw(transform.localToWorldMatrix);
        }
    }
}
