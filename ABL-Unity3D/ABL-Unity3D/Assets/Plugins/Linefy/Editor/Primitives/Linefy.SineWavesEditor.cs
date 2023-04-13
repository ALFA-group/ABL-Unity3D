using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Primitives;

namespace Linefy.Editors{
    [CustomEditor(typeof(LinefyCone))]
    public class SineWavesEditor : MonoBehaviourEditorsBase{

        [MenuItem("GameObject/3D Object/Linefy/Primitives/Sine Waves", false, 2)]
        public static void Create(MenuCommand menuCommand) {
            GameObject go = LinefySineWaves.CreateInstance().gameObject;
            postCreate(go, menuCommand);
        }
    }
}
