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
        private static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");
        
        private static readonly Dictionary<PersistentColony, Vector2> ScrollPositions =
            new Dictionary<PersistentColony, Vector2>();

        private static Vector2 scrollPosition;
        
        /// <summary>
        /// Draw colonies list on Persistent RimWorlds.
        /// </summary>
        /// <param name="inRect"></param>
        /// <param name="margin"></param>
        /// <param name="colonies"></param>
        /// <param name="load"></param>
        public static void DrawColoniesList(ref Rect inRect, float margin, Vector2 closeButtonSize,
            List<PersistentColony> colonies, Action<int> load, Action newColony, Action<int> delete)
        {
            const int perRow = 3;
            var gap = (int) margin;

            inRect.width += gap;
            
            var colonyBoxWidth = (inRect.width - gap * perRow) / perRow;
            
            var viewRect = new Rect(0, 0, inRect.width - gap, (Mathf.Ceil((float) colonies.Count + 1 / perRow)) * colonyBoxWidth + (colonies.Count / perRow) * gap);
            var outRect = new Rect(inRect.AtZero());
            outRect.height -= closeButtonSize.y + margin;
            
            GUI.BeginGroup(inRect);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            var i = 0;
            
            for (var j = 0; j < colonies.Count; j++)
            {
                var colony = colonies[j];
                
                if (colony?.ColonyData == null || (colony.ColonyData.Leader != null && colony.ColonyData.Leader.LoadingTexture)) continue;

                var y = colonyBoxWidth * Mathf.Floor((float) i / perRow) + (i / perRow) * gap;
                
                var boxRect = new Rect((colonyBoxWidth * (i % perRow)) + (i % perRow) * gap, y, colonyBoxWidth,
                    colonyBoxWidth);
                
                Widgets.DrawAltRect(boxRect);
                
                // Delete button.
                var deleteSize = boxRect.width / 8;
                var deleteRect = new Rect(boxRect.x + boxRect.width - deleteSize, boxRect.y, deleteSize, deleteSize);

                // Draw delete button first.
                if (Widgets.ButtonImage(deleteRect, DeleteX))
                {
                    delete(i);
                }
                
                TooltipHandler.TipRegion(deleteRect, "FilUnderscore.PersistentRimWorlds.DeleteColony".Translate());

                Widgets.DrawHighlightIfMouseover(boxRect);

                // Draw whole box button second.
                if (Widgets.ButtonInvisible(boxRect))
                {
                    load(j);
                }
                
                var size = boxRect.width * 0.65f;
                var texture = GetTexture(colony.ColonyData.ColonyFaction);
                
                if (size >= texture.width)
                    size = texture.width;
                
                var textureRect = new Rect(boxRect.x + margin, boxRect.y + boxRect.height / 2 - size / 2, size, size);

                Rect leaderRect;
                
                if ((object) colony.ColonyData.Leader?.Texture != null)
                {
                    var leaderPortrait = colony.ColonyData.Leader.Texture;

                    leaderRect = new Rect(boxRect.x + boxRect.width * 0.56f, boxRect.y + boxRect.height / 2 - leaderPortrait.height / 2f, leaderPortrait.width,
                        leaderPortrait.height);
                    
                    GUI.DrawTexture(leaderRect, leaderPortrait);

                    TooltipHandler.TipRegion(leaderRect,
                        "FilUnderscore.PersistentRimWorlds.Colony.ColonyLeader".Translate(colony.ColonyData.Leader.Name
                            .ToStringFull));
                }
                else
                {
                    leaderRect = new Rect(boxRect.x + boxRect.width / 2 + margin, boxRect.y + margin,
                        boxRect.width - boxRect.width * 0.68f,
                        boxRect.height);

                    Text.Font = GameFont.Tiny;
                    Widgets.Label(leaderRect, "FilUnderscore.PersistentRimWorlds.Colony.NoLeader".Translate());
                }

                GUI.color = colony.ColonyData.Color;
                GUI.DrawTexture(textureRect, texture);
                GUI.color = Color.white;

                const float nameMargin = 4f;

                var colonyNameRect = new Rect(boxRect.x + nameMargin, boxRect.y + nameMargin,
                    boxRect.width - nameMargin - deleteSize,
                    textureRect.y - boxRect.y);

                Text.Font = GameFont.Small;

                if (!ScrollPositions.ContainsKey(colony))
                {
                    ScrollPositions.Add(colony, new Vector2());
                }

                var colonyScrollPosition = ScrollPositions[colony];

                Text.Font = GameFont.Tiny;

                WidgetExtensions.LabelScrollable(colonyNameRect, colony.ColonyData.ColonyFaction.Name,
                    ref colonyScrollPosition, false, true, false);
                Text.Font = GameFont.Small;
                
                ScrollPositions[colony] = colonyScrollPosition;
                
                i++;
            }

            /*
             * New Colony Button
             */

            {
                var y = colonyBoxWidth * Mathf.Floor((float) colonies.Count / perRow) +
                         (colonies.Count / perRow) * gap;

                var boxRect = new Rect((colonyBoxWidth * (colonies.Count % perRow)) + (colonies.Count % perRow) * gap,
                    y, colonyBoxWidth,
                    colonyBoxWidth);

                Widgets.DrawHighlightIfMouseover(boxRect);
                Widgets.DrawAltRect(boxRect);

                if (Widgets.ButtonInvisible(boxRect))
                {
                    newColony();
                }

                TooltipHandler.TipRegion(boxRect, "FilUnderscore.PersistentRimWorlds.CreateANewColony".Translate());

                Widgets.DrawLine(new Vector2(boxRect.x + boxRect.width / 2, boxRect.y + boxRect.height / 3),
                    new Vector2(boxRect.x + boxRect.width / 2, boxRect.y + boxRect.height * 0.66f), Color.white,
                    1f);

                Widgets.DrawLine(new Vector2(boxRect.x + boxRect.width / 3, boxRect.y + boxRect.height / 2),
                    new Vector2(boxRect.x + boxRect.width * 0.66f, boxRect.y + boxRect.height / 2), Color.white,
                    1f);
            }

            Widgets.EndScrollView();
            
            GUI.EndGroup();
        }

        /// <summary>
        /// Draw in-game colonies tab.
        /// </summary>
        /// <param name="inRect"></param>
        /// <param name="margin"></param>
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
                var texture = GetTexture(faction);
                
                if (size >= texture.width)
                    size = texture.width;
                
                var textureRect = new Rect(boxRect.x, boxRect.y, size, size);

                Rect leaderRect;
                
                if (colony.ColonyData.Leader != null && colony.ColonyData.Leader.Set && (object) colony.ColonyData.Leader.Texture != null || colony.ColonyData.Leader?.Reference != null)
                {
                    var portraitSize = new Vector2(boxRect.width / 2, boxRect.height);

                    // Always get a new portrait, if things such as screen size changes or outfit.
                    if (colony.ColonyData.Leader.Reference != null)
                    {
                        colony.ColonyData.Leader.Texture =
                            PortraitsCache.Get(colony.ColonyData.Leader.Reference, portraitSize);
                    }
                    
                    var leaderPortrait = colony.ColonyData.Leader.Texture;
                    
                    leaderRect = new Rect(boxRect.x + boxRect.width * 0.485f, boxRect.y, portraitSize.x,
                        portraitSize.y);
                    
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
                    if (Widgets.ButtonImage(textureRect, texture, colony.ColonyData.Color))
                    {
                        var callback = new Action<Color>(delegate(Color color) { colony.ColonyData.Color = color; });
                        
                        Find.WindowStack.Add(new Dialog_ColourPicker(colony.ColonyData.Color, callback));
                    }
                }
                else
                {
                    GUI.color = colony.ColonyData.Color;
                    GUI.DrawTexture(textureRect, texture);
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
                    boxRect.yMax - textureRect.yMax);

                Text.Font = GameFont.Small;

                if (!ScrollPositions.ContainsKey(colony))
                {
                    ScrollPositions.Add(colony, new Vector2());
                }

                var colonyScrollPosition = ScrollPositions[colony];

                Text.Font = GameFont.Tiny;

                WidgetExtensions.LabelScrollable(colonyNameRect, faction.Name, ref colonyScrollPosition, false, true,
                    false);
                Text.Font = GameFont.Small;
                
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

        private static Texture2D GetTexture(Faction faction)
        {
            return faction.def.ExpandingIconTexture;
        }
    }
}