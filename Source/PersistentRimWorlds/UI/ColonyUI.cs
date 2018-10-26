using System;
using System.Collections.Generic;
using ColourPicker;
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

        // TODO: Scale depending on screen size?
        private static readonly Dictionary<PersistentColony, Vector2> ScrollPositions =
            new Dictionary<PersistentColony, Vector2>();

        private static Vector2 scrollPosition;

        /// <summary>
        /// Draw colonies list on Persistent RimWorlds.
        /// </summary>
        /// <param name="inRect"></param>
        /// <param name="colonies"></param>
        public static void DrawColoniesList(ref Rect inRect, float margin,
            List<PersistentColony> colonies, Action<int> load)
        {
            const int perRow = 3;
            var gap = (int) margin;

            inRect.width += gap;
            
            var colonyBoxWidth = (inRect.width - gap * perRow) / perRow;
            
            var viewRect = new Rect(0, 0, inRect.width - gap, Mathf.Ceil((float) colonies.Count / perRow) * colonyBoxWidth + (colonies.Count / perRow) * gap);
            var outRect = new Rect(inRect.AtZero());
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (var i = 0; i < colonies.Count; i++)
            {
                var y = colonyBoxWidth * Mathf.Floor((float) i / perRow) + (i / perRow) * gap;
                
                var colony = colonies[i];
             
                var boxRect = new Rect((colonyBoxWidth * (i % perRow)) + (i % perRow) * gap, y, colonyBoxWidth,
                    colonyBoxWidth);
                
                Widgets.DrawHighlightIfMouseover(boxRect);
                
                Widgets.DrawAltRect(boxRect);
                
                if (Widgets.ButtonInvisible(boxRect))
                {
                    load(i);
                }
            }
            
            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }

        /// <summary>
        /// Draw in-game colonies tab.
        /// </summary>
        /// <param name="inRect"></param>
        /// <param name="colonies"></param>
        /// <param name="load"></param>
        public static void DrawColoniesTab(ref Rect inRect, float margin,
            List<PersistentColony> colonies, Action<int> load)
        {
            const int perRow = 6;
            var gap = (int) margin;
            
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
         
            var colonyBoxWidth = (inRect.width - gap * perRow) / perRow;
            
            var viewRect = new Rect(0, 0, inRect.width - gap, Mathf.Ceil((float) colonies.Count / perRow) * colonyBoxWidth + (colonies.Count / perRow) * gap);
            var outRect = new Rect(inRect.AtZero());
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            SortColonies(ref colonies);
            
            for (var i = 0; i < colonies.Count; i++)
            {
                var y = colonyBoxWidth * Mathf.Floor((float) i / perRow) + (i / perRow) * gap;
                
                var colony = colonies[i];
             
                var boxRect = new Rect((colonyBoxWidth * (i % perRow)) + (i % perRow) * gap, y, colonyBoxWidth,
                    colonyBoxWidth);

                Faction faction;
                
                if (Equals(colony, persistentWorld.Colony))
                {
                    Widgets.DrawHighlight(boxRect);
                    faction = Find.FactionManager.OfPlayer;
                }
                else
                {
                    Widgets.DrawAltRect(boxRect);
                    faction = colony.ColonyData.ColonyFaction;
                }
                
                var size = boxRect.width * 0.65f;

                if (size >= Town.width)
                    size = Town.width;
                
                var textureRect = new Rect(boxRect.x, boxRect.y, size, size);

                Rect leaderRect;
                
                if (colony.ColonyData.Leader != null)
                {
                    var portraitSize = new Vector2(boxRect.width / 4, boxRect.height);

                    // Always get a new portrait, if things such as screen size changes or outfit.
                    if (colony.ColonyData.Leader.Reference != null)
                    {
                        colony.ColonyData.Leader.Texture =
                            PortraitsCache.Get(colony.ColonyData.Leader.Reference, portraitSize);
                    }
                    
                    var leaderPortrait = colony.ColonyData.Leader.Texture;
                    
                    leaderRect = new Rect(boxRect.x + boxRect.width * 0.68f, boxRect.y, leaderPortrait.width,
                        leaderPortrait.height);
                    
                    if (Equals(colony, persistentWorld.Colony))
                    {
                        if (WidgetExtensions.ButtonImage(leaderRect, leaderPortrait, Color.white, GenUI.MouseoverColor))
                        {
                            // TODO: Open leader selection dialog if no leader altering mods such as Relations Tab or Psychology is found.
                            Find.WindowStack.Add(new Dialog_PersistentWorlds_LeaderPawnSelection(colony));
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(leaderRect, leaderPortrait);
                    }

                    TooltipHandler.TipRegion(leaderRect,
                        Equals(colony, persistentWorld.Colony)
                            ? "FilUnderscore.PersistentRimWorlds.Colony.ClickToChangeLeader".Translate()
                            : "FilUnderscore.PersistentRimWorlds.Colony.ColonyLeader".Translate(colony.ColonyData.Leader.Name.ToStringFull));
                }
                else
                {
                    leaderRect = new Rect(boxRect.x + boxRect.width * 0.68f, boxRect.y,
                        boxRect.width - boxRect.width * 0.68f,
                        boxRect.height);

                    Text.Font = GameFont.Tiny;
                    Widgets.Label(leaderRect, "FilUnderscore.PersistentRimWorlds.Colony.NoLeader".Translate());
                }

                if (Equals(colony, persistentWorld.Colony))
                {
                    if (Widgets.ButtonImage(textureRect, Town, colony.ColonyData.Color))
                    {
                        var callback = new Action<Color>(delegate(Color color) { colony.ColonyData.Color = color; });
                        
                        Find.WindowStack.Add(new Dialog_ColourPicker(colony.ColonyData.Color, callback));
                    }
                }
                else
                {
                    GUI.color = colony.ColonyData.Color;
                    GUI.DrawTexture(textureRect, Town);
                    GUI.color = Color.white;
                    
                    Widgets.DrawHighlightIfMouseover(boxRect);

                    if (Widgets.ButtonInvisible(boxRect))
                    {
                        load(i);
                    }
                }

                GUI.color = Color.white;

                TooltipHandler.TipRegion(textureRect,
                    Equals(colony, persistentWorld.Colony)
                        ? "FilUnderscore.PersistentRimWorlds.Colony.ClickToChangeColor".Translate()
                        : "FilUnderscore.PersistentRimWorlds.Colony.ClickToSwitchTo".Translate());
                
                var colonyNameRect = new Rect(boxRect.x + 4f, textureRect.yMax, boxRect.width - leaderRect.width,
                    boxRect.height - textureRect.yMax);

                Text.Font = GameFont.Small;

                if (!ScrollPositions.ContainsKey(colony))
                {
                    ScrollPositions.Add(colony, new Vector2());
                }

                var colonyScrollPosition = ScrollPositions[colony];
                
                Widgets.LabelScrollable(colonyNameRect, faction.Name, ref colonyScrollPosition);

                ScrollPositions[colony] = colonyScrollPosition;
            }
            
            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }

        public static void Reset()
        {
            scrollPosition = new Vector2();
            ScrollPositions.Clear();
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