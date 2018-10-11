using System.IO;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColony : IExposable
    {
        public FileInfo FileInfo;
        public PersistentColonyData ColonyData = new PersistentColonyData();

        public void ExposeData()
        {
            Scribe_Deep.Look(ref ColonyData, "data");
        }
        
        public static PersistentColony Convert(Game game, PersistentColonyData colonyColonyData = null)
        {
            var persistentColony = new PersistentColony
            {
                ColonyData = PersistentColonyData.Convert(game, colonyColonyData)
            };

            return persistentColony;
        }
    }
}