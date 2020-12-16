using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.UI;
using RimWorld;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using Verse;
using Verse.Sound;
using FileLog = PersistentWorlds.Utils.FileLog;

namespace PersistentWorlds.Patches
{
    // Some different implementation from Parexy's Multiplayer Mod - saves the hassle of transpiling.
    [HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoMainMenuControls))]
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

            if (PersistentWorldWidgets.ButtonImageOn(rect1, WorldIcon))
                PersistentWorldsMod.MainMenuButtonDelegate.DynamicInvoke();

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

            /*
             * DEPRECATED - Removed in the next release
             * 
            if(Current.ProgramState == ProgramState.Entry)
            {
                int index;
                if ((index = optList.FindIndex(opt => opt.label == "LoadGame".Translate())) != -1)
                {
                    optList.Insert(index + 1, new ListableOption("FilUnderscore.PersistentRimWorlds".Translate(), (Action) PersistentWorldsMod.MainMenuButtonDelegate));
                }
            }
            else 
            */
            if(Current.ProgramState == ProgramState.Playing)
            {
                if (PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                {
                    int index;
                    if ((index = optList.FindIndex(opt => opt.label == "Save".Translate())) != -1)
                    {
                        optList.Insert(index, new ListableOption("FilUnderscore.PersistentRimWorlds.Save.World".Translate(), (Action)PersistentWorldsMod.SaveMenuButtonDelegate));
                        optList.RemoveAt(index + 1);
                    }
                }
            }
        }
    }
}