using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;
using JetBrains.Annotations;
using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Video;
using Verse;

namespace PersistentWorlds.Patches
{
    public static class Debug_Patches
    {
        [HarmonyPatch(typeof(MapDrawer), "SectionAt")]
        public static class MapDrawer_SectionAt_Patch
        {
            [HarmonyPrefix]
            public static void Prefix_Patch(MapDrawer __instance, IntVec3 loc, Section __result)
            {
                //Log.Message("Patch");
                
                //__instance.RegenerateEverythingNow();
            }
        }
    }
}