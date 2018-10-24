using System;
using System.Collections.Generic;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public static class ColonyUI
    {
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");
        
        public static void DrawColoniesList(ref Rect inRect, ref Vector2 scrollPosition, 
            ref List<PersistentColony> colonies)
        {
            
        }

        public static void DrawColoniesTab(ref Rect inRect, ref Vector2 scrollPosition,
            List<PersistentColony> colonies, Action<int> load)
        {
            // TODO: Scale depending on screen size?
            
            const int perRow = 6;
            const int gap = 25;
            var colonyBoxWidth = (inRect.width - gap * perRow) / perRow;
            
            var viewRect = new Rect(0, 0, inRect.width, Mathf.Ceil((float) colonies.Count / perRow) * colonyBoxWidth);
            var outRect = new Rect(inRect.AtZero());
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            
            for (var i = 0; i < colonies.Count; i++)
            {
                var y = colonyBoxWidth * Mathf.Floor((float) i / perRow);
                
                var colony = colonies[i];
                
                //var boxRect = new Rect((colonyBoxWidth * (i % perRow)) + ((i % perRow) != 0 ? gap : 0), y, colonyBoxWidth, colonyBoxWidth);
                var boxRect = new Rect((colonyBoxWidth * (i % perRow)) + (i % perRow) * gap, y, colonyBoxWidth,
                    colonyBoxWidth);
                
                Widgets.DrawAltRect(boxRect);
                
                var textureRect = new Rect(boxRect.x, boxRect.y, Town.width, Town.height);

                if (colony.ColonyData.Leader != null)
                {
                    var portraitSize = new Vector2(boxRect.width / 4, boxRect.height);
                    var leaderPortrait = PortraitsCache.Get(colony.ColonyData.Leader, portraitSize);

                    var leaderRect = new Rect(boxRect.x + boxRect.width * 0.75f, boxRect.y, leaderPortrait.width,
                        leaderPortrait.height);

                    GUI.DrawTexture(leaderRect, leaderPortrait);
                }

                if (Widgets.ButtonImage(textureRect, Town, colony.ColonyData.color))
                {
                    load(i);
                }
            }
            
            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }
    }
}