using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using PersistentWorlds.Logic;
using PersistentWorlds.World;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class MainTabWindow_Colonies : MainTabWindow
    {
        public static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");
        // TODO: Draw.

        private Vector2 scrollPosition = Vector2.zero;
        private List<ScrollableListItem> items;
        
        public MainTabWindow_Colonies()
        {
            this.forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            /*
            base.DoWindowContents(inRect);
            
            GUI.BeginGroup(inRect);

            var boxRect = new Rect(0, 0, 100, 100);
            Widgets.DrawBox(boxRect, 1);
            
            GUI.BeginGroup(boxRect);

            var townTexRect = new Rect(boxRect.x + 10, boxRect.y, 80, 80);
            GUI.DrawTexture(townTexRect, Town);
            
            GUI.EndGroup();
            
            GUI.EndGroup();
            */
            
            ScrollableListUI.DrawList(ref inRect, ref scrollPosition, ref this.items);
        }

        public override void PreOpen()
        {
            if (PersistentWorldManager.PersistentWorld == null)
            {
                this.Close();
                return;
            }
            
            base.PreOpen();
            
            this.ConvertColoniesToItems();
        }

        private void ConvertColoniesToItems()
        {
            this.items = new List<ScrollableListItem>();

            foreach (var colony in PersistentWorldManager.PersistentWorld.Colonies)
            {
                var item = new ScrollableListItem();

                item.Text = colony.ColonyData.ColonyFaction.Name;

                if (colony != PersistentWorldManager.PersistentWorld.Colony)
                {
                    item.ActionButtonText = "Switch To";
                    item.ActionButtonAction = delegate
                    {                         
                        this.Close();

                        LongEventHandler.QueueLongEvent(delegate
                        {       
                                PersistentWorldManager.PersistentWorld.Colony = colony;
                                PersistentWorldManager.PersistentWorld.PatchPlayerFaction();

                                LoadMaps(colony);
                                Current.Game.CurrentMap = Current.Game.FindMap(PersistentWorldManager.PersistentWorld.Maps[colony][0]);
                                UnloadMaps(colony);    
                            
                                PersistentWorldManager.PersistentWorld.ConvertCurrentGameSettlements(PersistentWorldManager.PersistentWorld.Game);
                                PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();

                                Find.CameraDriver.SetRootPosAndSize(colony.ColonyData.GameData.camRootPos, colony.ColonyData.GameData.desiredSize);
                            }, "LoadingColony", false, null);
                    };
                }
                
                this.items.Add(item);
            }
        }

        private void LoadMaps(PersistentColony colony)
        {
            /*
            foreach (var map in PersistentWorldManager.PersistentWorld.Maps[colony])
            {
                Current.Game.Maps.Add(map);
                map.mapDrawer.RegenerateEverythingNow();
                map.FinalizeLoading();
                map.Parent.FinalizeLoading();
            }
            */

            var maps = PersistentWorldManager.WorldLoadSaver.LoadMaps(colony.ColonyData.ActiveWorldTiles.ToArray());

            foreach (var map in maps)
            {
                Current.Game.Maps.Add(map);
                map.mapDrawer.RegenerateEverythingNow();
                map.FinalizeLoading();
                map.Parent.FinalizeLoading();

                if (PersistentWorldManager.PersistentWorld.Maps.ContainsKey(colony))
                    PersistentWorldManager.PersistentWorld.Maps[colony].Add(map.Tile);
                else
                {
                    PersistentWorldManager.PersistentWorld.Maps.Add(colony, new List<int>() {map.Tile});
                }
            }
        }

        private void UnloadMaps(PersistentColony colony)
        {
            // Concurrency...
            var toRemove = new List<Map>();
            
            foreach (var map in Current.Game.Maps)
            {
                if (PersistentWorldManager.PersistentWorld.Maps[colony].Contains(map.Tile)) continue;
                
                // Remove maps.
                toRemove.Add(map);
            }
            
            toRemove.Do(map => Current.Game.DeinitAndRemoveMap(map));
            toRemove.Clear();
            
            Find.ColonistBar.MarkColonistsDirty();
        }
    }
}