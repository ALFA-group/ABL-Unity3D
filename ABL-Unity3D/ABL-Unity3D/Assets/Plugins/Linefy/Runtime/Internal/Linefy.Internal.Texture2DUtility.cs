using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {

    public static class Texture2DUtility {

        public static Texture2D InvertRGB(this Texture2D source) {
            Texture2D result = Object.Instantiate(source) as Texture2D;
            result.hideFlags = HideFlags.HideAndDontSave;
            Color[] colors = result.GetPixels();
            for (int c = 0; c < colors.Length; c++) {
                colors[c].r = 1f - colors[c].r;
                colors[c].g = 1f - colors[c].g;
                colors[c].b = 1f - colors[c].b;
            }
            //Texture2D result = new Texture2D(onIcon.width, onIcon.height, TextureFormat.ARGB32, false);
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        public static Texture2D MultiplyAlpha(this Texture2D source, float multiplier) {
            Texture2D result = Object.Instantiate(source) as Texture2D;
            result.hideFlags = HideFlags.HideAndDontSave;
            Color[] colors = result.GetPixels();
            for (int c = 0; c < colors.Length; c++) {
                colors[c].a =  colors[c].a * multiplier;
            }
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        public static Texture2D SolidTexture2D(int sizeX, int sizeY, Color color) {
            Texture2D result = new Texture2D(sizeX, sizeY, TextureFormat.RGBA32, false, false);
            result.hideFlags = HideFlags.HideAndDontSave;
            Color[] colors = new Color[sizeX * sizeY];
            for (int i = 0; i<colors.Length; i++) {
                colors[i] = color;
            }
            result.SetPixels(colors);
            result.Apply();
            return result;
        }

        public static Texture2D GetReadableCopy(this Texture2D source) {
            RenderTexture tmp = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);
 
            Graphics.Blit(source, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D result = new Texture2D(source.width, source.height);
            result.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            return result;
        }
    }
}
