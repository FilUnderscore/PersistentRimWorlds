using UnityEngine;
using Verse;
using Verse.Sound;

namespace PersistentWorlds.UI
{
    public class PersistentWorldWidgets
    {
        private static readonly Texture2D ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");

        private static readonly Texture2D ButtonBGAtlasMouseover =
            ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover");

        private static readonly Texture2D ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick");
        
        public static bool ButtonImageOn(Rect rect, Texture2D tex,
            bool doMouseoverSound = true)
        {
            var atlas = ButtonBGAtlas;

            if (Mouse.IsOver(rect))
            {
                atlas = ButtonBGAtlasMouseover;

                if (Input.GetMouseButton(0))
                    atlas = ButtonBGAtlasClick;
            }
            
            Widgets.DrawAtlas(rect, atlas);
            
            if(doMouseoverSound)
                MouseoverSounds.DoRegion(rect);

            GUI.DrawTexture(rect.ScaledBy(0.75f), (Texture) tex);

            return Widgets.ButtonInvisible(rect, false);
        }
    }
}