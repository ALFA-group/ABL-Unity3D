using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CustomEditor(typeof(LinefyBox))]
    public class BoxEditor : MonoBehaviourEditorsBase
    {
        [MenuItem("GameObject/3D Object/Linefy/Primitives/Box", false, 1)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefyBox.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }
}
