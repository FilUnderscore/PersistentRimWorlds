using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using Verse;
using PersistentWorlds.Patches;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.World;
using Verse.Profile;

namespace PersistentWorlds.UI
{
    public sealed class Page_PersistentWorlds_LoadWorld_FileList : Page
    {
        #region Fields
        private Vector2 scrollPosition = Vector2.zero;
        
        private List<ScrollableListItem> items = new List<ScrollableListItem>();

        private bool normalClose = true;
        #endregion
        
        #region Properties
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        #endregion
        
        #region Constructors
        public Page_PersistentWorlds_LoadWorld_FileList()
        {
            // TODO: HMM
            PersistentWorldManager.GetInstance().Clear();
            
            this.LoadWorldsAsItems();
            this.LoadPossibleConversions();

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
            if (!normalClose) return;
            
            this.DoBack();
        }

        private void LoadWorldsAsItems()
        {
            // Have a method fetch all world folders in RimWorld save folder in a SaveUtil or something instead of here...
            foreach (var worldDir in Directory.GetDirectories(PersistentWorldLoadSaver.SaveDir))
            {
                var worldDirInfo = new DirectoryInfo(worldDir);

                var scrollableListItem = new ScrollableListItem();

                scrollableListItem.Text = worldDirInfo.Name;
                scrollableListItem.ActionButtonText = "Load".Translate();
                scrollableListItem.ActionButtonAction = delegate
                {
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        normalClose = false;
                        
                        var previousGame = Current.Game;

                        var persistentWorld = new PersistentWorld();
                        persistentWorld.LoadSaver = new PersistentWorldLoadSaver(persistentWorld, worldDirInfo.FullName);

                        // TODO: HMM
                        PersistentWorldManager.GetInstance().PersistentWorld = persistentWorld;
                        
                        persistentWorld.LoadSaver.LoadWorld();
                        
                        Current.Game = previousGame;

                        this.next = new Page_PersistentWorlds_LoadWorld_ColonySelection(persistentWorld) {prev = this};
                        this.DoNext();
                    }, "FilUnderscore.PersistentRimWorlds.LoadingWorld".Translate(), true, null);
                };

                scrollableListItem.DeleteButtonTooltip = "Delete-PersistentWorlds".Translate();
                scrollableListItem.DeleteButtonAction = delegate
                {
                    // TODO: Implement deleting persistent worlds.
                    var dialogBox = new Dialog_MessageBox("DeleteWorld-PersistentWorlds".Translate(), "Delete",
                        delegate
                        {
                            // TODO: Delete persistent world.
                        }, "Cancel", null, "DeleteWorldTitle-PersistentWorlds".Translate(), true)
                    {
                        buttonCText = "Convert",
                        buttonCAction = delegate
                        {
                            // TODO: Convert world back to single colony game.  
                        }
                    };


                    Find.WindowStack.Add(dialogBox);
                };
                
                items.Add(scrollableListItem);
            }
        }

        private void LoadPossibleConversions()
        {
            foreach (var allSavedGameFile in GenFilePaths.AllSavedGameFiles)
            {
                var scrollableListItem = new ScrollableListItem();

                if (SaveFileUtils.HasPossibleSameWorldName(this.items.ToArray(), allSavedGameFile.FullName))
                {
                    continue;
                }
                
                scrollableListItem.Text = Path.GetFileNameWithoutExtension(allSavedGameFile.Name);
                scrollableListItem.ActionButtonText = "Convert".Translate();
                scrollableListItem.ActionButtonAction = delegate
                {
                    normalClose = false;
                    
                    var persistentWorld = new PersistentWorld();

                    persistentWorld.LoadSaver = new PersistentWorldLoadSaver(persistentWorld, allSavedGameFile.FullName)
                        {Status = PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting};
                    
                    PersistentWorldManager.GetInstance().PersistentWorld = persistentWorld;
                    
                    GameDataSaveLoader.LoadGame(Path.GetFileNameWithoutExtension(allSavedGameFile.Name));
                };
                
                items.Add(scrollableListItem);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            ScrollableListUI.DrawList(ref inRect, ref this.scrollPosition, ref this.items);
        }
        #endregion
    }
}