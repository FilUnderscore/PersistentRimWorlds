using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColony
    {
        public PersistentColonyData ColonyData = new PersistentColonyData();

        public static PersistentColony Convert(Game game)
        {
            PersistentColony persistentColony = new PersistentColony();
            
            persistentColony.ColonyData = PersistentColonyData.Convert(game);

            return persistentColony;
        }
    }
}