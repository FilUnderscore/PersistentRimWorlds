using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColony : IExposable
    {
        public Faction Faction; // Used when AsFaction() is called.
        
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

            if (Faction != null)
            {
                return Faction;
            }
            
            var preexistingFaction =
                PersistentWorldManager.PersistentWorld.Game.World.factionManager.FirstFactionOfDef(FactionDefOf
                    .PlayerColony);

            preexistingFaction.Name = this.ColonyData.ColonyFaction.Name;

            if (!this.ColonyData.ColonyFaction.HasName)
            {
                preexistingFaction.Name = null;
            }
            
            preexistingFaction.leader = this.ColonyData.ColonyFaction.leader;
            var factionRelations =
                AccessTools.Field(typeof(Faction), "relations").GetValue(this.ColonyData.ColonyFaction);
            AccessTools.Field(typeof(Faction), "relations").SetValue(preexistingFaction, factionRelations);

            Faction = preexistingFaction;

            return preexistingFaction;
        }
    }
}