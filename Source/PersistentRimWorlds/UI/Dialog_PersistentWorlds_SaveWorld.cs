using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading;
using PersistentWorlds.SaveAndLoad;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_SaveWorld : Window
    {
        private readonly List<WorldUI.UIEntry> worldEntries = new List<WorldUI.UIEntry>();
        
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        
        public Dialog_PersistentWorlds_SaveWorld()
        {
            this.LoadWorldsAsEntries();
            
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }

        private void LoadWorldsAsEntries()
        {
            this.worldEntries.Clear();
            
            new Thread(() =>
            {
                foreach (var entry in SaveFileUtils.LoadWorldEntries())
                {
                    this.worldEntries.Add(entry);
                }
            }).Start();
        }
        
        public override void PostClose()
        {
            WorldUI.Reset();
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            WorldUI.DrawWorldSaveList(ref inRect, this.Margin, this.CloseButSize, this.worldEntries, this.OnOverwriteWorld,
                this.OnNewWorld, this.OnDeleteWorld);
        }

        private void OnNewWorld()
        {
            this.Close();
            
            this.LoadWorldsAsEntries();
        }
        
        private void OnOverwriteWorld(string worldPath, bool isCurrentWorld)
        {
            this.Close();
            
            this.LoadWorldsAsEntries();
        }

        private void OnDeleteWorld(string worldPath)
        {
            WorldUI.ShowDeleteWorldDialog(worldPath);
            
            this.LoadWorldsAsEntries();
        }
    }
}