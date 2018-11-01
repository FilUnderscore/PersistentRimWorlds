using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public sealed class Page_PersistentWorlds_Main : Page
    {
        #region Properties
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        #endregion        
        
        #region Constructors
        public Page_PersistentWorlds_Main()
        {
            this.doWindowBackground = true;
            this.doCloseButton = true;
            this.doCloseX = true;
        }
        #endregion

        #region Methods
        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.width / 2 - inRect.width / 6f, 0.0f, inRect.width, 45f), "FilUnderscore.PersistentRimWorlds".Translate());
            Text.Font = GameFont.Small;
            
            var rect1 = new Rect((inRect.width - 170f) / 2, 0.0f + 45f, 170f, inRect.height);

            var optList = new List<ListableOption>
            {
                new ListableOption("FilUnderscore.PersistentRimWorlds.Load.World".Translate(),
                    delegate
                    {
                        this.next = new Page_PersistentWorlds_LoadWorld_FileList {prev = this};
                        this.DoNext();
                    }, null),
            };

            var num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            GUI.EndGroup();
        }
        #endregion
    }
}