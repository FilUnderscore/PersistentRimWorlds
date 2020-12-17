using System.Collections.Generic;
using System.Linq;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LeaderPawnSelection : Window
    {
        #region Fields
        private PersistentColony colony;
        #endregion
        
        #region Properties
        public override Vector2 InitialSize => new Vector2(620f, 700f);
        #endregion
        
        #region Constructors
        public Dialog_PersistentWorlds_LeaderPawnSelection(PersistentColony colony)
        {
            this.colony = colony;

            this.doCloseButton = true;
            this.doCloseX = true;
        }
        #endregion

        #region Methods
        public override void PostClose()
        {
            LeaderUI.Reset();
        }

        public override void DoWindowContents(Rect inRect)
        {
            LeaderUI.DrawColonistsMenu(ref inRect, this.Margin, new List<Pawn>(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction), SetLeader);
        }

        private void SetLeader(Pawn pawn)
        {
            this.Close();

            this.colony.ColonyData.Leader = new PersistentColonyLeader(pawn);
        }
        #endregion
    }
}