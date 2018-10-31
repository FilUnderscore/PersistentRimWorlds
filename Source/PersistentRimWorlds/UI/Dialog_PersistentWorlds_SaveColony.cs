using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_SaveColony : Window
    {
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