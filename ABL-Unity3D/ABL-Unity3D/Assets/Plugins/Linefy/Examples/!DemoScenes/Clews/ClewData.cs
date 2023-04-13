using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Serialization;

namespace LinefyExamples {
     public class ClewData : ScriptableObject {

        [Range(4, 256)]
        public int knotsCount = 64;
        [Range(2, 16)]
        public int segmentsCount = 5;

        public SerializationData_Polyline clewData;

        public PolylineVertex[] vertices;


    }
}
