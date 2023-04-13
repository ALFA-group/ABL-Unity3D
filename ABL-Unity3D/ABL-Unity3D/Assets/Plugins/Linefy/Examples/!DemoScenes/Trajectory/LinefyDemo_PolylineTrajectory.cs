using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

namespace LinefyExamples {
    public class LinefyDemo_PolylineTrajectory : MonoBehaviour {

 
        public float trajectoryWidth;
        public Texture2D trajectoryTexture;

        public Transform spawnPoint;
        public float spawnPointRadius = 1;
        public GameObject ballSource;
        float spawnTimer;
        public float SpawnFrequency = 0.5f;
        public LinefyDemo_PolylineTrajectoryBall[] objs;
        int indexCounter;
        public int poolLength;
        public float DisableFadeLength = 3;
        public float DisableAltitude = -3.8f;
 

        void Start() {
            objs = new LinefyDemo_PolylineTrajectoryBall[poolLength];
            for (int i = 0; i < objs.Length; i++) {
                LinefyDemo_PolylineTrajectoryBall spawned = (Instantiate(ballSource) as GameObject).GetComponent<LinefyDemo_PolylineTrajectoryBall>();
                spawned.OnAfterCreate(i, this);
                objs[i] = spawned;
            }
        }

        public LinefyDemo_PolylineTrajectoryBall getReadyObj() {
            return System.Array.Find(objs, f => f.state == LinefyDemo_PolylineTrajectoryBall.State.waitSpawn);
        }

        void Update() {
            if (spawnTimer > SpawnFrequency) {
                LinefyDemo_PolylineTrajectoryBall r = getReadyObj();
                if (r != null) {
                    r.Spawn();
                    UpdateInfo();
                }
                spawnTimer = 0;
            } else {
                spawnTimer += Time.deltaTime;
            }
        }

        public void UpdateInfo() {
            int activePolylines = 0;
            int totalVerticesCount = 0;
            for (int i = 0; i < objs.Length; i++) {
                if (objs[i].state != LinefyDemo_PolylineTrajectoryBall.State.waitSpawn) {
                    activePolylines++;
                    int c = objs[i].pathPolyline.count;
                    totalVerticesCount += c;
                }
            }
        }

    }
}
