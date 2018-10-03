using System.Collections.Generic;
using PersistentWorlds.Logic;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LoadWorld_ColonySelection : Window
    {
        private string _saveFileName;
        private List<PersistentColony> _colonies;
        
        public Dialog_PersistentWorlds_LoadWorld_ColonySelection(string saveFileName)
        {   
            this.doWindowBackground = true;

            this._saveFileName = saveFileName;

            this._colonies = SaveUtils.LoadColonies(this._saveFileName);
        }

        public override Vector2 InitialSize => new Vector2(600f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            Rect rect1 = new Rect((inRect.width - 170f) / 2, 0.0f, 170f, inRect.height);

            List<ListableOption> optList = new List<ListableOption>();

            for (var i = 0; i < _colonies.Count; i++)
            {
                optList.Add(new ListableOption("Colony Index: " + i.ToString(), delegate
                {
                    PersistentWorldManager.LoadColonyIndex = i;
                    PersistentWorldManager.WorldLoader = new PersistentWorldLoader();
                    
                    GameDataSaveLoader.LoadGame(this._saveFileName);
                }));
            }
            
            optList.Add(new ListableOption("Back to Menu", delegate { Find.WindowStack.TryRemove(this); }));
            
            double num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            GUI.EndGroup();
        }
    }
}