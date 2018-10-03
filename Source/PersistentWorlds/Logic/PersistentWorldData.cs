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
            
            Scribe_Deep.Look<WorldInfo>(ref this.info, "info", new object[0]);
            Scribe_Deep.Look<WorldGrid>(ref this.grid, "grid", new object[0]);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Log.Warning("Saving mode...");
            }
            
            Log.Warning("ExposingComponents in PersistentWorldData:ExposeData");
            this.ExposeComponents();
        }

        public void ExposeComponents()
        {
            Log.Message("Called ExposeComponents in persistentworlddata.");
            Scribe_Deep.Look<FactionManager>(ref this.factionManager, "factionManager", new object[0]);
            Scribe_Deep.Look<WorldPawns>(ref this.worldPawns, "worldPawns", new object[0]);
            Scribe_Deep.Look<WorldObjectsHolder>(ref this.worldObjectsHolder, "worldObjects", new object[0]);
            Scribe_Deep.Look<GameConditionManager>(ref this.gameConditionManager, "gameConditionManager", new object[] { PersistentWorldManager.PersistentWorld.Game.World });
            Scribe_Deep.Look<StoryState>(ref this.storyState, "storyState", new object[] { PersistentWorldManager.PersistentWorld.Game.World });
            Scribe_Deep.Look<WorldFeatures>(ref this.worldFeatures, "worldFeatures", new object[0]);
            Scribe_Collections.Look<WorldComponent>(ref this.worldComponents, "worldComponents", LookMode.Deep, new object[] { PersistentWorldManager.PersistentWorld.Game.World });
        }

        public static PersistentWorldData Convert(Game game)
        {
            Log.Message("Called PersistentWorldData conbvert.");
            PersistentWorldData persistentWorldData = new PersistentWorldData();

            persistentWorldData.info = game.World.info;
            persistentWorldData.grid = game.World.grid;

            persistentWorldData.factionManager = game.World.factionManager;
            persistentWorldData.worldPawns = game.World.worldPawns;
            persistentWorldData.worldObjectsHolder = game.World.worldObjects;
            persistentWorldData.gameConditionManager = game.World.gameConditionManager;
            persistentWorldData.storyState = game.World.storyState;
            persistentWorldData.worldFeatures = game.World.features;
            persistentWorldData.worldComponents = game.World.components;
            
            return persistentWorldData;
        }
    }
}