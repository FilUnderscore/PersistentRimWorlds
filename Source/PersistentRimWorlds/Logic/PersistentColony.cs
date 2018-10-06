using RimWorld;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColony : IExposable
    {
        public PersistentColonyData ColonyData = new PersistentColonyData();

        public void ExposeData()
        {
            Scribe_Deep.Look<PersistentColonyData>(ref ColonyData, "data");
        }
        
        public static PersistentColony Convert(Game game, PersistentColonyData colonyColonyData = null)
        {
            var persistentColony = new PersistentColony
            {
                ColonyData = PersistentColonyData.Convert(game, colonyColonyData)
            };

            return persistentColony;
        }
        
        public Faction AsFaction()
        {
            // TODO: Check if ColonyFaction is null, if so return new arrivals.
            Log.Message("As Faction.");
            var preexistingFaction =
                PersistentWorldManager.PersistentWorld.Game.World.factionManager.FirstFactionOfDef(FactionDefOf
                    .PlayerColony);

            if (preexistingFaction == ColonyData.ColonyFaction)
            {
                return ColonyData.ColonyFaction;
            }
            
            PersistentWorldManager.PersistentWorld.Game.World.factionManager.Add(ColonyData.ColonyFaction);
            PersistentWorldManager.PersistentWorld.Game.World.factionManager.Remove(preexistingFaction);
            
            return ColonyData.ColonyFaction;
        }
    }
}