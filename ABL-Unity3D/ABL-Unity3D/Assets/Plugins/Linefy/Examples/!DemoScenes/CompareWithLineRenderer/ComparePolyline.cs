using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

[ExecuteInEditMode]
public class ComparePolyline : MonoBehaviour
{
    Polyline pl;
    public float width;
    public Color colormult;

 
    void Update()
    {
        if (pl == null) {
            pl = new Polyline(3, false, 0, true, Color.white, 1);
            pl.widthMode = WidthMode.WorldspaceBillboard;
            pl.SetPosition(0, new Vector3(-1,-1, 0));
            pl.SetPosition(1, new Vector3(0, 1, 0));
            pl.SetPosition(2, new Vector3(1, -1, 0));
        }
        pl.colorMultiplier = colormult;
        pl.widthMultiplier = width;
        pl.Draw(transform.localToWorldMatrix);
    }
}
