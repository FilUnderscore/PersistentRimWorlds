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
            this.doCloseButton = true;
            this.doCloseX = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.width / 2 - inRect.width / 6f, 0.0f, inRect.width, 45f), "PersistentRimWorlds".Translate());
            Text.Font = GameFont.Small;
            
            var rect1 = new Rect((inRect.width - 170f) / 2, 0.0f + 45f, 170f, inRect.height);

            var optList = new List<ListableOption>
            {
                new ListableOption("Load-PersistentWorlds".Translate(),
                    delegate { Find.WindowStack.Add(new Dialog_PersistentWorlds_LoadWorld_FileList()); }, null),
                
                // TODO: Temp, color picker.
                new ListableOption("Color Picker Test", delegate
                {
                    Find.WindowStack.Add(new Dialog_ColorPicker());
                }, null)
            };

            var num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            GUI.EndGroup();
        }
    }
}