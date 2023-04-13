using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using UnityEngine.SceneManagement;

namespace LinefyExamples {
    [ExecuteInEditMode]
    [DefaultExecutionOrder(71)]
    public class LinefyDemoScene : MonoBehaviour {
        public float duration = 5;
        public SceneManager scene;
        public string descriptionText = "empty";
        public bool descriptionTop;
        public DotsAtlas descriptionFont;
        public LabelsRenderer descriptionLR;
        public float timer;
        public NearClipPlaneMatrix ncpm;

        Lines fadeToBlackLine;
        [Range(0, 1)]
        public float transitionFadeValue = 1f;
        float transitionFadeDuration = .5f;
        float descriptionFadeInDelay = 0.5f;
        float descriptionFadeInDuration = 1;
 
        LabelsRenderer loadingLabels;
        float screenWidth;

        void Start() {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.autorotateToPortrait = false;
#if UNITY_2020_3_OR_NEWER
            Screen.brightness = 1;
#endif
            if (Application.isPlaying) {
                timer = 0;
                transitionFadeValue = 1;
                LateUpdate();
            }
        }

        void LateUpdate() {
            if (loadingLabels == null) {
                loadingLabels = new LabelsRenderer(1);
                loadingLabels.transparent = true;
                loadingLabels[0] = new Label("loading scene...", Vector3.zero, new Vector2Int(100, -50));
                loadingLabels.size = 2;
                loadingLabels.horizontalAlignment = TextAlignment.Left;
                loadingLabels.zTest = UnityEngine.Rendering.CompareFunction.Always;
                loadingLabels.renderOrder = 999;
            }

            if (fadeToBlackLine == null) {
                fadeToBlackLine = new Lines(1, true);
                fadeToBlackLine.zTest = UnityEngine.Rendering.CompareFunction.Always;
                fadeToBlackLine.renderOrder = 1000;
            }

            if (descriptionLR == null) {
                descriptionLR = new LabelsRenderer(1);
                descriptionLR.zTest = UnityEngine.Rendering.CompareFunction.Always;
                descriptionLR.renderOrder = 999;
                descriptionLR.atlas = descriptionFont;
                descriptionLR.transparent = true;
                descriptionLR.size = 0.75f;
            }

            float fadein = Mathf.Clamp01(1f - timer / transitionFadeDuration);
            float fadeOut = Mathf.Clamp01(1f - (duration - timer) / transitionFadeDuration);
            transitionFadeValue = Mathf.Max(fadein, fadeOut);
            Vector2 ps = ncpm.cameraPixelRect.size + new Vector2(4,4);
            if (transitionFadeValue > 0) {
                fadeToBlackLine[0] = new Line(
                    new Vector3(-2, ps.y / 2, 0),
                    new Vector3(ps.x, ps.y / 2, 0),
                    new Color(0, 0, 0, transitionFadeValue),
                    ps.y
                );

                fadeToBlackLine.Draw(ncpm.screen);
            }

            if (fadeOut > 0) {
 
            }

            float discriptionTransparency = Mathf.Clamp01((timer - descriptionFadeInDelay) / descriptionFadeInDuration);

            descriptionLR.textColor = new Color(1, 1, 1, discriptionTransparency);
            Vector3 pos = descriptionTop ? new Vector3(Mathf.Floor(ps.x / 2), ps.y-40) : new Vector3(Mathf.Floor(ps.x / 2), 40);
            descriptionLR[0] = new Label(descriptionText, pos, Vector2Int.zero);
            descriptionLR.atlas = descriptionFont;
            descriptionLR.Draw(ncpm.screen);

 

            if (Application.isPlaying) {
                timer = Time.timeSinceLevelLoad;
                if (timer > duration) {
                    LoadScene(1);
                }
            }
        }

        public void LoadScene(int sign) {
            Scene currentScene = SceneManager.GetActiveScene();
            int currentBuildIndex = currentScene.buildIndex;
            int count = SceneManager.sceneCountInBuildSettings;
            int idx = (currentBuildIndex + count + sign) % count;
            SceneManager.LoadScene(idx, LoadSceneMode.Single);
        }
    }
}
