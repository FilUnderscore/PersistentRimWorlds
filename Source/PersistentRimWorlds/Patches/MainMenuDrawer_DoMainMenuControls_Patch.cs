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
        public static bool drawing;

        static void Prefix()
        {
            drawing = true;
        }

        static void Postfix()
        {
            drawing = false;
        }
    }

    [HarmonyPatch(typeof(OptionListingUtility), nameof(OptionListingUtility.DrawOptionListing))]
    public class MainMenuDrawer_DoMainMenuControls_OptionListingUtility_DrawOptionListing_Patch
    {
        static void Prefix(Rect rect, List<ListableOption> optList)
        {
            if (!MainMenuMarker.drawing)
                return;

            if (Current.ProgramState == ProgramState.Entry)
            {
                int index1;
                if ((index1 = optList.FindIndex(opt => opt.label == "LoadGame".Translate())) != -1)
                {
                    optList.Insert(index1 + 1, new ListableOption("FilUnderscore.PersistentRimWorlds".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Page_PersistentWorlds_LoadWorld_FileList());
                    }));
                }

                return;
            }

            if (Current.ProgramState != ProgramState.Playing ||
                !PersistentWorldManager.GetInstance().PersistentWorldNotNull()) return;
            
            int index;
            if ((index = optList.FindIndex(opt => opt.label == "Save".Translate())) != -1)
            {
                optList.Insert(index,
                    new ListableOption("FilUnderscore.PersistentRimWorlds.Save.World".Translate(),
                        () => { Find.WindowStack.Add(new Dialog_PersistentWorlds_SaveWorld()); }));
                
                optList.RemoveAt(index + 1);
            }

            if ((index = optList.FindIndex(opt => opt.label == "LoadGame".Translate())) != -1)
            {
                optList.RemoveAt(index);
            }
            
            if  (((index = optList.FindIndex(opt => opt.label == "QuitToMainMenu".Translate())) != -1) || (Current.Game.Info.permadeathMode && 
                 (index = optList.FindIndex(opt => opt.label == "SaveAndQuitToMainMenu".Translate())) != -1))
            {
                optList.Insert(index, new ListableOption("SaveAndQuitToMainMenu".Translate(), () =>
                {
                    PersistentWorldManager.GetInstance().PersistentWorld.SaveWorld(GenScene.GoToMainMenu);
                }));
             
                if(Current.Game.Info.permadeathMode)
                    optList.RemoveAt(index + 1);
            }

            if (((index = optList.FindIndex(opt => opt.label == "QuitToOS".Translate())) != -1) || (Current.Game.Info.permadeathMode &&
                (index = optList.FindIndex(opt => opt.label == "SaveAndQuitToOS".Translate())) != -1))
            {
                optList.Insert(index, new ListableOption("SaveAndQuitToOS".Translate(), () =>
                {
                    PersistentWorldManager.GetInstance().PersistentWorld.SaveWorld(GenScene.GoToMainMenu);
                    Root.Shutdown();
                }));
             
                if(Current.Game.Info.permadeathMode)
                    optList.RemoveAt(index + 1);
            }
        }
    }
}