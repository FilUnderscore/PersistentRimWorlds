using Harmony;
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

        public void SetFactionData()
        {
            var gameColony = PersistentWorldManager.PersistentWorld.Game.World.factionManager.OfPlayer;
            var dataColony = this.ColonyData.ColonyFaction;

            if (dataColony.HasName)
            {
                gameColony.Name = dataColony.Name;
            }
            else
            {
                gameColony.Name = null;
            }

            var relationsField = AccessTools.Field(typeof(Faction), "relations");
            relationsField.SetValue(gameColony, relationsField.GetValue(dataColony));
        }
    }
}