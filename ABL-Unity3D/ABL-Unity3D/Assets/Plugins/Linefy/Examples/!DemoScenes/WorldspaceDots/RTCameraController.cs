using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Internal;

[ExecuteInEditMode]
public class RTCameraController : MonoBehaviour
{

    Lines gridLines;
    Camera cam;


    public Vector2Int gridDimesion = new Vector2Int(2,2);

    private void Update() {
        if (cam == null) {
            cam = gameObject.GetComponent<Camera>();
        }

        int linesCount = gridDimesion.x + 1 + gridDimesion.y + 1;
        if (gridLines == null) {
            gridLines = new Lines(linesCount);
        }
 
        Matrix4x4 camTM = Matrix4x4Utility.FarClipPlaneViewportMatrix(cam);
        gridLines.count = linesCount;
        float stepX = 1f / (float)gridDimesion.x;
        float stepY = 1f / (float)gridDimesion.y;
 

        int linesCounter = 0;
        for (int x = 0; x <= gridDimesion.x; x++) {
            Vector2 a = new Vector2(  x * stepX, 0);
            Vector2 b = new Vector2(  x * stepX, 1);
            gridLines.SetPosition(linesCounter, a, b);
            linesCounter++;
        }

        for (int y = 0; y <= gridDimesion.y; y++) {
            Vector2 a = new Vector2(0,   y * stepY);
            Vector2 b = new Vector2(1,   y * stepY);
            gridLines.SetPosition(linesCounter, a, b);
            linesCounter++;
        }

        gridLines.Draw(camTM);
 
    }
}
