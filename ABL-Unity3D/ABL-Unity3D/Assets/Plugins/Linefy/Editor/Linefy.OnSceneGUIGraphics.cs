using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy;
using Linefy.Internal;


namespace Linefy {
    [InitializeOnLoad]
    public static class OnSceneGUIGraphics {
 
        static List<Drawable> wordspaceItems;
        static List<Drawable> guispaceItems;
        static HashSet<Drawable> wordspaceHs;
        static HashSet<Drawable> guispaceHs;

        static System.Diagnostics.Stopwatch selfStopwatch;
        static float selfMilliseconds;
        static System.Diagnostics.Stopwatch frameStopwatch = new System.Diagnostics.Stopwatch();
        public static float onScenGUIRepaintDeltaTime;
        static float frameMilliseconds = 10f;

        static UnityEngine.GUIStyle statisticLabelStyle;
        static string statisticText;
        static Matrix4x4 clipPlaneMatrix;
        static Camera sceneViewCamera;

        static Texture2D _transparentBlackTexture;
        public static Texture2D transparentBlackTexture {
            get {
                if (_transparentBlackTexture == null) {
                    _transparentBlackTexture = new Texture2D(1, 1);
                    _transparentBlackTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.4f));
                    _transparentBlackTexture.Apply(true, true);
                }
                return _transparentBlackTexture;
            }
        }

        public static Action drawGUIBeforeLinefyObjects = () => { };
        public static Action drawGUIAfterLinefyObjects = () => { };

        static bool _showStatistic = EditorPrefs.GetBool("PolyflowStudio.Linefy.ShowStatistic");
        public static bool showStatistic {
            get {
                return _showStatistic;
            }

            set {
                if (value != _showStatistic) {
                    _showStatistic = value;
                    EditorPrefs.SetBool("PolyflowStudio.Linefy.ShowStatistic", _showStatistic);
                }
            }
        }

        static bool _darkSkin = EditorPrefs.GetBool("PolyflowStudio.Linefy.DarkSkin");
        public static bool darkSkin {
            get {
                return _darkSkin;
            }

            set {
                if (value != _darkSkin) {
                    _darkSkin = value;
                    EditorPrefs.SetBool("PolyflowStudio.Linefy.DarkSkin", _darkSkin);
                }
            }
        }

        static LinefyReadMeAsset _readme;
        public static LinefyReadMeAsset readme {
            get {
                if (_readme == null) {
                    string[] assetGUIDs = AssetDatabase.FindAssets("t:LinefyReadMeAsset");
                    if (assetGUIDs == null || assetGUIDs.Length == 0) {
                        Debug.LogFormat("{0} not found. Please reinstall package", "LINEFY README.asset");
                    } else {
                        string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
                        _readme = (LinefyReadMeAsset)AssetDatabase.LoadAssetAtPath(path, typeof(LinefyReadMeAsset));
                    }
                }
                return _readme;
            }

        }

        static OnSceneGUIGraphics() {
            wordspaceItems = new List<Drawable>(64);
            guispaceItems = new List<Drawable>(64);

            wordspaceHs = new HashSet<Drawable>();
            guispaceHs = new HashSet<Drawable>();

            selfStopwatch = new System.Diagnostics.Stopwatch();
            statisticLabelStyle = new GUIStyle();
            statisticLabelStyle.fontSize = 12;
            statisticLabelStyle.normal.textColor = new Color(1, 1, 1, 0.5f);
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif

        }

        public static Vector3 WorldToGUIPoint(Vector3 worldPoint) {
            if (sceneViewCamera) {
                Vector3 r = sceneViewCamera.WorldToScreenPoint(Handles.matrix.MultiplyPoint3x4(worldPoint));
                r.y = sceneViewCamera.pixelHeight - r.y;
                return r;
            } else {
                return worldPoint;
            }
        }

        public static Ray GUIPointToRay(Vector2 guiPoint) {

            if (sceneViewCamera) {

                return sceneViewCamera.ScreenPointToRay(GUIUtility.GUIToScreenPoint(guiPoint));

            } else {
                return new Ray();
            }
        }

        public static Vector2 WorldToGUIPoint(Vector3 worldPoint, ref float distance) {
            if (sceneViewCamera) {
                Vector3 r = sceneViewCamera.WorldToScreenPoint(Handles.matrix.MultiplyPoint3x4(worldPoint));
                distance = r.z;
                r.y = sceneViewCamera.pixelHeight - r.y;
                return r;
            } else {
                return worldPoint;
            }
        }

        static void OnSceneGUI(SceneView sv) {

            Handles.BeginGUI();
            drawGUIBeforeLinefyObjects();
            drawGUIBeforeLinefyObjects = () => { };
            Handles.EndGUI();


            if (readme != null && readme.initedVersion != readme.versionName) {
                readme.initedVersion = readme.versionName;
                EditorUtility.SetDirty(readme);
                string logText = string.Format("   ★★★ Welcome to Linefy {0} ★★★ \n", readme.versionName);
                Selection.SetActiveObjectWithContext(readme, null);
                Debug.LogFormat(logText);
            }


            Event e = Event.current;
            sceneViewCamera = Camera.current;

            if (e.type == EventType.Layout) {
                sv.Repaint();
            } else if (sceneViewCamera != null && e.type == EventType.Repaint) {
                selfStopwatch.Reset();
                selfStopwatch.Start();

                //wordspaceItems.Sort(OrderComparison);
                //guispaceItems.Sort(OrderComparison);

                foreach (Drawable entity in wordspaceItems) {
                    //Debug.LogFormat("entity.onSceneGUIMatrix = {0}", entity.onSceneGUIMatrix);
                    entity.DrawNow(entity.onSceneGUIMatrix);
                }

                float clipPlaneOffset = (sceneViewCamera.farClipPlane - sceneViewCamera.nearClipPlane) * 0.00001f;
                clipPlaneMatrix = Matrix4x4Utility.NearClipPlaneGUISpaceMatrix(sceneViewCamera, clipPlaneOffset);

                foreach (Drawable entity in guispaceItems) {
                    entity.DrawNow(clipPlaneMatrix * entity.onSceneGUIMatrix);
                }

                selfStopwatch.Stop();
                selfMilliseconds = (float)selfStopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond;
                frameStopwatch.Stop();
                float nFrameMilliseconds = frameStopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond;
                onScenGUIRepaintDeltaTime = nFrameMilliseconds / 1000f;
                frameMilliseconds = ((frameMilliseconds * 19) + nFrameMilliseconds) / 20f;
                frameStopwatch.Reset();
                frameStopwatch.Start();

                if (showStatistic) {
                    statisticText = string.Format("draw: {0}ms total repaint: {1}ms  \n", selfMilliseconds.ToString("F2"), frameMilliseconds.ToString("F2"));
                    int linesGroupsCount = 0;
                    int linesCount = 0;
                    int dotsGroupsCount = 0;
                    int dotsCount = 0;
                    int polylinesCount = 0;
                    int polylineVertsCount = 0;

                    CollectInfo(wordspaceItems, ref linesGroupsCount, ref dotsGroupsCount, ref polylinesCount, ref linesCount, ref dotsCount, ref polylineVertsCount);
                    CollectInfo(guispaceItems, ref linesGroupsCount, ref dotsGroupsCount, ref polylinesCount, ref linesCount, ref dotsCount, ref polylineVertsCount);

                    statisticText += string.Format("    {0} lines groups, {1} total lines \n", linesGroupsCount, linesCount);
                    statisticText += string.Format("    {0} dots groups, {1} total dots \n", dotsGroupsCount, dotsCount);
                    statisticText += string.Format("    {0} polylines, {1} total polyline vertices \n", polylinesCount, polylineVertsCount);
                }

                wordspaceItems.Clear();
                wordspaceHs.Clear();
                guispaceItems.Clear();
                guispaceHs.Clear();
            }

            Handles.BeginGUI();
            drawGUIAfterLinefyObjects();
            drawGUIAfterLinefyObjects = () => { };

            if (showStatistic) {

                Rect infoRect = new Rect(12, 12, 320, 110);
                GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
                GUI.Box(infoRect, "Linefy.OnSceneGUIGraphics statistic", GUI.skin.window);
                infoRect.x += 14;
                infoRect.y += 24;
                GUI.Label(infoRect, statisticText, statisticLabelStyle);

                Rect closeButtonRect = new Rect(304, 32, 22, 22);
                if (GUI.Button(closeButtonRect, "X")) {
                    showStatistic = false;
                    Editor ed = ScriptableObject.CreateInstance<Editor>();

                    ed.Repaint();
                }
            }

            Handles.EndGUI();
        }

        static void CollectInfo(List<Drawable> list, ref int lgroups, ref int dgroups, ref int pgroups, ref int lcount, ref int dcount, ref int pcount) {
            foreach (Drawable le in list) {
                le.GetStatistic(ref lgroups, ref lcount, ref dgroups, ref dcount, ref pgroups, ref pcount);
            }
        }

        public static void DrawWorldspace(Drawable item) {
            if (item != null) {
                if (wordspaceHs.Add(item)) {
                    item.onSceneGUIMatrix = Matrix4x4.identity;
                    wordspaceItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Actually sends objects to render on Scene View. Use in OnSceneGUI() only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        public static void DrawWorldspace(Drawable item, Matrix4x4 matrix) {
            if (item != null) {
                if (wordspaceHs.Add(item)) {
                    item.onSceneGUIMatrix = matrix;
                    wordspaceItems.Add(item);
                }
            }
        }

        public static void DrawGUIspace(Drawable item) {
            if (item != null) {
                if (guispaceHs.Add(item)) {
                    guispaceItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Actually sends objects to render on Scene View. Use in OnSceneGUI() only.
        /// </summary>
        /// <param name="matrix">screen Space matrix</param>
        public static void DrawGUIspace(Drawable item, Matrix4x4 matrix) {
            if (item != null) {
                if (guispaceHs.Add(item)) {
                    item.onSceneGUIMatrix = matrix;
                    guispaceItems.Add(item);
                }
            }
        }

        public static Vector2 GetSceneViewPixelSize() {
            Vector2 result = Vector2.zero;
            if (sceneViewCamera != null) { 
                result.x = sceneViewCamera.pixelWidth ;
                result.y = sceneViewCamera.pixelHeight;
            }
            return result;
        }

        #region styles
        public static bool useDarkSkin {
            get {
                return darkSkin || EditorGUIUtility.isProSkin;
            }
        }

        static GUIStyle _buttonStyle;
        public static GUIStyle buttonStyle {
            get {
                if (_buttonStyle == null) {
                    _buttonStyle = new GUIStyle("Button");
                    _buttonStyle.normal.background = null;
                    _buttonStyle.padding = new RectOffset();
                }
                return _buttonStyle;
            }
        }

        public static bool DrawToggle(Rect r, EditorIconContent content, bool state) {
            if (useDarkSkin) {
                return GUI.Toggle(r, state, content.bright, buttonStyle);
            } else {
                if (state) {
                    return GUI.Toggle(r, state, content.bright, buttonStyle);
                } else {
                    return GUI.Toggle(r, state, content.dark, buttonStyle);
                }
            }
        }

        public static void EditorGUILayoutSeparator() {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            Color c = OnSceneGUIGraphics.useDarkSkin ? new Color(1.0f, 1.0f, 1.0f, 0.5f) : new Color(0.0f, 0.0f, 0.0f, 0.5f);
            EditorGUI.DrawRect(rect, c);
            EditorGUILayout.Space();
        }

        public static void  GUILayoutSeparator( Rect rect ) {
            Color c = OnSceneGUIGraphics.useDarkSkin ? new Color(1.0f, 1.0f, 1.0f, 0.5f) : new Color(0.0f, 0.0f, 0.0f, 0.5f);
            EditorGUI.DrawRect(rect, c);
        }

        public static GUIStyle sceneViewFloaterBackgrond {
            get {
                return useDarkSkin ? readme.sceneViewFloaterBackground_darkSkin : readme.sceneViewFloaterBackground_brightSkin;
            }
        }



        #endregion

    }
}