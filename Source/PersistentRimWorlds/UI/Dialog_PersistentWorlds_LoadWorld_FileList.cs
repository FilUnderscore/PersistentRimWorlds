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
using PersistentWorlds.World;
using Verse.Profile;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LoadWorld_FileList : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        
        private List<ScrollableListItem> items = new List<ScrollableListItem>();
        
        public override Vector2 InitialSize => new Vector2(600f, 700f);

        public Dialog_PersistentWorlds_LoadWorld_FileList()
        {
            this.LoadWorldsAsItems();
            this.LoadPossibleConversions();

            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
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
                        var previousGame = Current.Game;
                    
                        PersistentWorldManager.WorldLoadSaver = new PersistentWorldLoadSaver(worldDirInfo.FullName);
                        PersistentWorldManager.WorldLoadSaver.LoadWorld();

                        Current.Game = previousGame;
                    
                        Find.WindowStack.Add(new Dialog_PersistentWorlds_LoadWorld_ColonySelection());
                    }, "FilUnderscore.PersistentRimWorlds.LoadingWorld".Translate(), true, null);
                };

                scrollableListItem.DeleteButtonTooltip = "Delete-PersistentWorlds".Translate();
                scrollableListItem.DeleteButtonAction = delegate
                {
                    // TODO: Implement deleting persistent worlds.
                    var dialogBox = new Dialog_MessageBox("DeleteWorld-PersistentWorlds".Translate(), "Delete",
                        delegate { }, "Cancel", delegate { }, "DeleteWorldTitle-PersistentWorlds".Translate(), true)
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
                    PersistentWorldManager.WorldLoadSaver = new PersistentWorldLoadSaver(allSavedGameFile.FullName);
                    PersistentWorldManager.WorldLoadSaver.Status =
                        PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting;
                    
                    GameDataSaveLoader.LoadGame(Path.GetFileNameWithoutExtension(allSavedGameFile.Name));
                };
                
                items.Add(scrollableListItem);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            ScrollableListUI.DrawList(ref inRect, ref this.scrollPosition, ref this.items);
        }
    }
}