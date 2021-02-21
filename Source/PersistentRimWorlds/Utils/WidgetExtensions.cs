using UnityEngine;
using Verse;
using Verse.Sound;

namespace PersistentWorlds.Utils
{
    [StaticConstructorOnStartup]
    public static class WidgetExtensions
    {
        private static readonly Texture2D ButtonBgAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");

        private static readonly Texture2D ButtonBgAtlasMouseover =
            ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover");

        private static readonly Texture2D ButtonBgAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick");

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
        
        public static bool ButtonImageOn(Rect rect, Texture2D tex,
            bool doMouseoverSound = true)
        {
            var atlas = ButtonBgAtlas;

            if (Mouse.IsOver(rect))
            {
                atlas = ButtonBgAtlasMouseover;

                if (Input.GetMouseButton(0))
                    atlas = ButtonBgAtlasClick;
            }
            
            Widgets.DrawAtlas(rect, atlas);
            
            if(doMouseoverSound)
                MouseoverSounds.DoRegion(rect);

            GUI.DrawTexture(rect.ScaledBy(0.75f), (Texture) tex);

            return Widgets.ButtonInvisible(rect, false);
        }
    }
}