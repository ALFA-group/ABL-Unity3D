using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CustomEditor(typeof(LinefyGrid2d))]
    public class Grid2dEditor : MonoBehaviourEditorsBase{
    
        [MenuItem("GameObject/3D Object/Linefy/Primitives/Grid2d", false, 3)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefyGrid2d.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }
}
