using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CustomEditor(typeof(LinefyGrid3d))]
    public class Grid3dEditor : MonoBehaviourEditorsBase{
    
        [MenuItem("GameObject/3D Object/Linefy/Primitives/Grid3d", false, 4)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefyGrid3d.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }
}
