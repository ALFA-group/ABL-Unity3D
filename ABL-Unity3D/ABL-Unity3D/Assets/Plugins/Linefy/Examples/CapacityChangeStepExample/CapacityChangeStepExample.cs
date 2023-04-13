using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class CapacityChangeStepExample : MonoBehaviour {
        Polyline _pl;
        Polyline pl {
            get {
                if (_pl == null) {
                    _pl = new Polyline(itemsCount);
                }
                return _pl;
            }
        }

        Dots _dots;
        Dots dots {
            get {
                if (_dots == null) {
                    _dots = new Dots(itemsCount);
                }
                return _dots;
            }
        }

        Lines _lines;
        Lines lines {
            get {
                if (_lines == null) {
                    _lines = new Lines(itemsCount);
                }
                return _lines;
            }
        }

        [Range(0, 256)]
        public int itemsCount;
        public bool autoAnimateCount;
        public int capacityChangeStep;
        public bool polylineIsClosed;

        float segmentsCount;


        public AnimationCurve countCurve;

        float dMult = 1.4f;


        void Update() {
            if (Application.isPlaying) {
                itemsCount = (int)(countCurve.Evaluate(Time.timeSinceLevelLoad * 0.25f) * 256);
            }

            pl.count = itemsCount;
            pl.capacityChangeStep = capacityChangeStep;
            pl.widthMultiplier = 4;
            pl.feather = 1;
            pl.transparent = true;
            pl.isClosed = polylineIsClosed;

            dots.count = itemsCount;
            dots.capacityChangeStep = capacityChangeStep;
            dots.widthMultiplier = 1;
            dots.transparent = true;
            dots.renderOrder = 1;

            lines.count = itemsCount;
            lines.capacityChangeStep = capacityChangeStep;
            lines.widthMultiplier = 4;
            lines.feather = 1;
            lines.transparent = true;

            float a = 1;

            for (int i = 1; i < pl.count; i++) {
                float r = i * 0.015f;
                a += 1f / (Mathf.PI * r * dMult);
                Vector3 pos = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0);
                Vector3 pos2 = pos * 1.2f;
                lines.SetPosition(i, pos, pos2);
                pl.SetPosition(i, pos);
                dots.SetPosition(i, pos2);
            }

            pl.Draw();
            dots.Draw();
            lines.Draw();
        }
    }
}
