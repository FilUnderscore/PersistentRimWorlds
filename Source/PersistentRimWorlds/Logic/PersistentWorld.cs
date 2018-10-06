﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentWorld
    {
        // Game.World is accessed.
        public Game Game = new Game();

        public PersistentWorldData WorldData = new PersistentWorldData();
        public PersistentColony Colony;
        
        //public List<Map> Maps = new List<Map>();
        public Dictionary<Map, bool> Maps = new Dictionary<Map, bool>();
        public List<PersistentColony> Colonies = new List<PersistentColony>();

        public PersistentWorld()
        {
            Current.Game = this.Game;
            this.Game.World = new RimWorld.Planet.World();
        }

        public void LoadWorld()
        {
            Log.Message("Setting current game.");
            Current.Game = this.Game;
            
            Log.Message("Calling LoadWorld");
            LongEventHandler.SetCurrentEventText("LoadingPersistentWorld".Translate());
            
            this.Game.LoadGame();
            
            // At the end.. because Scribe doesn't run due to us not loading Game directly.
            this.Game.FinalizeInit();
            
            Log.Message("Set faction second time ?");
            AccessTools.Field(typeof(FactionManager), "ofPlayer").SetValue(this.Game.World.factionManager, this.Colony.ColonyData.ColonyFaction);
            FactionGenerator.EnsureRequiredEnemies(this.Colony.ColonyData.ColonyFaction);
            
            GameComponentUtility.LoadedGame();

            var pFaction = Current.Game.World.factionManager.FirstFactionOfDef(FactionDefOf.PlayerColony);
            Current.Game.World.factionManager.Remove(pFaction);

            this.Colony.ColonyData.ColonyFaction.loadID = 9;
            Current.Game.World.factionManager.Add(this.Colony.ColonyData.ColonyFaction);
            
//            GenScene.GoToMainMenu();
        }

        public void ExposeAndFillGameSmallComponents()
        {
            Log.Message("Calling ExposeAndFillGameSmallComponents");

            if (Colony == null)
            {
                // Return to main menu.
                Log.Error("Colony is null. - Persistent Worlds");
                GenScene.GoToMainMenu();
                return;
            }
            
            /*
            AccessTools.Field(typeof(Game), "info").SetValue(this.Game, Colony.ColonyData.GameData.info);
            AccessTools.Field(typeof(Game), "rules").SetValue(this.Game, Colony.ColonyData.GameData.rules);
            this.Game.Scenario = Colony.ColonyData.GameData.scenario;
            this.Game.tickManager = this.WorldData.TickManager;
            this.Game.playSettings = Colony.ColonyData.GameData.playSettings;
            this.Game.storyWatcher = Colony.ColonyData.GameData.storyWatcher;
            this.Game.gameEnder = Colony.ColonyData.GameData.gameEnder;
            this.Game.letterStack = Colony.ColonyData.GameData.letterStack;
            this.Game.researchManager = Colony.ColonyData.GameData.researchManager;
            this.Game.storyteller = Colony.ColonyData.GameData.storyteller;
            this.Game.history = Colony.ColonyData.GameData.history;
            this.Game.taleManager = Colony.ColonyData.GameData.taleManager;
            this.Game.playLog = Colony.ColonyData.GameData.playLog;
            this.Game.battleLog = Colony.ColonyData.GameData.battleLog;
            this.Game.outfitDatabase = Colony.ColonyData.GameData.outfitDatabase;
            this.Game.drugPolicyDatabase = Colony.ColonyData.GameData.drugPolicyDatabase;
            this.Game.tutor = Colony.ColonyData.GameData.tutor;
            this.Game.dateNotifier = Colony.ColonyData.GameData.dateNotifier;
            this.Game.components = Colony.ColonyData.GameData.gameComponents;
            */
            
            Colony.ColonyData.GameData.SetGame();

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            Log.Warning("LoadingVars - Experimental (PersistentWorld:PersistentWorld) in ExposeGameSmallComps.");
            AccessTools.Method(typeof(Game), "FillComponents", new Type[0]).Invoke(this.Game, new object[0]);
            BackCompatibility.GameLoadingVars(this.Game);
        }

        public void LoadGameWorldAndMaps()
        {
            Log.Message("Calling LoadGameWorldAndMaps");

            this.ExposeGameWorldData();
            
            this.Game.World.FinalizeInit();

            this.LoadMaps();

            this.ContinueLoadingMaps();
        }

        public void ContinueLoadingMaps()
        {
            if (this.Game.Maps.RemoveAll((Map x) => x == null) != 0)
            {
                Log.Warning("Custom - Some maps were null after loading.", false);
            }

            int num = -1;

            num = Colony.ColonyData.GameData.currentMapIndex;
            if (num < 0 && this.Game.Maps.Any<Map>())
            {
                Log.Error("PersistentWorlds - Current map is null after loading but there are maps available. Setting current map to [0].", false);
                num = 0;
            }

            Log.Message("Num");
            Log.Message("Maps Count: " + this.Game.Maps.Count);
            
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
            
            //AccessTools.Field(typeof(Game), "maps").SetValue(this.Game, this.Maps);

            Game.CurrentMap = ((num < 0) ? null : this.Game.Maps[num]);
            
            if(Find.CameraDriver != null)
                Find.CameraDriver.Expose();
            
            for (int i = 0; i < this.Game.Maps.Count; i++)
            {
                try
                {
                    this.Game.Maps[i].FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in Map.FinalizeLoading(): " + e, false);
                }

                try
                {
                    this.Game.Maps[i].Parent.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in MapParent.FinalizeLoading(): " + e, false);
                }
            }
        }

        private void LoadMaps()
        {
            PersistentWorldManager.WorldLoadSaver.LoadMaps();
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
            Log.Message("Calling ConstructGameWorldComponentsAndExposeComponents");
            this.Game.World.ConstructComponents();

            this.ExposeAndFillGameWorldComponents();
        }

        public void ExposeAndFillGameWorldComponents()
        {
            Log.Message("Calling ExposeAndFillGameWorldComponents");

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

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Log.Warning("LoadingVars - Experimental (PersistentWorld:PersistentWorld) in ExposeAndFillGameWorldComponents.");
                //BackCompatibility.WorldLoadingVars();
                if (this.WorldData.uniqueIDsManager != null)
                {
                    this.Game.uniqueIDsManager = this.WorldData.uniqueIDsManager;
                }
            }
        }

        public static PersistentWorld Convert(Game game)
        {
            Log.Warning("Run persistentworld convert.");
            
            PersistentWorld persistentWorld = new PersistentWorld();
            persistentWorld.Game = game;
            Current.Game = game;

            persistentWorld.WorldData = PersistentWorldData.Convert(game);

            persistentWorld.Colonies.Add(PersistentColony.Convert(game));
            
            persistentWorld.ConvertCurrentGameSettlements(game);
            
            //persistentWorld.Maps = game.Maps;
            
            return persistentWorld;
        }

        public void AddColoniesToWorldMap()
        {
            
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
                    Log.Message("settlement faction: " + settlement.Faction.Name);
                    Log.Message("settlement name: " + settlement.Name);
                    continue;
                }

                var colony = (Colony) WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("Colony"));
                settlement.Map.info.parent = colony;
                colony.Tile = settlement.Tile;
                colony.Name = settlement.Name;

                colony.PersistentColonyData = this.Colony != null ? this.Colony.ColonyData : this.Colonies[0].ColonyData;

                toAdd.Add(colony);
                toRemove.Add(settlement);
            }
            
            toAdd.Do(colony => game.World.worldObjects.Add(colony));
            toAdd.Clear();
            
            toRemove.Do(settlement => game.World.worldObjects.Remove(settlement));
            toRemove.Clear();
            
            // TODO: Have one ofPlayer faction, possibly by removing all player colonies on load and adding current colony as player faction.
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

                Log.Message("Yes");
                
                if (colony.PersistentColonyData == null)
                {
                    Log.Error("ColonyData is null for Colony WorldObject.");
                    continue;
                }
                
                Log.Message("Yes2");

                if (this.Colony == null || colony.PersistentColonyData == null || this.Colony.ColonyData == null || colony.PersistentColonyData.uniqueID != this.Colony.ColonyData.uniqueID) continue;
                
                Log.Message("Yes 3");
                
                var settlement = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction(Faction.OfPlayer);

                Log.Message("Yes 4");
                
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
    }
}