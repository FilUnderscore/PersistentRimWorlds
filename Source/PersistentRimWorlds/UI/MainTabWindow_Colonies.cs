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

                                PersistentWorldManager.PersistentWorld.ConvertCurrentGameSettlements(PersistentWorldManager.PersistentWorld.Game);
                                
                                LoadMaps(colony);
                                Current.Game.CurrentMap = Current.Game.FindMap(PersistentWorldManager.PersistentWorld.Maps[colony][0]);
                                UnloadMaps(colony);    
                            
                                PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();

                                //Find.CameraDriver.SetRootPosAndSize(colony.ColonyData.GameData.camRootPos, colony.ColonyData.GameData.desiredSize);
                            }, "LoadingColony", false, null);
                    };
                }
                
                this.items.Add(item);
            }
        }

        private void LoadMaps(PersistentColony colony)
        {
            Current.ProgramState = ProgramState.MapInitializing;
            
            var maps = PersistentWorldManager.WorldLoadSaver.LoadMaps(colony.ColonyData.ActiveWorldTiles.ToArray());

            foreach (var map in maps)
            {
                Current.Game.Maps.Add(map);
                
                foreach(var faction in Find.FactionManager.AllFactions)
                    map.pawnDestinationReservationManager.RegisterFaction(faction);
                
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

            Current.ProgramState = ProgramState.Playing;
        }

        private void UnloadMaps(PersistentColony colony)
        {
            // TODO: Save map first.
            
            // Concurrency...
            var toRemove = new List<Map>();
            
            foreach (var map in Current.Game.Maps)
            {
                if (PersistentWorldManager.PersistentWorld.Maps[colony].Contains(map.Tile)) continue;
                
                // Remove maps.
                toRemove.Add(map);
            }
            
            toRemove.Do(map => Current.Game.DeinitAndRemoveMap(map));
            
            Log.Message("Removed. " + toRemove.Count);
            toRemove.Clear();
            
            
            Find.ColonistBar.MarkColonistsDirty();
        }
    }
}