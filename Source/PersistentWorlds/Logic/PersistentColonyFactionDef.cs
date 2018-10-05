using RimWorld;
using Verse;

namespace PersistentWorlds.Logic
{
    public sealed class PersistentColonyFactionDef : FactionDef, IExposable
    {
        public PersistentColonyFactionDef(PersistentColony colony)
        {
            this.defName = colony.ColonyData.ColonyFaction.Name;
        }

        public PersistentColonyFactionDef()
        {
            
        }

        public void ExposeData()
        {
            
        }
    }
}