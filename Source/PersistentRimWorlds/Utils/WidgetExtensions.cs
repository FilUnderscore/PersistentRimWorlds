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
        
        public static void LabelScrollable(Rect rect, string label, ref Vector2 scrollbarPosition, bool scrollbar = true, bool dontConsumeScrollEventsIfNoScrollbar = false, bool takeScrollbarSpaceEvenIfNoScrollbar = true)
        {
            var flag1 = takeScrollbarSpaceEvenIfNoScrollbar || (double) Verse.Text.CalcHeight(label, rect.width) > (double) rect.height;
            var flag2 = flag1 && (!dontConsumeScrollEventsIfNoScrollbar || (double) Verse.Text.CalcHeight(label, rect.width - 16f) > (double) rect.height);
            var width = rect.width;
            if (flag1)
                width -= 16f;
            var rect1 = new Rect(0.0f, 0.0f, width, Mathf.Max(Verse.Text.CalcHeight(label, width) + 5f, rect.height));
            if (flag2)
                Widgets.BeginScrollView(rect, ref scrollbarPosition, rect1, scrollbar);
            else
                GUI.BeginGroup(rect);
            Widgets.Label(rect1, label);
            if (flag2)
                Widgets.EndScrollView();
            else
                GUI.EndGroup();
        }
    }
}