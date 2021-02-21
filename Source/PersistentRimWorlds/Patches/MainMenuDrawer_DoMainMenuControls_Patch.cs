using System;
using System.Collections.Generic;
using HarmonyLib;
using PersistentWorlds.UI;
using PersistentWorlds.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Patches
{
    // Some different implementation from Parexy's Multiplayer Mod - saves the hassle of transpiling.
    [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoMainMenuControls))]
    [StaticConstructorOnStartup]
    public class MainMenuMarker
    {
        private static readonly Texture2D WorldIcon = ContentFinder<Texture2D>.Get("UI/WorldIcon");
        
        public static bool drawing;

        static void Prefix()
        {
            drawing = true;
        }

        static void Postfix()
        {
            drawing = false;

            Vector2 size = new Vector2(WorldIcon.width / 16f, WorldIcon.height / 16f);

            Rect rect = new Rect((Verse.UI.screenWidth - size.x) * 0.99675f, (Verse.UI.screenHeight - size.y) * 0.99f, size.x, size.y);

            GUI.BeginGroup(rect);
            Rect rect1 = new Rect(0, 0, size.x, size.y);

            if (WidgetExtensions.ButtonImageOn(rect1, WorldIcon))
                Find.WindowStack.Add(new Page_PersistentWorlds_LoadWorld_FileList());
            
            TooltipHandler.TipRegion(rect1, "FilUnderscore.PersistentRimWorlds".Translate());

            GUI.EndGroup();
        }
    }

    [HarmonyPatch(typeof(OptionListingUtility), nameof(OptionListingUtility.DrawOptionListing))]
    public class MainMenuDrawer_DoMainMenuControls_OptionListingUtility_DrawOptionListing_Patch
    {
        static void Prefix(Rect rect, List<ListableOption> optList)
        {
            if (!MainMenuMarker.drawing)
                return;

            if (Current.ProgramState != ProgramState.Playing ||
                !PersistentWorldManager.GetInstance().PersistentWorldNotNull()) return;
            
            int index;
            if ((index = optList.FindIndex(opt => opt.label == "Save".Translate())) == -1) return;
                
            optList.Insert(index,
                new ListableOption("FilUnderscore.PersistentRimWorlds.Save.World".Translate(),
                    () =>
                    {
                        Find.WindowStack.Add(new Dialog_PersistentWorlds_SaveWorld());
                    }));
            optList.RemoveAt(index + 1);
        }
    }
}