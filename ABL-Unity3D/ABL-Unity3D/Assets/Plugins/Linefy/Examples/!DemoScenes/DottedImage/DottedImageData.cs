using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Serialization;

//[CreateAssetMenu(menuName = "Sword Data")]
namespace LinefyExamples {
    [PreferBinarySerialization]
    public class DottedImageData : ScriptableObject {
  

        [HideInInspector]
        public Vector3[] points = new Vector3[0];
        //public SerializationDataFull_Dots data = new SerializationDataFull_Dots();
        public int generationHash;

        public void generate(Texture2D photo, int dotsCount) {
            Distribution2d rectDistribution = new Distribution2d(dotsCount , new Vector2(1, 1), 3);
            points = new Vector3[dotsCount];
            for (int i = 0; i < rectDistribution.samplesCount; i++) {
                Vector2 coords = rectDistribution[i];
                float power = 1f - photo.GetPixelBilinear(coords.x, coords.y).grayscale;
                points[i] = rectDistribution[i];
                points[i].z = power;
 
            }
 
            generationHash++;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }


}
