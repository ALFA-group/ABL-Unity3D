using System;
using System.Collections.Generic;
using UnityEngine;
using Linefy;


[ExecuteInEditMode]
public class LinefyDemo_PlexusDemo_LogoAnimator : MonoBehaviour
{

    public float posFrom = - 20;
    public float posTo = 60;
    public float duration = 5;
    [Multiline]
    public string text;
    public float spacing;
    public bool setLines;
    public LinefyLabelsRenderer llr;
    
 
    void Update()
    {
        if (UnityEngine.Application.isPlaying) {
            float nt = (Time.timeSinceLevelLoad ) / duration;
            transform.localPosition = new Vector3(0, Mathf.Lerp(posFrom, posTo, nt), 0);
        }

        if (setLines) {
            string[] lines = text.Split(  new[] { Environment.NewLine }, StringSplitOptions.None);
            llr.labels = new Label[lines.Length];
            for (int i = 0; i<lines.Length; i++) {
                llr.labels[i].text = lines[i];
                llr.labels[i].position = new Vector3(0,-i*spacing, 0);
            }
            setLines = false;
        } 
    }
}
