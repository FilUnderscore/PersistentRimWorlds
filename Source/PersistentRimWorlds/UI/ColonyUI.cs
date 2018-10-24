using System;
using System.Collections.Generic;
using PersistentWorlds.Logic;
using PersistentWorlds.Utils;
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
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
         
            // TODO: Scale depending on screen size?   
            const int perRow = 6;
            const int gap = 25;
            var colonyBoxWidth = (inRect.width - gap * perRow) / perRow;
            
            var viewRect = new Rect(0, 0, inRect.width, Mathf.Ceil((float) colonies.Count / perRow) * colonyBoxWidth);
            var outRect = new Rect(inRect.AtZero());
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            SortColonies(ref colonies);
            
            for (var i = 0; i < colonies.Count; i++)
            {
                var y = colonyBoxWidth * Mathf.Floor((float) i / perRow);
                
                var colony = colonies[i];
                
                var boxRect = new Rect((colonyBoxWidth * (i % perRow)) + (i % perRow) * gap, y, colonyBoxWidth,
                    colonyBoxWidth);

                if (Equals(colony, persistentWorld.Colony))
                {
                    Widgets.DrawHighlight(boxRect);
                }
                else
                {
                    Widgets.DrawAltRect(boxRect);
                }

                var size = boxRect.width * 0.65f;

                if (size >= Town.width)
                    size = Town.width;
                
                var textureRect = new Rect(boxRect.x, boxRect.y, size, size);

                Rect leaderRect;
                
                if (colony.ColonyData.Leader != null)
                {
                    var portraitSize = new Vector2(boxRect.width / 4, boxRect.height);
                    var leaderPortrait = PortraitsCache.Get(colony.ColonyData.Leader, portraitSize);

                    leaderRect = new Rect(boxRect.x + boxRect.width * 0.68f, boxRect.y, leaderPortrait.width,
                        leaderPortrait.height);
                    
                    if (Equals(colony, persistentWorld.Colony))
                    {
                        if (WidgetExtensions.ButtonImage(leaderRect, leaderPortrait, Color.white, GenUI.MouseoverColor))
                        {
                            // TODO: Open leader selection dialog if no leader altering mods such as Relations Tab or Psychology is found.
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(leaderRect, leaderPortrait);
                    }
                }
                else
                {
                    leaderRect = new Rect(boxRect.x + boxRect.width * 0.68f, boxRect.y,
                        boxRect.width - boxRect.width * 0.68f,
                        boxRect.height);

                    Text.Font = GameFont.Tiny;
                    Widgets.Label(leaderRect, "FilUnderscore.PersistentRimWorlds.Colony.NoLeader".Translate());
                }

                if (Widgets.ButtonImage(textureRect, Town, colony.ColonyData.color))
                {
                    if (Equals(colony, persistentWorld.Colony))
                    {
                        // TODO: Open color picker.   
                    }
                    else
                    {
                        load(i);
                    }
                }

                var colonyNameRect = new Rect(boxRect.x, textureRect.yMax, boxRect.width - leaderRect.width,
                    boxRect.height - textureRect.yMax);


                Text.Font = GameFont.Small;
                Widgets.Label(colonyNameRect, colony.ColonyData.ColonyFaction.Name);
            }
            
            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }

        private static void SortColonies(ref List<PersistentColony> colonies)
        {
            if (colonies == null || colonies.Count == 0) return;

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            if (Equals(colonies[0], persistentWorld.Colony))
                return;

            for (var i = 0; i < colonies.Count; i++)
            {
                var colony = colonies[i];

                if (i == 0 || !Equals(colony, persistentWorld.Colony)) continue;

                colonies.RemoveAt(i);
                colonies.Insert(0, colony);

                break;
            }
        }
    }
}