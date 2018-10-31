using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_SaveColony : Window
    {
        public override void DoWindowContents(Rect inRect)
        {
            ColonyUI.DrawColoniesSaveList(ref inRect, this.Margin,
                PersistentWorldManager.GetInstance().PersistentWorld.Colonies, this.OnClick);
        }

        private void OnClick(int index)
        {
            
        }
    }
}