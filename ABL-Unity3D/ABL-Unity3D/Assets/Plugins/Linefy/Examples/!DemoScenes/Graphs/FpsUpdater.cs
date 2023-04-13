using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;

namespace LinefyExamples {
    public class FpsUpdater : MonoBehaviour {
        public LinefyDemo_PolylineGraph g;

        private void Start() {
            StartCoroutine(UpdateRange());
        }


        // Update is called once per frame
        void Update() {
            
            g.AddValueRight(1f/Time.unscaledDeltaTime);
        }

        public IEnumerator UpdateRange() {
            yield return new WaitForSeconds(2f);
            while (Application.isPlaying) {
                yield return new WaitForSeconds(0.5f);
                float minFps = float.MaxValue;
                float maxFps = float.MinValue;
                float averageFps = 0;
                g.GetValuesInfo(ref averageFps, ref minFps, ref maxFps);

 
                int maxRange = Mathf.CeilToInt(maxFps / 30f) * 30;
 
                int prevMaxRange = (int)g.yAxis.valuesRangeTo;

 
                int maxDelta = Mathf.Abs( maxRange/30 - prevMaxRange/30 );


                if (  maxDelta >2) {
                    float transitionTimer = 0;
                    float transitionLength = 3;
                    while (transitionTimer <= transitionLength) {
                        g.yAxis.valuesRangeTo = Mathf.Lerp(prevMaxRange, maxRange, MathUtility.LinearToSin( transitionTimer/transitionLength) );
                        transitionTimer += Time.deltaTime;
                        yield return 0;
                    }
 
                    g.yAxis.valuesRangeTo = maxRange;
                }

       

            }

        }
    }
}
