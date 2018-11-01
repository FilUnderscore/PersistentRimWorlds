using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_SaveWorld : Window
    {
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        
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
            WorldUI.Reset();
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            WorldUI.DrawWorldSaveList(ref inRect, this.Margin, new List<WorldUI.UIEntry>(), this.OnOverwriteWorld,
                this.OnNewWorld, this.OnDeleteWorld);
        }

        private void OnNewWorld()
        {
            this.Close();
        }
        
        private void OnOverwriteWorld(string worldPath)
        {
            this.Close();
        }

        private void OnDeleteWorld(string worldPath)
        {
            this.Close();
        }
    }
}