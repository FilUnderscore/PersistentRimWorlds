using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using Verse;
using PersistentWorlds.Utils;

namespace PersistentWorlds.Logic
{
    public class PersistentWorld : IDisposable
    {
        #region Fields
        public PersistentWorldLoadSaver LoadSaver;
        
        // Game.World is accessed.
        public Game Game = new Game();

        public PersistentWorldData WorldData = new PersistentWorldData();
        public PersistentColony Colony;
        
        // Stores map tile ids.
        public readonly Dictionary<int, HashSet<PersistentColony>> LoadedMaps = new Dictionary<int, HashSet<PersistentColony>>();
        public readonly List<PersistentColony> Colonies = new List<PersistentColony>();
        #endregion
        
        #region Constructors
        public PersistentWorld()
        {
            Current.Game = this.Game;
            this.Game.World = new RimWorld.Planet.World();
        }
        #endregion

        #region Methods
        public void LoadWorld()
        {
            Current.Game = this.Game;
            
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.LoadingWorld".Translate());
            
            this.Game.LoadGame();
            
            // At the end.. because Scribe doesn't run due to us not loading Game directly.
            this.Game.FinalizeInit();
            
            this.LoadCameraDriver();
            
            GameComponentUtility.LoadedGame();
            
            this.CheckAndSetColonyData();
        }
        
        // Called from Patched Game.LoadGame().
        public void LoadGame()
        {
            /*
             * Fill Game components.
             */
            
            if (Colony == null)
            {
                // Return to main menu.
                Log.Error("Colony is null. - Persistent Worlds");
                GenScene.GoToMainMenu();
                return;
            }
            
            Colony.GameData.SetGame();

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            AccessTools.Method(typeof(Game), "FillComponents", new Type[0]).Invoke(this.Game, new object[0]);
            BackCompatibility.GameLoadingVars(this.Game);

            /*
             * Load world and maps.
             */
            
            this.LoadGameWorldAndMaps();
        }

        private void LoadGameWorldAndMaps()
        {
            this.ExposeGameWorldData();
            
            this.Game.World.FinalizeInit();

            this.LoadMaps();
        }

        private void LoadMaps()
        {
            var maps = this.LoadSaver.LoadMaps(this.Colony.ColonyData.ActiveWorldTiles.ToArray());
            maps.Do(Current.Game.AddMap);
            
            this.ConvertToCurrentGameSettlements();
            
            if (this.Game.Maps.RemoveAll((Map x) => x == null) != 0)
            {
                Log.Warning("Some maps were null after loading.", false);
            }

            int num = -1;

            num = Colony.GameData.currentMapIndex;
            if (num < 0 && this.Game.Maps.Any<Map>())
            {
                Log.Error("PersistentWorlds - Current map is null after loading but there are maps available. Setting current map to [0].", false);
                num = 0;
            }

            if (num >= this.Game.Maps.Count)
            {
                Log.Error("Current map index out of bounds after loading.", false);
                if (this.Game.Maps.Any<Map>())
                {
                    num = 0;
                }
                else
                {
                    num = -1;
                }
            }
            
            Game.CurrentMap = ((num < 0) ? null : this.Game.Maps[num]);

            foreach (var t in this.Game.Maps)
            {
                try
                {
                    t.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in Map.FinalizeLoading(): " + e, false);
                }

                try
                {
                    t.Parent.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in MapParent.FinalizeLoading(): " + e, false);
                }

                this.LoadedMaps.Add(t.Tile, new HashSet<PersistentColony>(){Colony});
            }
        }

        private void LoadCameraDriver()
        {
            if (Find.CameraDriver == null)
            {
                Log.Error("Current CameraDriver is null.");
                return;
            }
            
            Find.CameraDriver.SetRootPosAndSize(this.Colony.GameData.camRootPos, this.Colony.GameData.desiredSize);
        }

        private void ExposeGameWorldData()
        {
            this.Game.World.info = this.WorldData.Info;
            this.Game.World.grid = this.WorldData.Grid;

            if (this.Game.World.components == null)
            {
                Log.Error("Game World Components is null! Please look into this.");
                this.Game.World.components = new List<WorldComponent>();
            }
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (this.Game.World.grid == null || !this.Game.World.grid.HasWorldData)
                {
                    WorldGenerator.GenerateWithoutWorldData(this.Game.World.info.seedString);
                }
                else
                {
                    WorldGenerator.GenerateFromScribe(this.Game.World.info.seedString);
                }
            }
            else
            {
                this.ExposeAndFillGameWorldComponents();
            }
        }

        public void ConstructGameWorldComponentsAndExposeComponents()
        {   
            this.Game.World.ConstructComponents();

            this.ExposeAndFillGameWorldComponents();
        }

        public void ExposeAndFillGameWorldComponents()
        {
            this.Game.tickManager = this.WorldData.TickManager;
            this.Game.World.factionManager = this.WorldData.FactionManager;
            this.Game.World.worldPawns = this.WorldData.WorldPawns;
            this.Game.World.worldObjects = this.WorldData.WorldObjectsHolder;
            this.Game.World.gameConditionManager = this.WorldData.GameConditionManager;
            this.Game.World.storyState = this.WorldData.StoryState;
            this.Game.World.features = this.WorldData.WorldFeatures;
            this.Game.uniqueIDsManager = this.WorldData.UniqueIDsManager;
            this.Game.World.components = this.WorldData.WorldComponents;
            
            AccessTools.Method(typeof(RimWorld.Planet.World), "FillComponents", new Type[0]).Invoke(this.Game.World, new object[0]);

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            if (this.WorldData.UniqueIDsManager != null)
            {
                this.Game.uniqueIDsManager = this.WorldData.UniqueIDsManager;
            }
        }

        public void Convert(Game game)
        {
            Current.Game = game;

            this.Game = game;
            
            this.WorldData = PersistentWorldData.Convert(game);

            var colony = PersistentColony.Convert(game);
            this.Colony = colony;
            
            this.Colonies.Add(colony);
            
            this.ConvertCurrentGameSettlements();
        }

        // Convert Settlements to Colony Bases (this.Colony) for saving
        public void ConvertCurrentGameSettlements()
        {
            // Concurrency errors :/
            var toAdd = new List<Colony>();
            var toRemove = new List<Settlement>();
            
            foreach (var settlement in this.Game.World.worldObjects.Settlements)
            {
                if (settlement.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (settlement.Map?.info == null)
                {
                    continue;
                }
                
                var colony = (Colony) WorldObjectSameIDMaker.MakeWorldObject(PersistentWorldsDefOf.Colony, settlement.ID);
                settlement.Map.info.parent = colony;
                colony.Tile = settlement.Tile;
                colony.Name = settlement.HasName ? settlement.Name : null;

                colony.PersistentColonyData = this.LoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting ? this.Colonies[0].ColonyData : this.Colony.ColonyData;

                toAdd.Add(colony);
                toRemove.Add(settlement);
            }
            
            toAdd.Do(colony => this.Game.World.worldObjects.Add(colony));
            toAdd.Clear();
            
            toRemove.Do(settlement => this.Game.World.worldObjects.Remove(settlement));
            toRemove.Clear();
        }

        // Convert Colony Bases to Settlements (this.Colony) for loading
        public void ConvertToCurrentGameSettlements()
        {   
            var toAdd = new List<Settlement>();
            var toRemove = new List<Colony>();
            
            foreach (var mapParent in this.WorldData.WorldObjectsHolder.MapParents)
            {
                if (!(mapParent is Colony)) continue;
                
                var colony = (Colony) mapParent;

                if (this.Colony == null || colony.PersistentColonyData == null || this.Colony.ColonyData == null || colony.PersistentColonyData.uniqueID != this.Colony.ColonyData.uniqueID) continue;
                
                if (colony.Map == null)
                {
                    var map = Current.Game.FindMap(colony.Tile);

                    if (map != null)
                    {
                        map.info.parent = colony;
                    }
                    else
                    {
                        Log.Error("Null map for colony " + colony.PersistentColonyData.uniqueID + " at " + colony.Tile);
                        
                        continue;
                    }
                }
                
                var settlement =
                    (Settlement) WorldObjectSameIDMaker.MakeWorldObject(WorldObjectDefOf.Settlement, colony.ID);
                settlement.SetFaction(Faction.OfPlayer);
                colony.Map.info.parent = settlement;
                settlement.Tile = colony.Tile;
                
                settlement.Name = colony.HasName ? colony.Name : null;
                settlement.namedByPlayer = colony.HasName; // Prevents non-stop renaming.
                
                toAdd.Add(settlement);
                toRemove.Add(colony);
            }
            
            toAdd.Do(settlement => this.WorldData.WorldObjectsHolder.Add(settlement));
            toAdd.Clear();
            
            toRemove.Do(colony => this.WorldData.WorldObjectsHolder.Remove(colony));
            toRemove.Clear();
        }

        public void UpdateWorld()
        {
            // Hooks in from Game UpdatePlay()
            
            // Because saving doesn't always work?
            // TODO: Check for in-game stuff.
        }

        public void PatchPlayerFaction()
        {
            if (this.Colony == null)
            {
                Log.Error("Colony is null. Not patching.");
                return;
            }

            SetPlayerFactionVarsOf(this.Colony.ColonyData.ColonyFaction);
        }

        public void ResetPlayerFaction()
        {
            SetPlayerFactionVarsOf(FactionGenerator.NewGeneratedFaction(FactionDefOf.PlayerColony));
        }

        private void SetPlayerFactionVarsOf(Faction newFaction)
        {
            var ofPlayerFaction = this.WorldData.FactionManager.OfPlayer;
            
            ofPlayerFaction.leader = newFaction.leader;
    
            ofPlayerFaction.def = newFaction.def;

            ofPlayerFaction.Name = newFaction.HasName ? newFaction.Name : null;
            
            ofPlayerFaction.randomKey = newFaction.randomKey;
            ofPlayerFaction.colorFromSpectrum = newFaction.colorFromSpectrum;
            ofPlayerFaction.centralMelanin = newFaction.centralMelanin;

            var relationsField = AccessTools.Field(typeof(Faction), "relations");
            var newFactionRelations = (List<FactionRelation>) relationsField.GetValue(newFaction);

            // Change all relations.
            foreach (var faction in this.WorldData.FactionManager.AllFactionsListForReading)
            {
                var relations = (List<FactionRelation>) relationsField.GetValue(faction);

                foreach (var relation in relations)
                {
                    if (relation.other == newFaction)
                    {
                        relation.other = ofPlayerFaction;
                    }
                }
            }
            
            relationsField.SetValue(ofPlayerFaction, newFactionRelations);
            
            ofPlayerFaction.kidnapped = newFaction.kidnapped;
            
            var predatorThreatsField = AccessTools.Field(typeof(Faction), "predatorThreats");
            predatorThreatsField.SetValue(ofPlayerFaction, predatorThreatsField.GetValue(newFaction));
            
            ofPlayerFaction.defeated = newFaction.defeated;
            ofPlayerFaction.lastTraderRequestTick = newFaction.lastTraderRequestTick;
            ofPlayerFaction.lastMilitaryAidRequestTick = newFaction.lastMilitaryAidRequestTick;

            var naturalGoodwillTimerField = AccessTools.Field(typeof(Faction), "naturalGoodwillTimer");
            naturalGoodwillTimerField.SetValue(ofPlayerFaction, naturalGoodwillTimerField.GetValue(newFaction));
        }

        public IEnumerable<Map> GetMapsForColony(PersistentColony colony)
        {
            foreach (var map in LoadedMaps.Keys)
            {
                if (colony.ColonyData.ActiveWorldTiles.Contains(map))
                {
                    yield return Current.Game.FindMap(map);
                }
            }
        }

        public void SaveColony(PersistentColony colony)
        {
            LoadSaver.SaveColonyAndColonyMapsData(colony);
        }

        public void UnloadColony(PersistentColony colony)
        {
            DynamicMapUnloader.UnloadColonyMaps(colony);
            Find.ColonistBar.MarkColonistsDirty();

            colony.GameData = null;
        }
        
        public void Dispose()
        {
            this.LoadSaver.ReferenceTable.ClearReferences();
        }

        public override string ToString()
        {
            return $"{nameof(PersistentWorld)} " +
                   $"({nameof(Game)}={Game}, " +
                   $"{nameof(Colony)}={Colony}, " +
                   $"{nameof(LoadedMaps)}={LoadedMaps.ToDebugString()}, " +
                   $"{nameof(Colonies)}={Colonies.ToDebugString()})";
        }
        
        public void CheckAndSetColonyData()
        {
            this.CheckAndSetColonyLeader();
        }

        private void CheckAndSetColonyLeader()
        {
            // TODO: Check for Fluffy's Relations Tab / Psychology.   

            if (this.Colony?.ColonyData == null)
            {
                throw new NullReferenceException($"{nameof(CheckAndSetColonyLeader)}: Something is null! {nameof(this.Colony)}: {this.Colony == null} and {nameof(this.Colony.ColonyData)}: {this.Colony?.ColonyData == null}");
            }

            if (this.Colony.ColonyData.Leader != null) return;

            foreach (var pawn in Find.CurrentMap.mapPawns.AllPawns)
            {
                if (!pawn.IsColonist) continue;

                this.Colony.ColonyData.Leader = pawn;
                break;
            }
        }
        #endregion
    }
}