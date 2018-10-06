using RimWorld;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColony : IExposable, ILoadReferenceable
    {
        public PersistentColonyData ColonyData = new PersistentColonyData();

        public void ExposeData()
        {
            this.ColonyData.ExposeData();
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
            var faction = new Faction {Name = ColonyData.Name};

            if (PersistentWorldManager.PersistentWorld != null && PersistentWorldManager.PersistentWorld.Colony == this)
            {
                faction.def = FactionDefOf.PlayerColony;
            }
            else
            {
                faction.def = FactionDefOf.Ancients;
            }

            return faction;
        }
        
        public string GetUniqueLoadID()
        {
            return "Colony_" + this.ColonyData.uniqueID;
        }
    }
}