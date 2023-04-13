using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CustomEditor(typeof(LinefyCone))]
    public class ConeEditor : MonoBehaviourEditorsBase{

        [MenuItem("GameObject/3D Object/Linefy/Primitives/Cone", false, 2)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefyCone.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }
}
