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

        private string nameInt;
        
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
                    this.cachedMat = MaterialPool.MatFrom(
                        this.PersistentColonyData?.ColonyFaction == null
                            ? FactionDefOf.PlayerColony.homeIconPath
                            : this.PersistentColonyData.ColonyFaction.def.homeIconPath,
                        ShaderDatabase.WorldOverlayTransparentLit, this.PersistentColonyData?.Color ?? Color.green,
                        WorldMaterials.WorldObjectRenderQueue);

                return this.cachedMat;
            }
        }

        public override Texture2D ExpandingIcon => ContentFinder<Texture2D>.Get(
            this.PersistentColonyData?.ColonyFaction == null
                ? FactionDefOf.PlayerColony.expandingIconTexture
                : this.PersistentColonyData.ColonyFaction.def.expandingIconTexture);

        public string Name 
        { 
            get => nameInt;
            set => nameInt = value;
        }

        public override string Label => nameInt ?? base.Label;

        public override bool HasName => !nameInt.NullOrEmpty();
        #endregion
        
        #region Methods
        /// <summary>
        /// Saving/loading of colony world object instance.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look(ref nameInt, "name");
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
                    defaultLabel = "FilUnderscore.PersistentRimWorlds.Command.VisitColony".Translate(),
                    defaultDesc = "FilUnderscore.PersistentRimWorlds.Command.VisitColony.Desc".Translate(),
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
                                "FilUnderscore.PersistentRimWorlds.Loading.Map", false, null);
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