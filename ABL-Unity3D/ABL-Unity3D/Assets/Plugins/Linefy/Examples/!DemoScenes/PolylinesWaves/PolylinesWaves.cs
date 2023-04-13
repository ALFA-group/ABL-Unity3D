using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;

[ExecuteInEditMode]
public class PolylinesWaves : MonoBehaviour {

    public AnimationCurve ac;

    [Range(32, 256)]
    public int segmentsCount = 128; 

    [Range(1,32)]
    public int polylinesCount;
 

    Polyline[] polylines;

    public float waveScale;
    public Vector2 size = new Vector2(3,1);
    public float waveHeight = 1;
    public float animSpeed;

    public int renderOrder = 0;
 
 
	void Update () {
        if (polylines == null || polylines.Length != polylinesCount || polylines[0].count != segmentsCount+1) {
            polylines = new Polyline[polylinesCount];
            for (int i = 0; i<polylines.Length; i++) {
                Polyline pl =  new Polyline(segmentsCount+1, true,  2, false, Color.white, (0.5f + Random.value  ));
                pl.widthMode = WidthMode.PercentOfScreenHeight;
                pl.colorMultiplier = Color.HSVToRGB( i/(float)(polylinesCount), 1, 1);
                polylines[i] = pl;
            }
        }

        float animVal = 0;
        if (Application.isPlaying) {
            animVal = Time.time * animSpeed;
        }

        float yPosStep = size.y / (float)(polylinesCount - 1);
        float yOffset = -size.y / 2f;
        float xPosStep = size.x / (float)(segmentsCount - 1);
        float normalizedStep = 1f / (segmentsCount - 1);
        for (int v = 0; v<=segmentsCount; v++) {
            float normalizedX = v * normalizedStep;
            float xPos = v * xPosStep;
            float curveMult = ac.Evaluate(normalizedX);
            float perlin = Mathf.PerlinNoise(normalizedX * waveScale + animVal, 0) * waveHeight ;
            perlin -= waveHeight / 2f;
            //Mathf.PerlinNoise(xPos * scale + animVal, 0) * waveHeight;
            for (int p = 0; p < polylines.Length; p++) {
                float yPos = (yOffset + perlin + p * yPosStep) * curveMult  ;
                polylines[p].SetPosition(v, new Vector3(xPos, yPos));
            }
        }

        for  (int i = 0; i < polylines.Length; i++) {
            polylines[i].renderOrder = renderOrder;
            polylines[i].Draw( transform.localToWorldMatrix );
        }

    }
}
