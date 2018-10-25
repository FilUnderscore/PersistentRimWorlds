using System;
using System.Collections.Generic;
using System.Linq;
using PersistentWorlds.Logic;
using PersistentWorlds.Logic.Comps;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PersistentWorlds.World
{
    /// <summary>
    /// Colony world object class that handles colonies on the world map.
    /// </summary>
    [StaticConstructorOnStartup]
    public sealed class Colony : MapParent
    {
        #region Fields
        // TODO: Implement commands.
        private static readonly Texture2D VisitCommand = ContentFinder<Texture2D>.Get("UI/Commands/Visit", true);

        public string Name;
        public PersistentColonyData PersistentColonyData = new PersistentColonyData();
        private Material cachedMat;
        #endregion
        
        #region Properties
        /// <summary>
        /// Material for colonies, such as color properties that are set here depending on colony data.
        /// </summary>
        public override Material Material
        {
            get
            {
                if (this.cachedMat == null)
                    this.cachedMat = MaterialPool.MatFrom("World/WorldObjects/DefaultSettlement", ShaderDatabase.WorldOverlayTransparentLit, this.PersistentColonyData?.Color ?? Color.green,
                        WorldMaterials.WorldObjectRenderQueue);

                return this.cachedMat;
            }
        }

        public override Texture2D ExpandingIcon => ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town", true);

        public override string Label => Name ?? base.Label;

        public override bool HasName => !Name.NullOrEmpty();
        #endregion
        
        #region Methods
        /// <summary>
        /// Saving/loading of colony world object instance.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look(ref Name, "name");
            Scribe_References.Look(ref PersistentColonyData, "colony");
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            foreach (var gizmo in base.GetCaravanGizmos(caravan))
            {
                yield return gizmo;
            }

            if (!this.HasMap)
            {
                yield return new Command_Action
                {
                    defaultLabel = "FilUnderscore.PersistentRimWorlds.VisitColony".Translate(),
                    defaultDesc = "FilUnderscore.PersistentRimWorlds.VisitColonyDesc".Translate(),
                    icon = VisitCommand,
                    hotKey = KeyBindingDefOf.Misc2,
                    action = delegate
                        {
                            // TODO: Settings to limit/unlimited how many colonies can be visited at one time.
                            
                            // TODO: Figure out how to load asynchronously to not lock up game.
                            LongEventHandler.QueueLongEvent(delegate
                                {
                                    Visit(caravan);
                                }, 
                                "FilUnderscore.PersistentRimWorlds.LoadingMap", false, null);
                        }
                };
            }
        }

        /// <summary>
        /// Inspect string that shows up in bottom-left corner of screen when world object is clicked.
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            var inspectString = "";

            inspectString += "Colony: " + this.PersistentColonyData.ColonyFaction.Name;
            
            return inspectString;
        }

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            return base.GetInspectTabs();
        }

        private void Visit(Caravan caravan)
        {
            DynamicMapLoader.LoadMap(this.Tile);
            
            CaravanEnterMapUtility.Enter(caravan, this.Map, x => CellFinder.RandomEdgeCell(this.Map));
        }
        #endregion
    }
}