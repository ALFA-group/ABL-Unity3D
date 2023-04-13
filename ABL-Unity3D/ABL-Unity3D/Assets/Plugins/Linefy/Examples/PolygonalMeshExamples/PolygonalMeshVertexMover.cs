using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinefyExamples {
    [ExecuteInEditMode]
    public class PolygonalMeshVertexMover : MonoBehaviour {
        public CreatePolygonalMesh cpm;
        public int positionIndex;
        public Vector3[] waypoints;

        public float cycleDuration = 5f;
        [Range(0,1f)]
        public float normalizedTime = 0;
        public bool animate;
 

        void Update() {
            float perWaypoint = cycleDuration / (float)(waypoints.Length);
            float cTime = cycleDuration * normalizedTime;
            if (Application.isPlaying) {
                cTime = Time.timeSinceLevelLoad % cycleDuration;
            }

            int wayPoint = Mathf.FloorToInt (cTime / perWaypoint);
            wayPoint = Mathf.Min(wayPoint, waypoints.Length - 1);
            float wplv = (cTime - (wayPoint * perWaypoint)) / perWaypoint;
            if (cpm.pm != null && animate) {
                Vector3 pos = Vector3.Lerp(waypoints[wayPoint], waypoints[(wayPoint+1)%waypoints.Length], wplv);
                cpm.positions[positionIndex] = pos;
                cpm.pm.SetPosition(positionIndex, pos); 
            }
 
        }
    }
}