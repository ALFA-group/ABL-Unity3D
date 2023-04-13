using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy;


namespace LinefyExamples {

 

    [CustomEditor(typeof(HandlesQuickStart))]
    public class HandlesQuickStartEditor : Editor {
        Matrix4x4 tm = Matrix4x4.identity;
 
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(0, 1, 0);
        Vector3 p2 = new Vector3(1, 1, 0);
        Vector3 p3 = new Vector3(1, 0, 0);

        Vector3ArrayHandle v3ah;

        Matrix4x4Handle tmHandle;
        Vector3Handle h0;
        Vector3Handle h1;
        Vector3Handle h2;
        Vector3Handle h3;

        Polyline contour;
        PolygonalMesh fill;

        void OnEnable() {
 
            tmHandle = new Matrix4x4Handle("tm", 0, null, null, null);
            h0 = new Vector3Handle(0);
            h1 = new Vector3Handle(1);
            h2 = new Vector3Handle(2);
            h3 = new Vector3Handle(3);
 
 
            contour = new Polyline(4, true, 1, true);
            contour.widthMultiplier = 4;
            Polygon fillPolygon = new Polygon(4);
            fillPolygon[0] = new PolygonCorner(0, 0, 0);
            fillPolygon[1] = new PolygonCorner(1, 0, 0);
            fillPolygon[2] = new PolygonCorner(2, 0, 0);
            fillPolygon[3] = new PolygonCorner(3, 0, 0);
            fill = PolygonalMesh.BuildProcedural(new Vector3[4], null, new Color[1] { new Color(0, 1, 0, 0.5f) }, new Polygon[] { fillPolygon });

        }

        void OnSceneGUI() {
            tmHandle.DrawOnSceneGUI(ref tm, 2, true);

            Handles.matrix = tm;
            p0 = h0.DrawOnSceneGUI(p0);
            p1 = h1.DrawOnSceneGUI(p1);
            p2 = h2.DrawOnSceneGUI(p2);
            p3 = h3.DrawOnSceneGUI(p3);
 

            contour.SetPosition(0, p0);
            contour.SetPosition(1, p1);
            contour.SetPosition(2, p2);
            contour.SetPosition(3, p3);

            fill.SetPosition(0, p0);
            fill.SetPosition(1, p1);
            fill.SetPosition(2, p2);
            fill.SetPosition(3, p3);

            OnSceneGUIGraphics.DrawWorldspace(contour, Handles.matrix);
            OnSceneGUIGraphics.DrawWorldspace(fill, Handles.matrix);

        }

        private void OnDisable() {

            tmHandle.Dispose();
            h0.Dispose();
            h1.Dispose();
            h2.Dispose();
            h3.Dispose();
            contour.Dispose();
            fill.Dispose();
        }
    }
}
