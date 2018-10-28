using System;
using System.Collections.Generic;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.World;
using Verse;

namespace PersistentWorlds.Logic.Comps
{
    public sealed class HumanColonyThingComp : ThingComp
    {
        public int ColonyId = -1;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            this.SetColonyData();
        }

        private void SetColonyData()
        {
            if (!PersistentWorldManager.GetInstance().HasPersistentWorld)
                return;

            if (!(this.parent is Pawn pawn) || !pawn.IsColonist ||
                !PersistentWorldManager.GetInstance()
                    .PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame))
                return;

            var colony = GetColony();
            
            this.ColonyId = colony?.PersistentColonyData?.UniqueId ??
                            PersistentWorldManager.GetInstance().PersistentWorld.Colony.ColonyData.UniqueId;
        }

        private Colony GetColony()
        {
            var tile = this.parent.Tile;
            var worldObject = Find.WorldObjects.WorldObjectAt(tile, PersistentWorldsDefOf.Colony);

            return (Colony) worldObject;
        }
    }
}