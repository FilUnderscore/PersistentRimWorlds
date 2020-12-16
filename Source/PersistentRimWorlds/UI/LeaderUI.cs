using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public static class LeaderUI
    {
        private static Vector2 scrollPosition;
        
        public static void DrawColonistsMenu(ref Rect inRect, float margin, List<Pawn> colonists)
        {
            const int perRow = 3;
            var gap = (int) margin;

            inRect.width += gap;
            
            UITools.DrawBoxGridView(out _, out _, ref inRect, ref scrollPosition, perRow, gap, (i, boxRect) =>
            {
                Pawn colonist = colonists[i];
                
                Widgets.DrawAltRect(boxRect);
                
                var portraitSize = new Vector2(boxRect.width / 2, boxRect.height);
                Texture pawnTexture = PortraitsCache.Get(colonist, portraitSize);
                
                GUI.DrawTexture(boxRect, pawnTexture);
                
                return true;
            }, colonists.Count);
        }

        public static void Reset()
        {
            
        }
    }
}