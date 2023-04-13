using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    public class LinefyDemo_PolylineTrajectoryBall : MonoBehaviour {

        public enum State {
            waitSpawn,
            animation,
            animationFade
        }

        public State state = State.waitSpawn;
        public Color color;
        LinefyDemo_PolylineTrajectory parent;
        float disableFadeTimer;
        public Polyline pathPolyline;
        Rigidbody rb;
        int updateEveryFrame = 3;
        int updateCounter;
        float fadeTime;
        bool forceIsAdded;
        int PathVertsCount;
        MaterialPropertyBlock mpb;
        MeshRenderer mr;
        float overallTransparency;
        float visibleDistance = 10;
        Dots dots;

        public void OnAfterCreate(int index, LinefyDemo_PolylineTrajectory parent) {
            mr = GetComponent<MeshRenderer>();
            mpb = new MaterialPropertyBlock();
            this.parent = parent;
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            gameObject.SetActive(false);
            name = string.Format("index {0}", index);
            pathPolyline = new Polyline(name, 0, true, 1, false, parent.trajectoryTexture, 4, 128);
            pathPolyline.widthMode = WidthMode.PercentOfScreenHeight;
        }


        public void Spawn() {
            gameObject.SetActive(true);
            disableFadeTimer = 0;
            forceIsAdded = false;
            rb.isKinematic = false;
            transform.rotation = Random.rotation;

            transform.position = parent.spawnPoint.position + Random.insideUnitSphere * parent.spawnPointRadius;

            color = Color.HSVToRGB(Random.value, 1, 1);
            mpb.SetColor("_Color", color);
            mr.SetPropertyBlock(mpb);
            state = State.animation;
            pathPolyline.AddWithDistance(new PolylineVertex(transform.position, color, parent.trajectoryWidth));
            updateCounter = 0;
            overallTransparency = 1;
        }

        public void Disable() {
            pathPolyline.count = 0;
            state = State.waitSpawn;
            rb.isKinematic = true;
            gameObject.SetActive(false);
        }

        public void FixedUpdate() {
            if (state != State.waitSpawn) {
                if (forceIsAdded == false) {
                    rb.AddForce(Random.insideUnitSphere.normalized * 100f);
                    forceIsAdded = true;
                }
            }
        }

        public void Update() {
            if (state == State.animation) {
                if (transform.position.y < parent.DisableAltitude) {
                    state = State.animationFade;
                    disableFadeTimer = parent.DisableFadeLength;
                }
            } else if (state == State.animationFade) {
                overallTransparency = disableFadeTimer / parent.DisableFadeLength;
                disableFadeTimer -= Time.deltaTime;
                if (disableFadeTimer < 0) {
                    Disable();
                }
            }

            if (state != State.waitSpawn) {
                if (updateCounter == updateEveryFrame) {
                    pathPolyline.AddWithDistance(new PolylineVertex(transform.position, color, parent.trajectoryWidth));
                    updateCounter = 0;
                    float totalPathLength = pathPolyline[pathPolyline.count - 1].textureOffset;
                    float vd = Mathf.Min(visibleDistance, totalPathLength);
                    for (int i = 0; i < pathPolyline.count; i++) {
                        float d = pathPolyline.GetDistance(i);
                        float op = (1f - (totalPathLength - d) / vd) * overallTransparency;
                        pathPolyline.SetAlpha(i, op);
                    }
                } else {
                    updateCounter++;
                }
                pathPolyline.Draw();
            }
        }



    }
}
