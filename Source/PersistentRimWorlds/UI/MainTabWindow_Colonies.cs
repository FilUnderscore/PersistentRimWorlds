using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class MainTabWindow_Colonies : MainTabWindow
    {
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");
        // TODO: Draw.
        
        public MainTabWindow_Colonies()
        {
            this.forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            
            GUI.BeginGroup(inRect);

            var boxRect = new Rect(0, 0, 100, 100);
            Widgets.DrawBox(boxRect, 1);
            
            GUI.BeginGroup(boxRect);

            var townTexRect = new Rect(boxRect.x + 10, boxRect.y, 80, 80);
            GUI.DrawTexture(townTexRect, Town);
            
            GUI.EndGroup();
            
            GUI.EndGroup();
        }
    }
}