using System;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace PersistentWorlds.UI
{
    public sealed class Dialog_PersistentWorlds_NameWorld : Window
    {
        private readonly Action<string> onName;

        private string curName;

        public override Vector2 InitialSize => new Vector2(640f, 160f);

        public Dialog_PersistentWorlds_NameWorld(Action<string> onName)
        {
            this.onName = onName;
        }
        
        private bool IsValidName(string s)
        {
            if (SaveFileUtils.WorldWithNameExists(s))
            {
                Messages.Message("FilUnderscore.PersistentRimWorlds.Save.WorldNameAlreadyUsed".Translate(),
                    MessageTypeDefOf.RejectInput, false);
                
                return false;
            }
            else if (!GenText.IsValidFilename(s) || GrammarResolver.ContainsSpecialChars(s))
            {
                // TODO: Invalid chars message.      

                return false;
            }

            return true;
        }

        // TODO: Redo this method?
        public override void DoWindowContents(Rect inRect)
        {
            this.curName = Widgets.TextField(new Rect(0.0f, 80f, (float) ((double)inRect.width / 2.0 + 70.0), 35f), this.curName);

            Widgets.Label(new Rect(0.0f, 0.0f, inRect.width, inRect.height),
                "FilUnderscore.PersistentRimWorlds.Save.NewWorld.Desc".Translate());
            
            var buttonRect = new Rect((float) (inRect.width / 2.0 + 90.0), inRect.height - 35f,
                (float)(inRect.width / 2.0 - 90.0), 35f);

            if (!Widgets.ButtonText(buttonRect, "OK".Translate()))
                return;

            if (IsValidName(this.curName))
            {
                onName?.Invoke(this.curName);
            }
        }
    }
}