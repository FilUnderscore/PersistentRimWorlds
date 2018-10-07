using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Harmony;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace PersistentWorlds.Logic
{
    public class PersistentWorld
    {
        // TODO: Refactor.
        
        // Game.World is accessed.
        public Game Game = new Game();

        public PersistentWorldData WorldData = new PersistentWorldData();
        public PersistentColony Colony;
        
        public Dictionary<PersistentColony, List<Map>> Maps = new Dictionary<PersistentColony, List<Map>>();
        public List<PersistentColony> Colonies = new List<PersistentColony>();

        public PersistentWorld()
        {
            Current.Game = this.Game;
            this.Game.World = new RimWorld.Planet.World();
        }

        public void LoadWorld()
        {
            Current.Game = this.Game;
            
            LongEventHandler.SetCurrentEventText("LoadingPersistentWorld".Translate());
            
            this.Game.LoadGame();
            
            // At the end.. because Scribe doesn't run due to us not loading Game directly.
            this.Game.FinalizeInit();
            
            this.LoadCameraDriver();
            
            GameComponentUtility.LoadedGame();
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
            
            Colony.ColonyData.GameData.SetGame();

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            AccessTools.Method(typeof(Game), "FillComponents", new Type[0]).Invoke(this.Game, new object[0]);
            BackCompatibility.GameLoadingVars(this.Game);

            /*
             * Load world and maps.
             */
            
            this.LoadGameWorldAndMaps();
        }

        public void LoadGameWorldAndMaps()
        {
            this.ExposeGameWorldData();
            
            this.Game.World.FinalizeInit();

            this.LoadMaps();
        }

        private void LoadMaps()
        {
            PersistentWorldManager.WorldLoadSaver.LoadMaps();
            
            // TODO: Load all maps in memory but have maps in Current.Game.Maps depending on active maps. Maps can be shared.
            
            if (this.Game.Maps.RemoveAll((Map x) => x == null) != 0)
            {
                Log.Warning("Some maps were null after loading.", false);
            }

            int num = -1;

            num = Colony.ColonyData.GameData.currentMapIndex;
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
            }
        }

        private void LoadCameraDriver()
        {
            if (Find.CameraDriver == null)
            {
                Log.Error("Current CameraDriver is null.");
                return;
            }
            
            Find.CameraDriver.SetRootPosAndSize(this.Colony.ColonyData.GameData.camRootPos, this.Colony.ColonyData.GameData.desiredSize);
        }

        public void ExposeGameWorldData()
        {
            this.Game.World.info = this.WorldData.info;
            this.Game.World.grid = this.WorldData.grid;

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
            this.Game.World.factionManager = this.WorldData.factionManager;
            this.Game.World.worldPawns = this.WorldData.worldPawns;
            this.Game.World.worldObjects = this.WorldData.worldObjectsHolder;
            this.Game.World.gameConditionManager = this.WorldData.gameConditionManager;
            this.Game.World.storyState = this.WorldData.storyState;
            this.Game.World.features = this.WorldData.worldFeatures;
            this.Game.uniqueIDsManager = this.WorldData.uniqueIDsManager;
            this.Game.World.components = this.WorldData.worldComponents;
            
            AccessTools.Method(typeof(RimWorld.Planet.World), "FillComponents", new Type[0]).Invoke(this.Game.World, new object[0]);

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            if (this.WorldData.uniqueIDsManager != null)
            {
                this.Game.uniqueIDsManager = this.WorldData.uniqueIDsManager;
            }
        }

        public static PersistentWorld Convert(Game game)
        {
            var persistentWorld = new PersistentWorld {Game = game};
            Current.Game = game;
            
            persistentWorld.WorldData = PersistentWorldData.Convert(game);
            
            persistentWorld.Colonies.Add(PersistentColony.Convert(game));
            
            persistentWorld.ConvertCurrentGameSettlements(game);
            
            return persistentWorld;
        }

        // Convert Settlements to Colony Bases (this.Colony) for saving
        public void ConvertCurrentGameSettlements(Game game)
        {
            // Concurrency errors :/
            var toAdd = new List<Colony>();
            var toRemove = new List<Settlement>();
            
            foreach (var settlement in game.World.worldObjects.Settlements)
            {
                if (settlement.Faction != Faction.OfPlayer)
                {
                    continue;
                }
                
                var colony = (Colony) WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("Colony"));
                settlement.Map.info.parent = colony;
                colony.Tile = settlement.Tile;
                colony.Name = settlement.Name;

                colony.PersistentColonyData = PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting ? this.Colonies[0].ColonyData : this.Colony.ColonyData;

                toAdd.Add(colony);
                toRemove.Add(settlement);
            }
            
            toAdd.Do(colony => game.World.worldObjects.Add(colony));
            toAdd.Clear();
            
            toRemove.Do(settlement => game.World.worldObjects.Remove(settlement));
            toRemove.Clear();
        }

        // Convert Colony Bases to Settlements (this.Colony) for loading
        public void ConvertToCurrentGameSettlements()
        {   
            var toAdd = new List<Settlement>();
            var toRemove = new List<Colony>();
            
            foreach (var mapParent in this.WorldData.worldObjectsHolder.MapParents)
            {
                if (!(mapParent is Colony)) continue;
                
                var colony = (Colony) mapParent;
                
                if (this.Colony == null || colony.PersistentColonyData == null || this.Colony.ColonyData == null || colony.PersistentColonyData.uniqueID != this.Colony.ColonyData.uniqueID) continue;
                
                var settlement = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(Faction.OfPlayer);
                
                // TODO: Unexpected? null colony map for some odd reason.
                if (colony.Map?.info == null)
                {
                    if (colony.Map == null)
                    {
                        Log.Error("Colony map is null.");
                    }
                    else
                    {
                        if (colony.Map.info == null)
                        {
                            Log.Error("Colony map info is null.");
                        }
                    }

                    continue;
                }
                
                colony.Map.info.parent = settlement;
                settlement.Tile = colony.Tile;
                settlement.Name = colony.Name;
                
                toAdd.Add(settlement);
                toRemove.Add(colony);
            }
            
            toAdd.Do(settlement => this.WorldData.worldObjectsHolder.Add(settlement));
            toAdd.Clear();
            
            toRemove.Do(colony => this.WorldData.worldObjectsHolder.Remove(colony));
            toRemove.Clear();
        }

        public void SortMaps(IEnumerable<Map> maps)
        {            
            foreach (var map in maps)
            {
                foreach (var colony in this.Colonies)
                { 
                    if (!colony.ColonyData.ActiveWorldTiles.Contains(map.Tile)) continue;
                    
                    if(!this.Maps.ContainsKey(colony))
                        this.Maps.Add(colony, new List<Map>());
                    
                    this.Maps[colony].Add(map);
                }
            }
        }

        public void PreAddMaps()
        {
            if (this.Colony == null) return;
            
            foreach (var map in this.Maps[this.Colony])
            {
                if (!this.Game.Maps.Contains(map))
                {
                    this.Game.Maps.Add(map);
                }
            }
        }

        public void UpdateWorld()
        {
            // Hooks in from Game UpdatePlay()
            return;
            
            if (this.Colony != null && this.Maps.ContainsKey(this.Colony))
            {
                /*
                 * Add Needed Maps
                 */
                var toAdd = new List<Map>();

                foreach (var map in this.Maps[this.Colony])
                {
                    if (!this.Game.Maps.Contains(map))
                    {
                        toAdd.Add(map);
                    }
                }

                toAdd.Do(map => this.Game.AddMap(map));
                toAdd.Do(map => map.FinalizeLoading());
                toAdd.Do(map => MapComponentUtility.MapGenerated(map));
                toAdd.Clear();
                
                /*
                 * Remove Unneeded Maps
                 */
                var toRemove = new List<Map>();

                foreach (var map in this.Game.Maps)
                {
                    // TODO: Add map when needed so it doesn't get deinitialized by mistake.
                    if (!this.Maps[this.Colony].Contains(map))
                    {
                        toRemove.Add(map);
                    }
                }

                toRemove.Do(map => this.Game.DeinitAndRemoveMap(map));
                toRemove.Clear();
            }
        }

        public void PatchPlayerFaction()
        {
            if (this.Colony == null)
            {
                Log.Error("Colony is null. Not patching.");
                return;
            }

            // TODO: Change all vars.
            SetPlayerFactionVarsOf(this.Colony.ColonyData.ColonyFaction);
        }

        public void ResetPlayerFaction()
        {
            // TODO: Change all vars.
            SetPlayerFactionVarsOf(FactionGenerator.NewGeneratedFaction(FactionDefOf.PlayerColony));
        }

        private void SetPlayerFactionVarsOf(Faction newFaction)
        {
            var ofPlayerFaction = this.WorldData.factionManager.OfPlayer;
            ofPlayerFaction.leader = newFaction.leader;
            ofPlayerFaction.avoidGridsSmart = newFaction.avoidGridsSmart;
            ofPlayerFaction.def = newFaction.def;

            ofPlayerFaction.Name = newFaction.HasName ? newFaction.Name : null;
            
            ofPlayerFaction.randomKey = newFaction.randomKey;
            ofPlayerFaction.colorFromSpectrum = newFaction.colorFromSpectrum;

            var relationsField = AccessTools.Field(typeof(Faction), "relations");
            var newFactionRelations = (List<FactionRelation>) relationsField.GetValue(newFaction);

            // Change all relations.
            foreach (var faction in this.WorldData.factionManager.AllFactionsListForReading)
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
            
            // Remove any relations with other player colonies.
            // newFaction.RemoveAllRelations() doesn't work because requires Find.FactionManager which can be null depending on when this is called.
        }

        // TODO: Reset player faction to be tribe or colony depending on scenario. Should be called before selecting landing site.
        public void ResetPlayerFactionScenario()
        {
            
        }
    }
}