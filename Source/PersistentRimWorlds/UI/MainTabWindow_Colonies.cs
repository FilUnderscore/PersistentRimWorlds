using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using PersistentWorlds.Logic;
using PersistentWorlds.World;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public sealed class MainTabWindow_Colonies : MainTabWindow
    {
        #region Fields
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");

        private static readonly FieldInfo reservedDestinationsField =
            AccessTools.Field(typeof(PawnDestinationReservationManager), "reservedDestinations");
        
        private Vector2 scrollPosition = Vector2.zero;
        private List<ScrollableListItem> items;
        #endregion
        
        #region Properties
        public override Vector2 RequestedTabSize => new Vector2(Verse.UI.screenWidth * 0.5f, Verse.UI.screenHeight / 3.5f);
        #endregion
        
        #region Methods
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

            for (var i = 0; i < PersistentWorldManager.PersistentWorld.Colonies.Count; i++)
            {
                var colony = PersistentWorldManager.PersistentWorld.Colonies[i];

                var item = new ScrollableListItemColored {Text = colony.ColonyData.ColonyFaction.Name};

                if (colony != PersistentWorldManager.PersistentWorld.Colony)
                {
                    item.canChangeColor = true;
                    item.texture = Town;
                    
                    var index = i;
                    
                    item.ActionButtonText = "Switch To";
                    item.ActionButtonAction = delegate
                    {                         
                        this.Close();

                        LongEventHandler.QueueLongEvent(delegate
                        {
                            PersistentWorldManager.PersistentWorld.ConvertCurrentGameSettlements(PersistentWorldManager.PersistentWorld.Game);

                            PersistentWorldManager.WorldLoadSaver.LoadColony(ref colony);
                            PersistentWorldManager.PersistentWorld.Colonies[index] = colony;
                            
                            PersistentWorldManager.PersistentWorld.PatchPlayerFaction();

                            UnloadMapReferences(colony);
                            
                            LoadMaps(colony);
                            Current.Game.CurrentMap = Current.Game.FindMap(PersistentWorldManager.PersistentWorld.Maps[colony][0]);
                            UnloadMaps(colony);    
                            
                            PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();

                            Find.CameraDriver.SetRootPosAndSize(colony.GameData.camRootPos, colony.GameData.desiredSize);
                        }, "LoadingColony", false, null);
                    };
                }
                
                this.items.Add(item);
            }
        }

        private void UnloadMapReferences(PersistentColony colony)
        {
            foreach (var map in Current.Game.Maps)
            {
                if (PersistentWorldManager.PersistentWorld.Maps.ContainsKey(colony) && PersistentWorldManager.PersistentWorld.Maps[colony].Contains(map.Tile)) continue;
                
                PersistentWorldManager.ReferenceTable.ClearReferencesFor("\\Saves\\WorldHere\\Maps\\" + map.Tile + ".pwmf");
            }
        }

        private void LoadMaps(PersistentColony colony)
        {
            Current.ProgramState = ProgramState.MapInitializing;
            
            var maps = PersistentWorldManager.WorldLoadSaver.LoadMaps(colony.ColonyData.ActiveWorldTiles.ToArray());

            foreach (var map in maps)
            {
                Current.Game.Maps.Add(map);

                /*
                 * Register in case as to not cause problems.
                 */
                
                var reservedDestinations =
                    (Dictionary<Faction, PawnDestinationReservationManager.PawnDestinationSet>)
                    reservedDestinationsField.GetValue(map.pawnDestinationReservationManager);

                foreach (var faction in Find.FactionManager.AllFactions)
                {
                    if (!reservedDestinations.ContainsKey(faction))
                    {
                        map.pawnDestinationReservationManager.RegisterFaction(faction);
                    }
                }
                
                /*
                 * Regenerate map and load.
                 */
                
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
        #endregion
    }
}