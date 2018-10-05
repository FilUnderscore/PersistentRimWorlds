using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentWorldData : IExposable
    {
        public WorldInfo info = new WorldInfo();
        public WorldGrid grid;

        public FactionManager factionManager;
        public WorldPawns worldPawns;
        public WorldObjectsHolder worldObjectsHolder;
        public GameConditionManager gameConditionManager;
        public StoryState storyState;
        public WorldFeatures worldFeatures;
        public UniqueIDsManager uniqueIDsManager;
        public List<WorldComponent> worldComponents = new List<WorldComponent>();
        
        public TickManager TickManager = new TickManager();

        public void ExposeData()
        {
            Log.Warning("Calling PersistentWorldData ExposeData()");

            if (PersistentWorldManager.PersistentWorld == null)
            {
                Log.Error("PersistentWorld is null.");
                
                GenScene.GoToMainMenu();

                return;
            }
            
            Scribe_Deep.Look<WorldInfo>(ref this.info, "info", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.World.info = this.info;
            Scribe_Deep.Look<WorldGrid>(ref this.grid, "grid", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.World.grid = this.grid;
            
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Log.Warning("Saving mode...");
            }
            
            Log.Warning("ExposingComponents in PersistentWorldData:ExposeData");
            this.ExposeComponents();
        }

        public void ExposeComponents()
        {
            if (PersistentWorldManager.PersistentWorld == null)
            {
                Log.Error("PersistentWorld is null.");
                
                GenScene.GoToMainMenu();

                return;
            }
            
            Log.Message("Construct components");
            PersistentWorldManager.PersistentWorld.Game.World.ConstructComponents();
            
            Log.Message("Called ExposeComponents in persistentworlddata.");
            
            Scribe_Deep.Look<TickManager>(ref this.TickManager, "tickManager", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.tickManager = this.TickManager;
            
            Scribe_Deep.Look<FactionManager>(ref this.factionManager, "factionManager", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.World.factionManager = this.factionManager;
            
            Scribe_Deep.Look<WorldPawns>(ref this.worldPawns, "worldPawns", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.World.worldPawns = this.worldPawns;
            
            Scribe_Deep.Look<WorldObjectsHolder>(ref this.worldObjectsHolder, "worldObjects", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.World.worldObjects = this.worldObjectsHolder;
            
            Scribe_Deep.Look<GameConditionManager>(ref this.gameConditionManager, "gameConditionManager", new object[] { PersistentWorldManager.PersistentWorld.Game.World });
            PersistentWorldManager.PersistentWorld.Game.World.gameConditionManager = this.gameConditionManager;
            
            Scribe_Deep.Look<StoryState>(ref this.storyState, "storyState", new object[] { PersistentWorldManager.PersistentWorld.Game.World });
            PersistentWorldManager.PersistentWorld.Game.World.storyState = this.storyState;
            
            Scribe_Deep.Look<WorldFeatures>(ref this.worldFeatures, "worldFeatures", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.World.features = this.worldFeatures;
            
            Scribe_Deep.Look<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
            PersistentWorldManager.PersistentWorld.Game.uniqueIDsManager = this.uniqueIDsManager;
            
            Scribe_Collections.Look<WorldComponent>(ref this.worldComponents, "worldComponents", LookMode.Deep, new object[] { PersistentWorldManager.PersistentWorld.Game.World });
            PersistentWorldManager.PersistentWorld.Game.World.components = this.worldComponents;
        }

        public static PersistentWorldData Convert(Game game)
        {
            Log.Message("Called PersistentWorldData conbvert.");
            var persistentWorldData = new PersistentWorldData
            {
                info = game.World.info,
                grid = game.World.grid,
                TickManager = game.tickManager,
                factionManager = game.World.factionManager,
                worldPawns = game.World.worldPawns,
                worldObjectsHolder = game.World.worldObjects,
                gameConditionManager = game.World.gameConditionManager,
                storyState = game.World.storyState,
                worldFeatures = game.World.features,
                uniqueIDsManager = game.uniqueIDsManager,
                worldComponents = game.World.components
            };

            return persistentWorldData;
        }
    }
}