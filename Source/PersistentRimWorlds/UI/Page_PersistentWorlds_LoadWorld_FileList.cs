using System.Collections.Generic;
using System.IO;
using System.Threading;
using Harmony;
using PersistentWorlds;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using Verse;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.UI;

namespace PersistentWorlds.UI
{
    public sealed class Page_PersistentWorlds_LoadWorld_FileList : Page
    {   
        #region Fields
        private bool normalClose = true;

        private readonly List<WorldUI.UIEntry> worldEntries = new List<WorldUI.UIEntry>();
        private readonly List<WorldUI.UIEntry> saveGameEntries = new List<WorldUI.UIEntry>();
        #endregion
        
        #region Properties
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        #endregion
        
        #region Constructors
        public Page_PersistentWorlds_LoadWorld_FileList()
        {
            // TODO: HMM
            PersistentWorldManager.GetInstance().Clear();

            // Multi thread to decrease loading times.
            new Thread(() =>
            {
                this.LoadWorldsAsItems();

                this.LoadPossibleConversions();
            }).Start();
            
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }
        #endregion

        #region Methods
        public override void PostClose()
        {
            WorldUI.Reset();
            
            if (!normalClose) return;
            
            this.DoBack();
        }

        private void LoadWorldsAsItems()
        {
            // Have a method fetch all world folders in RimWorld save folder in a SaveUtil or something instead of here...
            foreach (var worldDir in Directory.GetDirectories(PersistentWorldLoadSaver.SaveDir))
            {
                var worldDirInfo = new DirectoryInfo(worldDir);
                
                worldEntries.Add(new WorldUI.WorldUIEntry(worldDirInfo));
            }
        }

        private void LoadPossibleConversions()
        {
            var names = new List<string>();
            worldEntries.Do(entry => names.Add(((WorldUI.WorldUIEntry) entry).Name));
            var namesArray = names.ToArray();
            
            foreach (var allSavedGameFile in GenFilePaths.AllSavedGameFiles)
            {
                if (SaveFileUtils.HasPossibleSameWorldName(namesArray, allSavedGameFile.FullName))
                    continue;
                
                saveGameEntries.Add(new WorldUI.SaveGameUIEntry(allSavedGameFile.FullName));
            }
            
            names.Clear();
        }

        public override void DoWindowContents(Rect inRect)
        {
            WorldUI.DrawWorldList(ref inRect, this.Margin, this.worldEntries, this.saveGameEntries, this.LoadWorld, this.DeleteWorld, this.ConvertWorld);
        }

        private void LoadWorld(string worldDir)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                normalClose = false;
                        
                var previousGame = Current.Game;

                var persistentWorld = new PersistentWorld();
                persistentWorld.LoadSaver = new PersistentWorldLoadSaver(persistentWorld, worldDir);

                // TODO: HMM
                PersistentWorldManager.GetInstance().PersistentWorld = persistentWorld;
                        
                persistentWorld.LoadSaver.LoadWorld();
                        
                Current.Game = previousGame;
                        
                this.next = new Page_PersistentWorlds_LoadWorld_ColonySelection(persistentWorld) {prev = this};
                this.DoNext();
            }, "FilUnderscore.PersistentRimWorlds.LoadingWorld", true, null);
        }
        
        private void DeleteWorld(string worldDir)
        {
            var dialogBox = new Dialog_MessageBox("FilUnderscore.PersistentRimWorlds.DeleteWorldDesc".Translate(worldDir), "Delete".Translate(),
                delegate
                {
                    // TODO: Delete persistent world.
                }, "FilUnderscore.PersistentRimWorlds.Cancel".Translate(), null, "FilUnderscore.PersistentRimWorlds.DeleteWorld".Translate(), true)
            {
                buttonCText = "FilUnderscore.PersistentRimWorlds.ConvertWorld".Translate(),
                buttonCAction = delegate
                {
                    // TODO: Convert world back to single colony game.  
                }
            };


            Find.WindowStack.Add(dialogBox);
        }
        
        private void ConvertWorld(string filePath)
        {
            normalClose = false;

            var prevGame = Current.Game; // Fix UIRoot_Entry error.

            var persistentWorld = new PersistentWorld();

            Current.Game = prevGame; // Fix UIRoot_Entry error.
                    
            persistentWorld.LoadSaver = new PersistentWorldLoadSaver(persistentWorld, filePath)
                {Status = PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting};
                    
            PersistentWorldManager.GetInstance().PersistentWorld = persistentWorld;
                    
            GameDataSaveLoader.LoadGame(Path.GetFileNameWithoutExtension(filePath));
        }
        #endregion
    }
}