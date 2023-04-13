using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class LinefyDemo_DottedImage :MonoBehaviour {

        public Texture2D photo;
        public int dotsCount = 10000;
        public DotsAtlas atlas;
 
        Dots dots;
        public float widthMult = 1;
        public Color colorMult = Color.black;
        public DottedImageData data;

        public bool generate;

        int loadedGenerationHash = -1;

        void Start() {
            if (Application.isPlaying) {
                Update();
            }
        }

        void Update() {
            if (data != null) {

                if (dots == null) {
                    dots = new Dots(data.points.Length);
                    for (int i = 0; i< data.points.Length; i++) {
                        dots[i] = new Dot(new Vector3(data.points[i].x, data.points[i].y), data.points[i].z, 0, Color.white);
                    }
                }

                if (loadedGenerationHash != data.generationHash) {
                    dots.count = data.points.Length;
                    for (int i = 0; i < data.points.Length; i++) {
                        dots[i] = new Dot(new Vector3(data.points[i].x, data.points[i].y), data.points[i].z, 0, Color.white);
                    }
                    loadedGenerationHash = data.generationHash;
                }
                dots.transparent = true;
                dots.colorMultiplier = colorMult;
                dots.atlas = atlas;
                dots.widthMultiplier = widthMult;
                dots.Draw(transform.localToWorldMatrix);

                if (generate) {
                    data.generate(photo, dotsCount);
                    generate = false;
                }
            }
 
        }
    }
}
