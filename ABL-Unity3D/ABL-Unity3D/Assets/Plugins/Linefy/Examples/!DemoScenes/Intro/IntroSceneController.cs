using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;

[ExecuteInEditMode]
public class IntroSceneController : MonoBehaviour
{
    public float startOrthoSize = 3;
    public Vector3 startPos;
    
    [System.Serializable]
    public class WayPoint {
        public string name;
        [HideInInspector]
        public float orthoSize;
        public Vector3 position;
        public float duration;
 
        [HideInInspector]
        public RangeFloat transitionFromTo;

        [HideInInspector]
        public RangeFloat fromTo;
    }

    public WayPoint[] waypoints;
    public float transitionDuration;
    public Camera cam;

    public bool editMode;
    public bool setKey;
    public int setKeyTo;
    public AnimationCurve transitionCurve;

    public bool animate;
    [Range(0,1)]
    public float normalizedTime;

    public float totalTime;
 
    void Update()
    {

        if (editMode ) {
            if (setKey) {
                waypoints[setKeyTo].orthoSize = cam.orthographicSize;
                waypoints[setKeyTo].position = cam.transform.position;
                setKey = false;
            }
        } else  {

            totalTime = 0;
            for (int i = 0; i < waypoints.Length; i++) {
                totalTime += transitionDuration;
                totalTime += waypoints[i].duration;
            }


            float t = 0;
            for (int i = 0; i < waypoints.Length; i++) {
                float transitionEnd = t + transitionDuration;
                waypoints[i].transitionFromTo = new RangeFloat(t, transitionEnd);
                float tEnd = transitionEnd + waypoints[i].duration;
                waypoints[i].fromTo = new RangeFloat(transitionEnd, tEnd);
                t += transitionDuration + waypoints[i].duration;
            }


            float currentTime = normalizedTime * totalTime;
            if (Application.isPlaying) {
                currentTime = Time.timeSinceLevelLoad;
            }

            for (int i = 0; i < waypoints.Length; i++) {
                if (i == 0) {
                    float lv = 0;
                    if (waypoints[i].transitionFromTo.InRange(currentTime, ref lv)) {
                        lv = transitionCurve.Evaluate(lv);
                        cam.orthographicSize = Mathf.LerpUnclamped(startOrthoSize, waypoints[i].orthoSize, lv);
                        cam.transform.position = Vector3.LerpUnclamped(startPos, waypoints[i].position, lv );
                    }

                } else {
                    float lv = 0;
                    if (waypoints[i].transitionFromTo.InRange(currentTime, ref lv)) {
                        lv = transitionCurve.Evaluate(lv);
                        cam.orthographicSize = Mathf.LerpUnclamped(waypoints[i - 1].orthoSize, waypoints[i].orthoSize, lv);
                        cam.transform.position = Vector3.LerpUnclamped(waypoints[i-1].position, waypoints[i].position, lv);
                    }
                }
 
                if (waypoints[i].fromTo.InRange(currentTime )) {
                    cam.orthographicSize = waypoints[i].orthoSize;
                    cam.transform.position = waypoints[i].position;
                }
            }
        }


    }
}
