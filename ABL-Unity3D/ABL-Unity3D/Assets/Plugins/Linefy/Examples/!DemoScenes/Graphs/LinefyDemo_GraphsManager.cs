using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LinefyExamples {

    [DefaultExecutionOrder(-1)]
    [ExecuteInEditMode]
    public class LinefyDemo_GraphsManager : MonoBehaviour {

        [System.Serializable]
        public class Item {
            public LinefyDemo_PolylineGraph graph;
            public float from;
            public float to;
            public float yOffset;
            public float speed;

            public void Update(float pt) {
                pt *= speed;
                float fromToRange = to - from;
                float fromToCenter = from + fromToRange / 2f;
                float val =  Mathf.PerlinNoise(pt, yOffset) * 2 - 1f;
                val = fromToCenter + val * fromToRange;
                graph.AddValueRightRealtime(val);
            }

 
        }

        public Rect rect = new Rect( new Vector2(0.5f, 0.5f), new Vector2(0.8f, 0.6f));
 
        public Item[] perlins;

        public float timer;
        public bool updateEditMode;


        public void LateUpdate() {
            if (Application.isPlaying == false && updateEditMode == false) {
                return;
            }

            for (int i = 0; i<perlins.Length; i++) {
                  perlins[i].Update(timer);
            }
            timer += Time.deltaTime;
 
        }
    }

}
