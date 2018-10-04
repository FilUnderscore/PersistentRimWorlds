using System;
using System.Collections.Generic;
using Harmony;
using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Video;
using Verse;

namespace PersistentWorlds.Patches
{
    public static class Debug_Patches
    {
        /*
        [HarmonyPatch(typeof(CrossRefHandler), "Clear")]
        public static class Patch_04
        {
            [HarmonyPrefix]
            public static bool PrefixClear(CrossRefHandler __instance, bool errorIfNotEmpty)
            {
                // TODO: Handle this for multiple file loading... resolving CrossRefHandler Clear() due to calling Scribe.loader.resolveCrossRefs()
                
                if (PersistentWorldManager.LoadColonyIndex == -1) return true;
                
                Log.Message("Cancel Clear.");
                return false;
            }
        }

        [HarmonyPatch(typeof(MapTemperature), "get_OutdoorTemp")]
        public static class Patch_05
        {
            [HarmonyPrefix]
            public static void PrefixTest(MapTemperature __instance)
            {
                if (PersistentWorldManager.LoadColonyIndex == -1) return;
                
                Log.Message("Resolve");
                Scribe.loader.crossRefs.ResolveAllCrossReferences();
            }
        }
        */
    }
}