using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CustomEditor(typeof(LinefyCircularPolyline))]
    public class LinefyCircularPolylineEditor : MonoBehaviourEditorsBase
    {
        [MenuItem("GameObject/3D Object/Linefy/Primitives/CircularPolyline", false, 0)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefyCircularPolyline.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }


    }
}
