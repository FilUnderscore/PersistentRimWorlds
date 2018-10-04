using System.Collections.Generic;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LoadWorld_ColonySelection : Window
    {
        private string _saveFileName;
        private List<ScrollableListItem> items = new List<ScrollableListItem>();
        
        public override Vector2 InitialSize => new Vector2(600f, 700f);

        private Vector2 scrollPosition = Vector2.zero;
        
        public Dialog_PersistentWorlds_LoadWorld_ColonySelection(string saveFileName)
        {   
            this.doWindowBackground = true;

            this._saveFileName = saveFileName;

            Log.Message("LoadColonySelectionWindow.ctor() assigned PWORLD");
            Log.Message("_saveFileName: " + _saveFileName);
            
            PersistentWorldManager.PersistentWorld = new PersistentWorld();
            Current.Game = null;
            
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
            var colonies = SaveUtils.LoadColonies(this._saveFileName);

            for (var i = 0; i < colonies.Count; i++)
            {
                var colony = colonies[i];
                
                var scrollableListItem = new ScrollableListItem();

                scrollableListItem.Text = "Colony Name Here";
                scrollableListItem.ActionButtonText = "Load";
                scrollableListItem.ActionButtonAction = delegate
                {
                    PersistentWorldManager.LoadColonyIndex = i;
                    PersistentWorldManager.WorldLoader = new PersistentWorldLoader();
                    
                    GameDataSaveLoader.LoadGame(this._saveFileName);
                };
                
                scrollableListItem.DeleteButtonAction = delegate
                {
                    
                };
                scrollableListItem.DeleteButtonTooltip =
                    "Delete this colony permanently. It will disappear from the world map without any trace.";
                
                items.Add(scrollableListItem);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            Rect rect1 = new Rect((inRect.width - 170f) / 2, 0.0f, 170f, inRect.height);

            List<ListableOption> optList = new List<ListableOption>();

            optList.Add(new ListableOption("New Colony", delegate
            {
                // TODO: Have normal creation menus without creating world choice however... include scenario and characters and storyteller, as well as world location.
                // TODO: Have other colonies on same world tiles loaded as a settlement with different color as a whole new worldobject that shows up in Colonies tab on world map.
                Find.WindowStack.Add((Window) new Page_SelectScenario());
            }));
            
            //optList.Add(new ListableOption("Back to Menu", delegate { Find.WindowStack.TryRemove(this); }));
            
            double num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            Log.Message("Num1: " + num1.ToString());
            
            var rect2 = new Rect(0, (float) num1, inRect.width, inRect.height);
            
            GUI.BeginGroup(rect2);
            ScrollableListUI.DrawList(ref rect2, ref this.scrollPosition, this.items.ToArray());
            GUI.EndGroup();
            
            GUI.EndGroup();
        }
    }
}