using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class LinefyDemo_LabelsRendererGenerator : MonoBehaviour {
        public LinefyLabelsRenderer labelsRenderer;

        public int labelsCount;
        public bool generate;
        public Gradient backgroundColors;



        void Update() {
            if (generate) {
                string[] text = new string[] { "THE QUCK", "JUMPS OVER", "BROWN FOX", "LAZY DOG", "LOREM", "IPSUM", "DOLAR", "The quick", "Jumps Over", "Brown Fox", "Jumps over", "LaZy", "Dog", "LoRem", "Ipsum", "Dolar", "loReM", "ipsum" };
                labelsRenderer.labels = new Label[labelsCount];
                for (int i = 0; i < labelsRenderer.labels.Length; i++) {
                    string name = text[i % text.Length];
                    name += Random.value > 0.5 ? " " : "";
                    name +=  Random.Range(0, 128).ToString();
                    labelsRenderer.labels[i] = new Label(name, Random.insideUnitSphere * 20, Vector2Int.zero);
                }
                generate = false;
            }

            //labelsRenderer.backgroundColor = backgroundColors.Evaluate((Time.time*0.25f)%1f); 
        }
    }
}
