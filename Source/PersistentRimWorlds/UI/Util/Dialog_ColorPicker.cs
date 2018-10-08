using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_ColorPicker : Window
    {
        //private Color selectedColor;
        //private Texture2D colorWheel;

        private float rValue = 128;
        private float gValue = 128;
        private float bValue = 128;
        
        public override Vector2 InitialSize => new Vector2(600, 300);
        
        public Dialog_ColorPicker()
        {
            this.doCloseX = true;
            this.doCloseButton = true;
            this.forcePause = true;
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            
            var redRect = new Rect(0, 40, inRect.width * 0.70f, 20);
            var greenRect = new Rect(0, 80, inRect.width * 0.70f, 20);
            var blueRect = new Rect(0, 120, inRect.width * 0.70f, 20);

            var redSideRect = new Rect(inRect.width * 0.70f + 20, 40, inRect.width * 0.25f, 20);
            var greenSideRect = new Rect(inRect.width * 0.70f + 20, 80, inRect.width * 0.25f, 20);
            var blueSideRect = new Rect(inRect.width * 0.70f + 20, 120, inRect.width * 0.25f, 20);
            
            Text.Font = GameFont.Medium;

            rValue = (int) Widgets.HorizontalSlider(redRect, rValue, 0, 255, false, null, "Red");
            gValue = (int) Widgets.HorizontalSlider(greenRect, gValue, 0, 255, false, null, "Green");
            bValue = (int) Widgets.HorizontalSlider(blueRect, bValue, 0, 255, false, null, "Blue");

            Text.Font = GameFont.Small;
            
            Widgets.Label(redSideRect, ((int) rValue).ToString());
            Widgets.Label(greenSideRect, ((int) gValue).ToString());
            Widgets.Label(blueSideRect, ((int) bValue).ToString());
            
            GUI.EndGroup();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            
            //this.PreDrawColors();
        }

        // Color drawing method from https://stackoverflow.com/a/42059976 (Thank you to them)
        /*
        private void PreDrawColors()
        {
            if (colorWheel != null) return;

            colorWheel = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);

            var centerX = colorWheel.width / 2;
            var centerY = colorWheel.height / 2;
            var radius = centerX * centerX;

            var redX = colorWheel.width;
            var redY = centerY;
            var redRad = colorWheel.width * colorWheel.width;

            var greenX = 0;
            var greenY = centerY;
            var greenRad = colorWheel.width * colorWheel.width;

            var blueX = centerX;
            var blueY = colorWheel.height;
            var blueRad = colorWheel.width * colorWheel.width;

            for (var i = 0; i < colorWheel.width; i++)
            {
                for (var j = 0; j < colorWheel.height; j++)
                {
                    var a = (int) (i - centerX);
                    var b = (int) (j - centerY);

                    var distance = (int) (a * a) + (b * b);
                    
                    if (distance < radius)
                    {
                        var rdx = i - redX;
                        var rdy = j - redY;
                        var redDist = (rdx * rdx + rdy * rdy);
                        var redVal = (int) (255 - ((redDist / (float) redRad) * 256));

                        var gdx = i - greenX;
                        var gdy = j - greenY;
                        var greenDist = (gdx * gdx + gdy * gdy);
                        var greenVal = (int) (255 - ((greenDist / (float) greenRad) * 256));

                        var bdx = i - blueX;
                        var bdy = j - blueY;
                        var blueDist = (bdx * bdx + bdy * bdy);
                        var blueVal = (int) (255 - ((blueDist) / (float) blueRad) * 256);
                        
                        var color = new Color(redVal / 255f, greenVal / 255f, blueVal / 255f);
                        float H = 0;
                        float S = 0;
                        float V = 0;
                        Color.RGBToHSV(color, out H, out S, out V);

                        color = Color.HSVToRGB(H, S, 1);
                        
                        colorWheel.SetPixel(i, j, color);
                    }
                    else
                    {
                        colorWheel.SetPixel(i, j, new Color(0, 0, 0, 0));
                    }
                }
            }
            
            colorWheel.Apply();
        }
        
        private void DrawColorBox(Rect rect)
        {
            GUI.DrawTexture(rect, colorWheel);
        }
        */
    }
}