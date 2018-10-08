﻿using System.Collections.Generic;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LoadWorld_ColonySelection : Window
    {        
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");

        private List<ScrollableListItem> items = new List<ScrollableListItem>();
        
        public override Vector2 InitialSize => new Vector2(600f, 700f);

        private Vector2 scrollPosition = Vector2.zero;
        
        public Dialog_PersistentWorlds_LoadWorld_ColonySelection()
        {   
            this.LoadColoniesAsItems();

            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }

        public override void PreOpen()
        {
            this.SetInitialSizeAndPosition();
        }

        private void LoadColoniesAsItems()
        {
            PersistentWorldManager.WorldLoadSaver.LoadColonies();
            
            for (var i = 0; i < PersistentWorldManager.PersistentWorld.Colonies.Count; i++)
            {
                var colony = PersistentWorldManager.PersistentWorld.Colonies[i];
                
                var scrollableListItem = new ScrollableListItem();

                scrollableListItem.Text = colony.ColonyData.ColonyFaction.Name;
                scrollableListItem.ActionButtonText = "Load".Translate();
                scrollableListItem.ActionButtonAction = delegate
                    {
                        PersistentWorldManager.PersistentWorld.Colony = colony;
                        
                        // This line cause UIRoot_Play to throw one error due to null world/maps, can be patched to check if null before running.
                        MemoryUtility.ClearAllMapsAndWorld();

                        PersistentWorldManager.PersistentWorld.PatchPlayerFaction();
                        PersistentWorldManager.WorldLoadSaver.TransferToPlayScene();
                    };
                
                scrollableListItem.DeleteButtonAction = delegate
                {
                    // TODO: Allow colonies to be deleted.   
                };
                scrollableListItem.DeleteButtonTooltip =
                    "DeleteColony-PersistentWorlds".Translate();

                scrollableListItem.canChangeColor = true;
                scrollableListItem.texture = Town; 
                
                // TODO: Show date/time of file save.
                
                items.Add(scrollableListItem);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            Rect rect1 = new Rect((inRect.width - 170f) / 2, 0.0f, 170f, inRect.height);

            List<ListableOption> optList = new List<ListableOption>();

            optList.Add(new ListableOption("NewColony".Translate(), delegate
            {
                // TODO: Have normal creation menus without creating world choice however... include scenario and characters and storyteller, as well as world location.
                // TODO: Have other colonies on same world tiles loaded as a settlement with different color as a whole new worldobject that shows up in Colonies tab on world map.
                Find.WindowStack.Add((Window) new Page_SelectScenario());
            }));
            
            double num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            var rect2 = new Rect(0, (float) num1, inRect.width, inRect.height);
            
            GUI.BeginGroup(rect2);
            ScrollableListUI.DrawList(ref rect2, ref this.scrollPosition, ref this.items);
            GUI.EndGroup();
            
            GUI.EndGroup();
        }
    }
}