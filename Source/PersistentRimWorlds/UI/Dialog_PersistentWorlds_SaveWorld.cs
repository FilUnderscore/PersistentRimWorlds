using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_SaveWorld : Window
    {
        public Dialog_PersistentWorlds_SaveWorld()
        {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }
        
        public override void PostClose()
        {
            
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            WorldUI.DrawWorldSaveList(ref inRect, this.Margin, new List<WorldUI.UIEntry>(), this.OnOverwriteWorld,
                this.OnNewWorld, this.OnDeleteWorld);
        }

        private void OnNewWorld()
        {
            
        }
        
        private void OnOverwriteWorld(string worldPath)
        {
            this.Close();

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            // TODO: Bring up a save dialog that allows the user to save the whole world into a new folder.
        }

        private void OnDeleteWorld(string worldPath)
        {
            
        }
    }
}