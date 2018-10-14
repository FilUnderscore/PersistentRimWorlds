using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentWorldData : IExposable
    {
        #region Fields
        //private List<PersistentColonyData> colonyDataList = new List<PersistentColonyData>();

        private int nextColonyId;
        
        private WorldInfo info = new WorldInfo();
        private WorldGrid grid;

        private FactionManager factionManager;
        private WorldPawns worldPawns;
        private WorldObjectsHolder worldObjectsHolder;
        private GameConditionManager gameConditionManager;
        private StoryState storyState;
        private WorldFeatures worldFeatures;
        private UniqueIDsManager uniqueIDsManager;
        private List<WorldComponent> worldComponents = new List<WorldComponent>();
        
        private TickManager tickManager = new TickManager();
        #endregion
        
        #region Properties
        //public List<PersistentColonyData> ColonyDataList => colonyDataList;

        public int NextColonyId
        {
            get => nextColonyId;
            set => nextColonyId = value;
        }
        
        public WorldInfo Info => info;
        public WorldGrid Grid => grid;

        public FactionManager FactionManager => factionManager;
        public WorldPawns WorldPawns => worldPawns;
        public WorldObjectsHolder WorldObjectsHolder => worldObjectsHolder;
        public GameConditionManager GameConditionManager => gameConditionManager;
        public StoryState StoryState => storyState;
        public WorldFeatures WorldFeatures => worldFeatures;
        public UniqueIDsManager UniqueIDsManager => uniqueIDsManager;
        public List<WorldComponent> WorldComponents => worldComponents;

        public TickManager TickManager => tickManager;
        #endregion
        
        #region Methods
        public void ExposeData()
        {            
            Log.Warning("Calling PersistentWorldData ExposeData()");

            //Scribe_Collections.Look<PersistentColonyData>(ref colonyDataList, "colonies", LookMode.Deep);
            
            Scribe_Deep.Look<WorldInfo>(ref this.info, "info", new object[0]);
            Current.Game.World.info = this.info;
            
            Scribe_Deep.Look<WorldGrid>(ref this.grid, "grid", new object[0]);
            Current.Game.World.grid = this.grid;
            
            Log.Warning("ExposingComponents in PersistentWorldData:ExposeData");
            this.ExposeComponents();
        }

        private void ExposeComponents()
        {   
            Log.Message("Construct components");
            Current.Game.World.ConstructComponents();
            
            Log.Message("Called ExposeComponents in persistentworlddata.");
            
            Scribe_Deep.Look<TickManager>(ref this.tickManager, "tickManager", new object[0]);
            Current.Game.tickManager = this.tickManager;
            
            Scribe_Deep.Look<FactionManager>(ref this.factionManager, "factionManager", new object[0]);
            Current.Game.World.factionManager = this.factionManager;
            
            Scribe_Deep.Look<WorldPawns>(ref this.worldPawns, "worldPawns", new object[0]);
            Current.Game.World.worldPawns = this.worldPawns;
            
            Scribe_Deep.Look<WorldObjectsHolder>(ref this.worldObjectsHolder, "worldObjects", new object[0]);
            Current.Game.World.worldObjects = this.worldObjectsHolder;
            
            Scribe_Deep.Look<GameConditionManager>(ref this.gameConditionManager, "gameConditionManager", new object[] { Current.Game.World });
            Current.Game.World.gameConditionManager = this.gameConditionManager;
            
            Scribe_Deep.Look<StoryState>(ref this.storyState, "storyState", new object[] { Current.Game.World });
            Current.Game.World.storyState = this.storyState;
            
            Scribe_Deep.Look<WorldFeatures>(ref this.worldFeatures, "worldFeatures", new object[0]);
            Current.Game.World.features = this.worldFeatures;
            
            Scribe_Deep.Look<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
            Current.Game.uniqueIDsManager = this.uniqueIDsManager;
            
            Scribe_Collections.Look<WorldComponent>(ref this.worldComponents, "worldComponents", LookMode.Deep, new object[] { Current.Game.World });
            Current.Game.World.components = this.worldComponents;
        }

        public static PersistentWorldData Convert(Game game)
        {
            Log.Message("Called PersistentWorldData conbvert.");
            var persistentWorldData = new PersistentWorldData
            {
                nextColonyId = PersistentWorldManager.GetInstance().PersistentWorld == null ? 0 : PersistentWorldManager.GetInstance().PersistentWorld.WorldData.nextColonyId,
                info = game.World.info,
                grid = game.World.grid,
                tickManager = game.tickManager,
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
        #endregion
    }
}