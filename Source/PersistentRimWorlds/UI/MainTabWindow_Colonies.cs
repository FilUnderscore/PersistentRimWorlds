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
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
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

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            for (var i = 0; i < persistentWorld.Colonies.Count; i++)
            {
                var colony = persistentWorld.Colonies[i];

                var item = new ScrollableListItemColored {Text = colony.ColonyData.ColonyFaction.Name};

                if (colony != persistentWorld.Colony)
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
                            persistentWorld.ConvertCurrentGameSettlements();

                            persistentWorld.LoadSaver.LoadColony(ref colony);
                            persistentWorld.Colonies[index] = colony;
                            
                            persistentWorld.PatchPlayerFaction();

                            UnloadMapReferences(colony);
                            
                            LoadMaps(colony);
                            Current.Game.CurrentMap = Current.Game.FindMap(persistentWorld.Maps[colony][0]);
                            UnloadMaps(colony);    
                            
                            persistentWorld.ConvertToCurrentGameSettlements();

                            Find.CameraDriver.SetRootPosAndSize(colony.GameData.camRootPos, colony.GameData.desiredSize);
                        }, "LoadingColony", false, null);
                    };
                }
                
                this.items.Add(item);
            }
        }

        private void UnloadMapReferences(PersistentColony colony)
        {
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            foreach (var map in Current.Game.Maps)
            {
                if (persistentWorld.Maps.ContainsKey(colony) && persistentWorld.Maps[colony].Contains(map.Tile)) continue;
                
                persistentWorld.LoadSaver.ReferenceTable.ClearReferencesFor("\\Saves\\WorldHere\\Maps\\" + map.Tile + ".pwmf");
            }
        }

        private void LoadMaps(PersistentColony colony)
        {
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            Current.ProgramState = ProgramState.MapInitializing;
            
            var maps = persistentWorld.LoadSaver.LoadMaps(colony.ColonyData.ActiveWorldTiles.ToArray());

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

                if (persistentWorld.Maps.ContainsKey(colony))
                    persistentWorld.Maps[colony].Add(map.Tile);
                else
                {
                    persistentWorld.Maps.Add(colony, new List<int>() {map.Tile});
                }
            }

            Current.ProgramState = ProgramState.Playing;
        }

        private void UnloadMaps(PersistentColony colony)
        {
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            // TODO: Save map first.
            
            // Concurrency...
            var toRemove = new List<Map>();
            
            foreach (var map in Current.Game.Maps)
            {
                if (persistentWorld.Maps[colony].Contains(map.Tile)) continue;
                
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