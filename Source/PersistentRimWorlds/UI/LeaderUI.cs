using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PersistentWorlds.UI
{
    public static class LeaderUI
    {
        private static Vector2 _scrollPosition;
        
        public static void DrawColonistsMenu(ref Rect inRect, float margin, List<Pawn> colonists, Action<Pawn> set)
        {
            const int perRow = 3;
            var gap = (int) margin;

            inRect.width += gap;
            
            UITools.DrawBoxGridView(out _, out _, ref inRect, ref _scrollPosition, perRow, gap, (i, boxRect) =>
            {
                var colonist = colonists[i];
                
                Widgets.DrawAltRect(boxRect);
                Widgets.DrawHighlightIfMouseover(boxRect);
                
                var portraitSize = new Vector2(boxRect.width / 2, boxRect.height);
                var portraitRect = new Rect(boxRect.x + boxRect.width * 0.5f - portraitSize.x / 2, boxRect.y, portraitSize.x, portraitSize.y);
                
                Texture pawnTexture = PortraitsCache.Get(colonist, portraitSize);

                GUI.DrawTexture(portraitRect, pawnTexture);
                TooltipHandler.TipRegion(boxRect, "FilUnderscore.PersistentRimWorlds.Colony.ChangeLeader.Select".Translate(colonist.Name.ToStringFull));

                if (Widgets.ButtonInvisible(boxRect))
                {
                    set(colonist);
                }
                
                return true;
            }, colonists.Count);
        }

        public static void Reset()
        {
            
        }
    }
}