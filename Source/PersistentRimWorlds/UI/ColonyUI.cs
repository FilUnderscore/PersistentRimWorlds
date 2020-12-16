using System;
using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using PersistentWorlds.Logic;
using PersistentWorlds.Utils;
using PersistentWorlds.World;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public static class ColonyUI
    {
        private static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");

        private static readonly Texture2D FavouriteStar = ContentFinder<Texture2D>.Get("UI/FavouriteStarOutline");

        private static readonly Texture2D FavouriteStarToBe =
            ContentFinder<Texture2D>.Get("UI/FavouriteStarToBeFilledIn");
        
        private static readonly Texture2D FavouredStar = ContentFinder<Texture2D>.Get("UI/FavouriteStarFilledInNew");

        private static readonly Dictionary<PersistentColony, Vector2> ScrollPositions =
            new Dictionary<PersistentColony, Vector2>();

        private static Vector2 scrollPosition;

        private static bool sorted = false;
        
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

            SortColoniesOneTime(ref colonies);
            
            UITools.DrawBoxGridView(out _, out var outRect, ref inRect, ref scrollPosition, perRow, gap,
                (i, boxRect) =>
                {
                    if (i >= colonies.Count) return false;

                    var colony = colonies[i];

                    if (colony?.ColonyData == null ||
                        (colony.ColonyData.Leader != null && colony.ColonyData.Leader.LoadingTexture)) return false;

                    Widgets.DrawAltRect(boxRect);
                
                    // Delete button.
                    var deleteSize = boxRect.width / 8;

                    var deleteRect = new Rect(boxRect.x + boxRect.width - deleteSize, boxRect.y, deleteSize,
                        deleteSize);

                    // Draw delete button first.
                    if (Widgets.ButtonImage(deleteRect, DeleteX))
                    {
                        delete(i);
                    }

                    TooltipHandler.TipRegion(deleteRect,
                        "FilUnderscore.PersistentRimWorlds.Delete.Colony.Click".Translate());

                    Widgets.DrawHighlightIfMouseover(boxRect);

                    // Draw whole box button second.
                    if (Widgets.ButtonInvisible(boxRect))
                    {
                        load(i);
                    }
                
                    var texture = GetTexture(colony.ColonyData.ColonyFaction);
                    var size = Mathf.Clamp(boxRect.width * 0.65f, 0, texture.width);
                        
                    var textureRect = new Rect(boxRect.x + margin, boxRect.y + boxRect.height / 2 - size / 2, size, size);
                    
                    DrawTexture(textureRect, texture, colony.ColonyData.Color);
    
                    DrawLeader(boxRect, colony, margin, 0.56f);

                    const float nameMargin = 4f;
    
                    var colonyNameRect = new Rect(boxRect.x + nameMargin, boxRect.y + nameMargin,
                        boxRect.width - nameMargin - deleteSize,
                        textureRect.y - boxRect.y);
    
                    DrawNameLabel(colonyNameRect, colony, colony.ColonyData.ColonyFaction);
                
                    return true;
                }, colonies.Count + 1, (width, height) =>
                {
                    /*
                     * New Colony Button
                     */       
                    var y = width * Mathf.Floor((float) colonies.Count / perRow) +
                            (colonies.Count / perRow) * gap;

                    var boxRect = new Rect((width * (colonies.Count % perRow)) + (colonies.Count % perRow) * gap,
                        y, width,
                        width);

                    Widgets.DrawHighlightIfMouseover(boxRect);
                    Widgets.DrawAltRect(boxRect);

                    if (Widgets.ButtonInvisible(boxRect))
                    {
                        newColony();
                    }

                    TooltipHandler.TipRegion(boxRect,
                        "FilUnderscore.PersistentRimWorlds.Create.NewColony".Translate());

                    Widgets.DrawLine(new Vector2(boxRect.x + boxRect.width / 2, boxRect.y + boxRect.height / 3),
                        new Vector2(boxRect.x + boxRect.width / 2, boxRect.y + boxRect.height * 0.66f), Color.white,
                        1f);

                    Widgets.DrawLine(new Vector2(boxRect.x + boxRect.width / 3, boxRect.y + boxRect.height / 2),
                        new Vector2(boxRect.x + boxRect.width * 0.66f, boxRect.y + boxRect.height / 2), Color.white,
                        1f);
                }, closeButtonSize);

            outRect.height -= closeButtonSize.y + margin;
        }
        
        
        /// <summary>
        /// Draw in-game colonies tab.
        /// </summary>
        /// <param name="inRect">The rect that is being modified - the rect that holds the colonies UI elements.</param>
        /// <param name="margin">The margin spacing/gap between each colony UI element.</param>
        /// <param name="colonies">List of colonies to be displayed.</param>
        /// <param name="load">Action to run on click (colony load/switch).</param>
        /// <param name="tabSize">The size of the tab when opened depending on screen resolution.</param>
        public static void DrawColoniesTab(ref Rect inRect, float margin,
            List<PersistentColony> colonies, Action<int> load, Vector2 tabSize)
        {
            var gap = (int) margin;
            
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            SortColoniesOneTime(ref colonies);
            SortColonies(ref colonies);

            // Scale the number of rows depending on the current resolution.
            int perRow = ((int) tabSize.x) / 160; // Reference is 6 per row @ 1920 resolution width.

            UITools.DrawBoxGridView(out _, out _, ref inRect, ref scrollPosition, perRow, gap,
                (i, origBoxRect) =>
                {
                    var boxRect = new Rect(origBoxRect.x, origBoxRect.y, origBoxRect.width * 0.8f, origBoxRect.height);
                    var addRect = new Rect(boxRect.x + boxRect.width, boxRect.y, origBoxRect.width * 0.2f,
                        origBoxRect.height);
                    
                   var colony = colonies[i];
             
                    Faction faction;

                    // The top-left most colony is the current colony the player is playing as.                    
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
                    
                    var texture = GetTexture(faction);
                    var size = Mathf.Clamp(boxRect.width * 0.65f, 0, texture.width);
                    
                    var textureRect = new Rect(boxRect.x, boxRect.y, size, size);
    
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
                        DrawTexture(textureRect, texture, colony.ColonyData.Color);
                        
                        Widgets.DrawHighlightIfMouseover(boxRect);
    
                        if (Widgets.ButtonInvisible(boxRect))
                        {
                            load(i);
                        }
                    }

                    GUI.color = Color.white;

                    DrawDynamicLeader(boxRect, out var leaderRect, colony, persistentWorld, 0.485f);
    
                    TooltipHandler.TipRegion(textureRect,
                        Equals(colony, persistentWorld.Colony)
                            ? "FilUnderscore.PersistentRimWorlds.Colony.ChangeColor".Translate()
                            : "FilUnderscore.PersistentRimWorlds.Colony.SwitchTo".Translate());
                    
                    var colonyNameRect = new Rect(boxRect.x + 4f, textureRect.yMax, boxRect.width - leaderRect.width,
                        boxRect.yMax - textureRect.yMax);
                    
                    DrawNameLabel(colonyNameRect, colony, faction);

                    // Drawing additional stuff
                    //TODO: Draw interactable favourites star in top-right.
                    DrawFavouriteStar(addRect, colony);

                    return true;
                }, colonies.Count);
        }

        public static void Reset()
        {
            scrollPosition = new Vector2();
            ScrollPositions.Clear();

            sorted = false;
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

        private static void SortColoniesOneTime(ref List<PersistentColony> colonies)
        {
            if (sorted)
                return;
            
            //colonies.Sort((x, y) => y.FileInfo.LastWriteTime.CompareTo(x.FileInfo.LastWriteTime));
            colonies.Sort((x, y) => x.ColonyData.UniqueId.CompareTo(y.ColonyData.UniqueId));
            
            sorted = true;
        }

        private static Texture2D GetTexture(Faction faction)
        {
            return faction.def.FactionIcon;
        }

        private static void DrawNameLabel(Rect rect, PersistentColony colony, Faction faction)
        {
            if(!ScrollPositions.ContainsKey(colony))
                ScrollPositions.Add(colony, new Vector2());

            var labelScrollPosition = ScrollPositions[colony];

            Text.Font = GameFont.Tiny;

            WidgetExtensions.LabelScrollable(rect, faction.Name, ref labelScrollPosition, false,
                true, false);
            
            Text.Font = GameFont.Small;

            ScrollPositions[colony] = labelScrollPosition;
        }
        
        private static void DrawLeader(Rect rect, PersistentColony colony, float margin, float widthMultiplier)
        {
            Rect leaderRect;
            
            if ((object) colony.ColonyData.Leader?.Texture != null)
            {
                var leaderPortrait = colony.ColonyData.Leader.Texture;

                leaderRect = new Rect(rect.x + rect.width * widthMultiplier,
                    rect.y + rect.height / 2 - leaderPortrait.height / 2f, leaderPortrait.width, leaderPortrait.height);
                
                GUI.DrawTexture(leaderRect, leaderPortrait);

                TooltipHandler.TipRegion(leaderRect,
                    "FilUnderscore.PersistentRimWorlds.Colony.ColonyLeader".Translate(colony.ColonyData.Leader.Name
                        .ToStringFull));
            }
            else
            {
                leaderRect = new Rect(rect.x + rect.width / 2 + margin, rect.y + margin,
                    rect.width - rect.width * widthMultiplier - margin, rect.height);

                Text.Font = GameFont.Tiny;
                
                Widgets.Label(leaderRect, "FilUnderscore.PersistentRimWorlds.Colony.NoLeader".Translate());
            }
        }

        private static bool CanChangeLeader(PersistentColony colony, PersistentWorld world)
        {
            return Equals(colony, world.Colony);
        }

        private static void DrawDynamicLeader(Rect rect, out Rect leaderRect, PersistentColony colony, PersistentWorld world,
            float widthMultiplier)
        {
            var portraitSize = new Vector2(rect.width / 2, rect.height);
            leaderRect = new Rect(rect.x + rect.width * widthMultiplier, rect.y, portraitSize.x, portraitSize.y);;

            if (colony.ColonyData.Leader != null && colony.ColonyData.Leader.Set &&
                (object) colony.ColonyData.Leader.Texture != null ||
                colony.ColonyData.Leader?.Reference != null)
            {
                if (colony.ColonyData.Leader.Reference != null)
                {
                    colony.ColonyData.Leader.Texture =
                        PortraitsCache.Get(colony.ColonyData.Leader.Reference, portraitSize, new Vector3(), 1f, true, true);
                }

                var leaderPortrait = colony.ColonyData.Leader.Texture;

                var canChangeLeader = CanChangeLeader(colony, world);

                if (canChangeLeader)
                {
                    if (WidgetExtensions.ButtonImage(leaderRect, leaderPortrait, Color.white, GenUI.MouseoverColor))
                    {
                        Find.WindowStack.Add(new Dialog_PersistentWorlds_LeaderPawnSelection(colony));
                    }
                }
                else
                {
                    GUI.DrawTexture(leaderRect, leaderPortrait);
                }

                TooltipHandler.TipRegion(leaderRect,
                    canChangeLeader
                        ? "FilUnderscore.PersistentRimWorlds.Colony.ChangeLeader".Translate()
                        : "FilUnderscore.PersistentRimWorlds.Colony.ColonyLeader".Translate(colony.ColonyData.Leader
                            .Name.ToStringFull));
            }
            else
            {
                Text.Font = GameFont.Tiny;
                
                Widgets.Label(leaderRect, "FilUnderscore.PersistentRimWorlds.Colony.NoLeader".Translate());

                Text.Font = GameFont.Small;
            }
        }

        private static void DrawTexture(Rect rect, Texture texture, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            
            GUI.DrawTexture(rect, texture);

            GUI.color = previousColor;
        }

        private static bool ButtonTextureHover(Rect rect, Texture texture, Texture hoverTexture, Color color, Color mouseoverColor, Color onColor, bool on, bool doMouseoverSound = true)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, texture);
            GUI.color = Color.white;

            if (on && !Mouse.IsOver(rect))
            {
                GUI.color = onColor;
                GUI.DrawTexture(rect, hoverTexture);
                GUI.color = Color.white;
            }
            else if (Mouse.IsOver(rect))
            {
                GUI.color = mouseoverColor;
                GUI.DrawTexture(rect, hoverTexture);
                GUI.color = Color.white;
            }
            
            if(doMouseoverSound)
                MouseoverSounds.DoRegion(rect);

            return Widgets.ButtonInvisible(rect, false);
        }

        private static void DrawFavouriteStar(Rect rect, PersistentColony colony)
        {
            var favoured = colony.ColonyData.Favoured;
            
            var size = rect.size.x;
            var starRect = new Rect(rect.x, rect.y, size, size);
            
            Widgets.DrawHighlight(starRect);
            
            if (ButtonTextureHover(starRect, FavouriteStar, FavouriteStarToBe,
                Color.gray, favoured ? Color.red : Color.green, GenUI.MouseoverColor, favoured))
            {
                colony.ColonyData.Favoured = !favoured;
            }

            TooltipHandler.TipRegion(starRect, !favoured ? "FilUnderscore.PersistentRimWorlds.Colony.Favourite.Add".Translate() : 
                "FilUnderscore.PersistentRimWorlds.Colony.Favourite.Remove".Translate());
        }

        public static void ShowDeleteColonyDialog(PersistentColony colony, Action<PersistentColony> onDelete, Action<PersistentColony> onConvert)
        {
            var dialogBox = new Dialog_MessageBox("FilUnderscore.PersistentRimWorlds.Delete.Colony.Desc".Translate(colony.ColonyData.ColonyFaction.Name), "Delete".Translate(), () => onDelete(colony), "FilUnderscore.PersistentRimWorlds.Cancel".Translate(), null, "FilUnderscore.PersistentRimWorlds.Delete.Colony".Translate(), true);

            if (onConvert != null)
            {
                dialogBox.buttonCText = "FilUnderscore.PersistentRimWorlds.Delete.Colony.Convert".Translate();

                dialogBox.buttonCAction = () => onConvert(colony);
            }
            
            Find.WindowStack.Add(dialogBox);
        }
    }
}