using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_Main : Window
    {
        public Dialog_PersistentWorlds_Main()
        {
            this.doWindowBackground = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            var rect1 = new Rect((inRect.width - 170f) / 2, 0.0f, 170f, inRect.height);

            var optList = new List<ListableOption>();
            
            TooltipHandler.TipRegion(rect1, "Disclaimer: Loading/Saving a persistent world can take a while depending on how many colonies are present.");
            
            optList.Add(new ListableOption("Load-PersistentWorlds".Translate(), delegate
            {
                Find.WindowStack.Add(new Dialog_PersistentWorlds_LoadWorld_FileList());
            }, null));
            
            if (Prefs.DevMode)
            {
                optList.Add(new ListableOption("Dev: Generate World", delegate
                {
                    
                }));
            }
            
            optList.Add(new ListableOption("BackToMenu".Translate(), GenScene.GoToMainMenu));
            
            var num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            GUI.EndGroup();
        }
    }
}