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
            foreach (var worldDir in Directory.GetDirectories(SaveUtils.SaveDir))
            {
                var worldDirInfo = new DirectoryInfo(worldDir);

                var scrollableListItem = new ScrollableListItem();

                scrollableListItem.Text = worldDirInfo.Name;
                scrollableListItem.ActionButtonText = "Load";
                scrollableListItem.ActionButtonAction = delegate
                {
                    // TODO: Load world...
                    Find.WindowStack.Add(new Dialog_PersistentWorlds_LoadWorld_ColonySelection(worldDirInfo.Name));
                };

                scrollableListItem.DeleteButtonTooltip = "Delete persistent world.";
                scrollableListItem.DeleteButtonAction = delegate
                {
                    // TODO: Remove directory.
                };
                
                // TODO: Check if is a persistent world...
                
                items.Add(scrollableListItem);
            }
        }

        private void LoadPossibleConversions()
        {
            foreach (var allSavedGameFile in GenFilePaths.AllSavedGameFiles)
            {
                var scrollableListItem = new ScrollableListItem();

                scrollableListItem.Text = Path.GetFileNameWithoutExtension(allSavedGameFile.Name);
                scrollableListItem.ActionButtonText = "Convert";
                scrollableListItem.ActionButtonAction = delegate
                {
                    // TODO: Launch converter...
                };
                
                items.Add(scrollableListItem);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            ScrollableListUI.DrawList(ref inRect, ref this.scrollPosition, this.items.ToArray());
        }
    }
}