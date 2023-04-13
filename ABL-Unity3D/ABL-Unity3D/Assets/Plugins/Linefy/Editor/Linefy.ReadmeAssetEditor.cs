using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;
using Linefy.Editors.Internal;

namespace Linefy {
 
    [CustomEditor(typeof(LinefyReadMeAsset))]
    public class ReadmeAssetEditor : Editor {
        SerializedInEditorPrefs_string screenshotOutputName;

        GUIStyle _textStyle;
        GUIStyle textStyle { 
            get {
                if (_textStyle == null) {
                    _textStyle = new GUIStyle( EditorStyles.label);
                    _textStyle.wordWrap = true;
                    _textStyle.richText = true;
                }
                return _textStyle;
            }
        }

        GUIStyle _linkStyle;
        GUIStyle linkStyle {
            get {
                if (_linkStyle == null) {
                    _linkStyle = new GUIStyle(EditorStyles.label);
                    _linkStyle.fontStyle = FontStyle.Normal;
                    _linkStyle.fontSize = 14;
                    _linkStyle.active = _linkStyle.normal;
                    _linkStyle.onActive = _linkStyle.normal;
                    _linkStyle.onFocused = _linkStyle.normal;
                    _linkStyle.focused = _linkStyle.normal;
                }
                return _linkStyle;
            }
        }

        LinefyLogo logo;
        LabelsRenderer versionNumberLR;
        float inspectorWidth = 200;
        EditorGUIViewport logoViewport;
        bool start;
        bool animIsPlaying;
        float animTimer;
        float animDuration = 0.5f;
        float animRotFrom;
        float animRotTo;
        GUIContent c_screenshotx1 = new GUIContent("x1", "Capture Game view screenshot");
        GUIContent c_screenshotx2 = new GUIContent("x2", "Capture Game view screenshot with x2 increased resolution.");
        GUIContent c_screenshotx3 = new GUIContent("x3", "Capture Game view screenshot with x3 increased resolution.");
        GUIContent c_screenshotx4 = new GUIContent("x4", "Capture Game view screenshot with x4 increased resolution.");
 
        private void OnEnable() {
            screenshotOutputName = new SerializedInEditorPrefs_string("screenshotOutput", Application.dataPath, null);
        }

        public override void OnInspectorGUI() {
			LinefyReadMeAsset t = target as LinefyReadMeAsset;
			
            if (Event.current.type == EventType.KeyDown && Event.current.character == 'x') {
                t.initedVersion = "";
                EditorUtility.SetDirty(t);
                Debug.Log("reset inited version");
            }
			
            if (  Event.current.type == EventType.KeyDown && Event.current.character == 'r') {
                t.releaseView = !t.releaseView;
                EditorUtility.SetDirty(t);
                GUIUtility.ExitGUI();
            }
			
			if (!t.releaseView) {
				ReleaseView();
			} else  {
                DrawDefaultInspector();
            }
        }
  
        void DrawLogo() {
            LinefyReadMeAsset t = target as LinefyReadMeAsset;
            Rect textureRect = EditorGUILayout.GetControlRect(false, inspectorWidth * 0.3f);
            if (logoViewport == null) {
                logoViewport = new EditorGUIViewport();
            }

            logoViewport.backgroundColor = Color.clear;
 

            if (logo == null) {
                logo = new LinefyLogo();
            }
 
            if (versionNumberLR == null) {
                versionNumberLR = new LabelsRenderer(1);
                versionNumberLR.zTest = UnityEngine.Rendering.CompareFunction.Always;
                versionNumberLR.renderOrder = 1000;
                versionNumberLR.size = 1;
                versionNumberLR.pixelPerfect = true;
            }
            versionNumberLR.textColor = OnSceneGUIGraphics.darkSkin ? Color.white : Color.black;
            versionNumberLR[0] = new Label(t.versionName, new Vector3(0, -inspectorWidth*0.12f, 0), new Vector2Int(0, 0));

            logo.linesTexture = t.logoTexture;
            logo.font = t.logoFont;

            Event e = Event.current;

            if (e.type == EventType.Layout) {
                if (animIsPlaying) {
                    Repaint();
                }
            } else if (e.type == EventType.Repaint) {
                if (animIsPlaying) {
                    float nt = t.logoAnimCurve.Evaluate( animTimer / animDuration );
                    logo.crossRotation.SetValue(Mathf.LerpUnclamped(animRotFrom, animRotTo, nt)); 
                    animTimer += OnSceneGUIGraphics.onScenGUIRepaintDeltaTime;
                    if (animTimer > animDuration) {
                        logo.crossRotation.SetValue(animRotTo);
                        animIsPlaying = false;
                    }
                }
                float s = textureRect.width / t.logoSize;
                logoViewport.SetParams(textureRect, 1, Vector2.zero);
                logoViewport.DrawLocalSpace(logo, Matrix4x4.Scale(Vector3.one * s) * Matrix4x4.Translate(new Vector3(0, 1, 0)));
                logoViewport.DrawLocalSpace(versionNumberLR, Matrix4x4.identity);
                logoViewport.Render();
            } else if (e.type == EventType.MouseDown) {
                if (textureRect.Contains(Event.current.mousePosition)) {
                    if (animIsPlaying == false) {
                        animIsPlaying = true;
                        animRotFrom = logo.crossRotation;
                        animRotTo = animRotFrom - 90;
                        animTimer = 0;
                    }
                }
            }
        }

        void captureScreenshot(int size) {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");
            string fileName = string.Format("/{0}_{1}.png", sceneName, date);
            string outputPath = screenshotOutputName.stringValue + fileName;
            ScreenCapture.CaptureScreenshot(outputPath, size);
            Debug.LogFormat("screenshot saved: {0}", outputPath);
        }
 

        public void DrawScreenshotControls() {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Capture screenshot:", GUILayout.Width(120));
            if (GUILayout.Button(c_screenshotx1, EditorStyles.miniButton )) {
                captureScreenshot(1);
            }
            if (GUILayout.Button(c_screenshotx2, EditorStyles.miniButton )) {
                captureScreenshot(2);
            }
            if (GUILayout.Button(c_screenshotx3, EditorStyles.miniButton )) {
                captureScreenshot(3);
            }
            if (GUILayout.Button(c_screenshotx4, EditorStyles.miniButton )) {
                captureScreenshot(4);
            }

            GUIContent outputButtonContent = new GUIContent("...", string.Format( "Screenshot output path: {0}", screenshotOutputName.stringValue));
            if (GUILayout.Button(outputButtonContent, EditorStyles.miniButton, GUILayout.Width(24))) {

                if (string.IsNullOrEmpty(screenshotOutputName.stringValue)) {
                    screenshotOutputName.stringValue = EditorUtility.SaveFolderPanel("Output path", Application.dataPath, null);
                } else {
                    string npath = EditorUtility.SaveFolderPanel("Output path", screenshotOutputName.stringValue, null);
                    if (!string.IsNullOrEmpty(npath)) {
                        screenshotOutputName.stringValue = npath;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }


        void ReleaseView() {
            LinefyReadMeAsset t = target as LinefyReadMeAsset;
            if (start == false) {
                Repaint();
                start = true;
            }

            Rect emptySpace = EditorGUILayout.GetControlRect(false, 1);
            if (Event.current.type == EventType.Repaint) {
                inspectorWidth = emptySpace.width;
            }
 
			DrawLogo();
  
            OnSceneGUIGraphics.EditorGUILayoutSeparator();
 
            GUIContent c_darkSkin = new GUIContent("Dark Skin", "Sets the colors of the Linefy controls to match the dark(Pro) editor theme.");
            OnSceneGUIGraphics.darkSkin = EditorGUILayout.Toggle(c_darkSkin, OnSceneGUIGraphics.darkSkin);

            GUIContent c_showStatistic = new GUIContent("Scene Statistic", "Displays statistics in the Scene window for all Linefy items that are drawn using OnSceneGUI() ");
            OnSceneGUIGraphics.showStatistic = EditorGUILayout.Toggle(c_showStatistic, OnSceneGUIGraphics.showStatistic);

            if (t.enableScreenshotControls) {
                DrawScreenshotControls();
            }

            if (t.urlLink != null) {
                OnSceneGUIGraphics.EditorGUILayoutSeparator();
                EditorGUILayout.LabelField("Online documentation:");

                if (GUILayout.Button(new GUIContent(t.documentationURL, "open online documentation"), t.urlLink, GUILayout.MaxWidth(inspectorWidth) ) ) {
                    Debug.Log("pressed");
                    Application.OpenURL(t.documentationURL);
                }
  
                EditorGUILayout.LabelField("Rate/Review in Asset Store:");

                if (GUILayout.Button(new GUIContent(t.assetStoreURL, "open Asset Store page"), t.urlLink, GUILayout.MaxWidth(inspectorWidth))) {
                    Application.OpenURL(t.assetStoreURL);
                }
 
                EditorGUILayout.LabelField("Forum thread:");

                if (GUILayout.Button(new GUIContent(t.forumThreadURL, "open Forum thread"), t.urlLink, GUILayout.MaxWidth(inspectorWidth))) {
                    Application.OpenURL(t.forumThreadURL);
                }

                EditorGUILayout.LabelField("Quick start examples:");
                EditorGUILayout.SelectableLabel(t.quickStartPath, linkStyle);


                EditorGUILayout.LabelField("Support e-mail");
                EditorGUILayout.SelectableLabel(t.supportEmail, linkStyle);

                OnSceneGUIGraphics.EditorGUILayoutSeparator();
            }

 
        }




        private void OnDisable() {
  
            if (logo != null) {
                logo.Dispose();
            }

            if (versionNumberLR != null) {
                versionNumberLR.Dispose();
            }

            if (logoViewport != null) {
                logoViewport.Dispose();
            }
        }

 
    }
}
