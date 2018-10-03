using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Harmony;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.World
{
    public static class PersistentWorldConverter
    {
        private static string worldName = null;

        public static void PrepareConvert(string worldName)
        {
            PersistentWorldConverter.worldName = worldName;
        }

        public static void Convert(Game __instance)
        {
            Log.Message("Convert to PersistentWorld...");

            worldName = __instance.World.info.name;
            
            LongEventHandler.QueueLongEvent(delegate
            {
                // TODO: Convert World to Persistent...
                ConvertWorld(__instance);
                
                // Jump back to main menu and alert user that conversion is complete.
                //GenScene.GoToMainMenu();
                
                //Find.WindowStack.Add(new Dialog_MessageBox("ConversionComplete-PersistentWorlds".Translate(__instance.World.info.name)));
            }, "ConvertingWorld-PersistentWorlds", true, null);
        }

        public static void ConvertWorld(Game __instance)
        {
            SaveUtils.ConvertAndSaveWorld(__instance);
        }

        public static bool Converting()
        {
            return worldName != null;
        }
    }
}