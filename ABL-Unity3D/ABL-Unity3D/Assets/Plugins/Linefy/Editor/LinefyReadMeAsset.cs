using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {
    [HelpURL("https://polyflow.xyz/content/linefy/documentation/")]
    public class LinefyReadMeAsset : ScriptableObject {
        [Header("Press R to enable release view")]

        public GUIStyle urlLink;
        public string versionName = "1.0";
        public string documentationURL = "https://polyflow.xyz/content/linefy/documentation/";
        public string assetStoreURL = "http://u3d.as/1NPS";
        public string forumThreadURL = "https://forum.unity.com/threads/linefy-gpu-powered-cross-platform-lines.874792/";

        public string quickStartPath = "Plugins/Linefy/(can be deleted) Examples/!QuickStart/";
        public string supportEmail = "polyflow3d@gmail.com";
        public string initedVersion = "none";
        public bool releaseView;
        public Texture2D debugRedTexture;

        public GUIStyle sceneViewFloaterBackground_darkSkin = new GUIStyle();
        public GUIStyle sceneViewFloaterBackground_brightSkin = new GUIStyle();

        public bool enableScreenshotControls;

        public DotsAtlas logoFont;
        public Texture2D logoTexture;
        public float logoSize = 10;
        public AnimationCurve logoAnimCurve = new AnimationCurve();

        [Multiline]
        public string description;

        [Header("GUI Content")]
        public EditorIconContent onSceneGUIEditModeEditablePolyline;
		public EditorIconContent onSceneGUIEditModeEditableLines;
		public EditorIconContent onSceneGUIEditModeEditableDots;
		
		public EditorIconContent drawLabelsToggle;

    }
}