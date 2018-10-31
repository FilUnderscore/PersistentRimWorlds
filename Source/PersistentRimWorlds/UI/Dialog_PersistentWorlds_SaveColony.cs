using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_SaveColony : Window
    {
        public Dialog_PersistentWorlds_SaveColony()
        {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }
        
        public override void PostClose()
        {
            ColonyUI.Reset();
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            ColonyUI.DrawColoniesList(ref inRect, this.Margin, this.CloseButSize,
                PersistentWorldManager.GetInstance().PersistentWorld.Colonies, this.OnClick);
        }

        private void OnClick(int index)
        {
            
        }
    }
}