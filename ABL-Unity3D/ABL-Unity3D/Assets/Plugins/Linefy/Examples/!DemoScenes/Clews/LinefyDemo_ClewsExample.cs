using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class LinefyDemo_ClewsExample : MonoBehaviour {

        class VTransform {
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 rangle;
            public Matrix4x4 tm;

            public VTransform( ) {
                rotation = Random.insideUnitSphere * 180;
                rangle = Random.insideUnitSphere * 180;
            }

            public void Update(float dt) {
                rotation += rangle * dt * 0.5f;
                tm = Matrix4x4.TRS(position, Quaternion.Euler(rotation), Vector3.one);
            }
        }

        VTransform[] transforms;
        Matrix4x4[] matrices;

        string prevClewName;
        public ClewData clewDataSO;
        Polyline clewPolyline;
        public bool worldspace;
        public float pixelWidth = 20;
        public float worldspaceWidth = 1;
        [Range(16,256)]
        public int count;

        public bool enableInstancing;
        public bool drawInstanced;


        public float distributionSize = 30f;

        void SetTransformCount() {
            transforms = new VTransform[count];
            matrices = new Matrix4x4[count];
            for (int i = 0; i < transforms.Length; i++) {
                transforms[i] = new VTransform();
            }
 
            int sideCount = Mathf.CeilToInt( Mathf.Sqrt(count));
            float posStep = distributionSize / (float)(sideCount-1);
            //int transfprm
            int tcounter = 0;
            for (int y = 0; y < sideCount; y++) {
                for (int x = 0; x < sideCount; x++) {
                    if (tcounter < transforms.Length) {
                        float xPos = -distributionSize / 2 + x * posStep;
                        float zPos = -distributionSize / 2 + y * posStep;
                        transforms[tcounter].position = new Vector3(xPos, 0, zPos);
                        tcounter++;
                    }
                }
            }
 
        }

        void Update() {
            if (transforms == null || transforms.Length != count) {
                SetTransformCount();
 
            }

            if (clewDataSO != null) {
                if (clewPolyline == null || prevClewName != clewDataSO.clewData.name) {
                    clewPolyline = new Polyline(clewDataSO.vertices.Length);
                    clewPolyline.count = clewDataSO.vertices.Length;
                    for (int i = 0; i< clewPolyline.count; i++) {
                        clewPolyline[i] = clewDataSO.vertices[i];
                    }
                    prevClewName = clewDataSO.clewData.name;
                }
            }

            if (clewPolyline != null ) {
                clewPolyline.widthMode = worldspace ? WidthMode.WorldspaceBillboard : WidthMode.PixelsBillboard;
                clewPolyline.widthMultiplier = worldspace? worldspaceWidth : pixelWidth;

                float deltaTime = Application.isPlaying ? Time.deltaTime : 0;

                if (drawInstanced) {
                    for (int i = 0; i < count; i++) {
                        transforms[i].Update(deltaTime);
                        matrices[i] = transforms[i].tm;
                    }
                    clewPolyline.DrawInstanced(matrices);
                } else {
                    for (int i = 0; i < count; i++) {
                        transforms[i].Update(deltaTime);
                        clewPolyline.Draw(transforms[i].tm);

                    }
                }

                 
            }
 
        }

 
    }
}
