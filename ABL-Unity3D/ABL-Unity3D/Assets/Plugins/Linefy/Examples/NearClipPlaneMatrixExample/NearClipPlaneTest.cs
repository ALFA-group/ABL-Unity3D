using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using Linefy.Primitives;

[ExecuteInEditMode]
public class NearClipPlaneTest : MonoBehaviour
{

    public Vector2 guiPosition;
    public Vector2 screenPosition;
    public Camera _camera;
    CircularPolyline gUISpaceCircle;
    CircularPolyline screenSpaceCircle;

    void Update()
    {
        if (_camera != null) {
            if (gUISpaceCircle == null) {
                gUISpaceCircle = new CircularPolyline(32, 10);
            }

            if (screenSpaceCircle == null) {
                screenSpaceCircle = new CircularPolyline(32, 15, Color.red);
            }
 
            gUISpaceCircle.Draw(NearClipPlaneMatrix.GUISpace(_camera) * Matrix4x4.Translate(guiPosition));

            screenSpaceCircle.Draw(NearClipPlaneMatrix.ScreenSpace(_camera) * Matrix4x4.Translate(screenPosition));

        }
    }
}
