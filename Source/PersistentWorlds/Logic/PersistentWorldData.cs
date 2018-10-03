using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentWorldData
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
    }
}