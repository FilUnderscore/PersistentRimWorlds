using System;
using System.Collections.Generic;
using PersistentWorlds.Logic;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public static class ColonyUI
    {
        public static void DrawColoniesList(ref Rect inRect, ref Vector2 scrollPosition, 
            ref List<PersistentColony> colonies)
        {
            
        }

        public static void DrawColoniesTab(ref Rect inRect, ref Vector2 scrollPosition,
            List<PersistentColony> colonies, Action<int> load)
        {
            const int colonyBoxHeight = 100;
            
            var viewRect = new Rect(0, 0, inRect.width, colonies.Count * colonyBoxHeight);
            var outRect = new Rect(inRect.AtZero());
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (var i = 0; i < colonies.Count; i++)
            {
                var colony = colonies[i];
                
                var colonyData = colony.ColonyData;
                
                var boxRect = new Rect(0, 0, colonyBoxHeight, colonyBoxHeight);
                
                Widgets.DrawBox(boxRect);

                if (Widgets.ButtonInvisible(boxRect))
                {
                    load(i);
                }
            }
            
            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }
    }
}