using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    // TODO: Full refactor of all UI classes involved with saving.
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
            WorldUI.DrawWorldSaveList(ref inRect, this.Margin, this.CloseButSize, this.worldEntries, this.SaveWorld,
                this.OnNewWorld, this.ShowDeleteWorldDialog);
        }

        private void OnNewWorld()
        {
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            var nameWorldDialog = new Dialog_PersistentWorlds_NameWorld((name) =>
            {
                var worldDir = SaveFileUtils.Clone(persistentWorld.LoadSaver.GetWorldFolderPath(),
                    PersistentWorldLoadSaver.SaveDir + "/" + name);
                
                // Delete original world file.
                worldDir.GetFiles("*.pwf").Do(file => file.Delete());
                
                // Change original world name.
                persistentWorld.WorldData.Info.name = name;
                
                persistentWorld.LoadSaver =
                    new PersistentWorldLoadSaver(persistentWorld, worldDir.FullName);

                this.SaveWorld();

                this.LoadWorldsAsEntries();
            });
            
            Find.WindowStack.Add(nameWorldDialog);
        }

        private void SaveWorld()
        {
            this.Close();
            
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            LongEventHandler.QueueLongEvent(delegate
            {
                persistentWorld.SaveColony();
                    
                persistentWorld.ConvertCurrentGameWorldObjects();
                
                persistentWorld.LoadSaver.SaveWorldData();
                    
                persistentWorld.ConvertToCurrentGameWorldObjects();
            }, "FilUnderscore.PersistentRimWorlds.Saving.World", false, null);
        }
        
        private void ShowDeleteWorldDialog(string worldPath)
        {
            WorldUI.ShowDeleteWorldDialog(worldPath, this.DeleteWorld);
        }

        private void DeleteWorld(string worldPath)
        {
            SaveFileUtils.DeleteDirectory(worldPath);
            
            this.LoadWorldsAsEntries();
        }
    }
}