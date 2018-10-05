using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PersistentWorlds.World
{
    public class Colony : MapParent
    {
        public static readonly Texture2D VisitCommand = ContentFinder<Texture2D>.Get("UI/Commands/Visit", true);

        public string Name;
        public PersistentColony PersistentColony;

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look<string>(ref Name, "name");
            Scribe_References.Look<PersistentColony>(ref PersistentColony, "colony");
        }

        public override void SpawnSetup()
        {
            if (PersistentWorldManager.WorldLoadSaver != null && PersistentWorldManager.WorldLoadSaver.Status ==
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Saving) return;
            
            base.SpawnSetup();
        }
    }
}