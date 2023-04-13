using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using UnityEngine.UI;

namespace LinefyExamples {

    /// <summary>
    /// This example shows how to draw linefy objects relative to RectTransform
    /// </summary>
    [ExecuteInEditMode]
 
    public class UIPolyline : LinefyRectTransform {
        Polyline graphFrame;
        Polyline graph;
 
        public float[] graphValues = new float[2] { 0, 1 };
 
        void Update() {
           
            //create polyline
            if (graph == null) {
                graph = new Polyline(1);
            }

            if (graphFrame == null) {
                //frame is closed polyline with fixed 4 corners
                graphFrame = new Polyline(4, true, 1, true);
                graphFrame.SetPosition(0, new Vector3(0, 0, 0));
                graphFrame.SetPosition(1, new Vector3(0, 1, 0));
                graphFrame.SetPosition(2, new Vector3(1, 1, 0));
                graphFrame.SetPosition(3, new Vector3(1, 0, 0));
                graphFrame.widthMultiplier = 2;
                graphFrame.colorMultiplier = Color.white;
            }

            graph.widthMultiplier = 3;
            graph.colorMultiplier = Color.red;

            //assign polyline positions
            graph.count = graphValues.Length;
            for (int i = 0; i < graphValues.Length; i++) {
                float xPos = i / (float)(graphValues.Length - 1);
                Vector2 point = new Vector2(xPos, graphValues[i]);
                graph.SetPosition(i, point);
            }

            graph.Draw(rectTransformWorldMatrix);
            graphFrame.Draw(rectTransformWorldMatrix);

        }
    }
}
