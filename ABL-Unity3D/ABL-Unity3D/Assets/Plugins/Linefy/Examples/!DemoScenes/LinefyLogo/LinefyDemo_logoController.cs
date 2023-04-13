using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;
using Linefy.Primitives;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class LinefyDemo_logoController : MonoBehaviour {
 


        public Texture2D linesTexture;
        
        [Header("Text")]
        public DotsAtlas font;
 
        public AnimationCurve anim;
        public float timer;
        public float animationDuration = 1;

        public bool duringAnim;
        public bool playAnim;
        float rotFrom;
        float rotTo;
        LinefyLogo logo;
        public float crossRotation;

        public float startDelay = 3;
        public float descriptionAnimationDuration = 5;
        public float dtimer;
        public LinefyLabelsRenderer descriptionLR;

        public float wavesOffsetSpeed = 30;
        public LinefySineWaves waves;
        public AnimationCurve waveHeightCurve;


        private void Update() {
            if (logo == null) {
                logo = new LinefyLogo( );

            }
            logo.linesTexture = linesTexture;
            logo.font = font;

            if (Application.isPlaying) {
                if (playAnim && duringAnim == false) {
                    duringAnim = true;
                    playAnim = false;
                    rotFrom = logo.crossRotation;
                    rotTo = rotFrom + 90 * 1;
                    timer = 0;
                }

                if (duringAnim) {
                    timer += Time.deltaTime * (1f/ animationDuration);
                    logo.crossRotation.SetValue( Mathf.LerpUnclamped(rotFrom, rotTo,  anim.Evaluate(timer)));
                    if (timer >= 1) {
                        duringAnim = false;
                    }
                }
                waves.waveOffset = Time.timeSinceLevelLoad * wavesOffsetSpeed;
                waves.waveHeight = waveHeightCurve.Evaluate(Time.timeSinceLevelLoad*0.3f);

            } else {
                logo.crossRotation.SetValue(crossRotation);
            }


            logo.Draw(transform.localToWorldMatrix);

     
        }
    }

 
}
