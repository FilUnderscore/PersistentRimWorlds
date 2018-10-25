using UnityEngine;
using Verse;

namespace PersistentWorlds.Utils
{
    public static class WidgetExtensions
    {
        public static bool ButtonImage(Rect butRect, Texture tex, Color baseColor, Color mouseoverColor)
        {
            GUI.color = !Mouse.IsOver(butRect) ? baseColor : mouseoverColor;

            GUI.DrawTexture(butRect, tex);
            
            GUI.color = baseColor;

            return Widgets.ButtonInvisible(butRect);
        }
    }
}