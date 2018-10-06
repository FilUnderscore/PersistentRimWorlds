using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PersistentWorlds.World
{
    public class Colony : MapParent
    {
        // TODO: Implement commands.
        //public static readonly Texture2D VisitCommand = ContentFinder<Texture2D>.Get("UI/Commands/Visit", true);

        public string Name;
        public PersistentColony PersistentColony;
        private Material cachedMat;
        
        public override Material Material
        {
            get
            {
                if (this.cachedMat == null)
                    this.cachedMat = MaterialPool.MatFrom("UI/World/Colony", ShaderDatabase.WorldOverlayTransparentLit, Color.green,
                        WorldMaterials.WorldObjectRenderQueue);

                return this.cachedMat;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look<string>(ref Name, "name");
            Scribe_References.Look<PersistentColony>(ref PersistentColony, "colony");
        }
    }
}