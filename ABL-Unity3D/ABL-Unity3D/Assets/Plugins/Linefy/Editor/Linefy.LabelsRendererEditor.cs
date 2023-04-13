using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LinefyLabelsRenderer))]
    public class LinefyLabelsRenderEditor : MonoBehaviourEditorsBase
    {
        [MenuItem("GameObject/3D Object/Linefy/LabelsRenderer", false, 1)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefyLabelsRenderer.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }
}
