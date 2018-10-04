using System;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentWorld
    {
        public string fileName;
        
        // Game.World is accessed.
        public Game Game = new Game();

        public PersistentWorldData WorldData = new PersistentWorldData();
        public PersistentColony colony;
        
        public List<Map> Maps = new List<Map>();
        public List<PersistentColony> Colonies = new List<PersistentColony>();

        public PersistentWorld()
        {
            Current.Game = this.Game;
            this.Game.World = new RimWorld.Planet.World();
        }

        public void LoadWorld()
        {
            Log.Message("Calling LoadWorld");
            LongEventHandler.SetCurrentEventText("LoadingPersistentWorld".Translate());
            
            this.Game.LoadGame();
            
            // At the end.. because Scribe doesn't run due to us not loading Game directly.
            this.Game.FinalizeInit();
            
            GameComponentUtility.LoadedGame();
        }

        public void ExposeAndFillGameSmallComponents()
        {
            Log.Message("Calling ExposeAndFillGameSmallComponents");

            if (colony == null)
            {
                // Return to main menu.
                Log.Error("Colony is null. - Persistent Worlds");
                GenScene.GoToMainMenu();
                return;
            }
            
            AccessTools.Field(typeof(Game), "info").SetValue(this.Game, colony.ColonyData.GameData.info);
            AccessTools.Field(typeof(Game), "rules").SetValue(this.Game, colony.ColonyData.GameData.rules);
            this.Game.Scenario = colony.ColonyData.GameData.scenario;
            this.Game.tickManager = this.WorldData.TickManager;
            this.Game.playSettings = colony.ColonyData.GameData.playSettings;
            this.Game.storyWatcher = colony.ColonyData.GameData.storyWatcher;
            this.Game.gameEnder = colony.ColonyData.GameData.gameEnder;
            this.Game.letterStack = colony.ColonyData.GameData.letterStack;
            this.Game.researchManager = colony.ColonyData.GameData.researchManager;
            this.Game.storyteller = colony.ColonyData.GameData.storyteller;
            this.Game.history = colony.ColonyData.GameData.history;
            this.Game.taleManager = colony.ColonyData.GameData.taleManager;
            this.Game.playLog = colony.ColonyData.GameData.playLog;
            this.Game.battleLog = colony.ColonyData.GameData.battleLog;
            this.Game.outfitDatabase = colony.ColonyData.GameData.outfitDatabase;
            this.Game.drugPolicyDatabase = colony.ColonyData.GameData.drugPolicyDatabase;
            this.Game.tutor = colony.ColonyData.GameData.tutor;
            this.Game.dateNotifier = colony.ColonyData.GameData.dateNotifier;
            this.Game.uniqueIDsManager = colony.ColonyData.GameData.uniqueIDsManager;
            this.Game.components = colony.ColonyData.GameData.gameComponents;
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Log.Warning("LoadingVars - Experimental (PersistentWorld:PersistentWorld) in ExposeGameSmallComps.");
                AccessTools.Method(typeof(Game), "FillComponents", new Type[0]).Invoke(this.Game, new object[0]);
                BackCompatibility.GameLoadingVars(this.Game);
            }
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
            if (this.Maps.RemoveAll((Map x) => x == null) != 0)
            {
                Log.Warning("Custom - Some maps were null after loading.", false);
            }

            int num = -1;

            num = colony.ColonyData.GameData.currentMapIndex;
            if (num < 0 && this.Maps.Any<Map>())
            {
                Log.Error("PersistentWorlds - Current map is null after loading but there are maps available. Setting current map to [0].", false);
                num = 0;
            }

            Log.Message("Num");
            Log.Message("Maps Count: " + this.Maps.Count);
            
            if (num >= this.Maps.Count)
            {
                Log.Error("Current map index out of bounds after loading.", false);
                if (this.Maps.Any<Map>())
                {
                    num = 0;
                }
                else
                {
                    num = -1;
                }
            }
            
            AccessTools.Field(typeof(Game), "maps").SetValue(this.Game, this.Maps);

            Game.CurrentMap = ((num < 0) ? null : this.Maps[num]);
            Find.CameraDriver.Expose();
            
            for (int i = 0; i < this.Maps.Count; i++)
            {
                try
                {
                    this.Maps[i].FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in Map.FinalizeLoading(): " + e, false);
                }

                try
                {
                    this.Maps[i].Parent.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in MapParent.FinalizeLoading(): " + e, false);
                }
            }
        }

        private void LoadMaps()
        {
            PersistentWorldManager.WorldLoader.LoadMaps(this);
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
            
            this.Game.World.factionManager = this.WorldData.factionManager;
            this.Game.World.worldPawns = this.WorldData.worldPawns;
            this.Game.World.worldObjects = this.WorldData.worldObjectsHolder;
            this.Game.World.gameConditionManager = this.WorldData.gameConditionManager;
            this.Game.World.storyState = this.WorldData.storyState;
            this.Game.World.features = this.WorldData.worldFeatures;
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
            
            persistentWorld.Maps = game.Maps;
            
            return persistentWorld;
        }
    }
}