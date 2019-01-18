using System.Collections.Generic;
using System.Linq;
using PersistentWorlds.Logic;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LeaderPawnSelection : Window
    {
        private PersistentColony colony;
        
        public Dialog_PersistentWorlds_LeaderPawnSelection(PersistentColony colony)
        {
            this.colony = colony;
        }

        public override void PostClose()
        {
            LeaderUI.Reset();
        }

        public override void DoWindowContents(Rect inRect)
        {
            LeaderUI.DrawColonistsMenu(ref inRect, this.Margin, new List<Pawn>(Find.WorldPawns.AllPawnsAlive.Where(pawn => pawn.IsColonist)));
        }
    }
}