using System;
using System.Collections.Generic;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Video;
using Verse;

namespace PersistentWorlds.Patches
{
    public static class Debug_Patches
    {
        [HarmonyPatch(typeof(Storyteller), "MakeIncidentsForInterval")]
        public static class Storyteller_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(Storyteller __instance)
            {
                Log.Warning("Storyteller Injection Patch Debug");
                
                if (__instance.AllIncidentTargets == null)
                {
                    Log.Error("Error with AllIncidentTargets");
                }

                if (__instance.storytellerComps == null)
                {
                    Log.Error("Storyteller comps is null.");
                }
            }
        }

        [HarmonyPatch(typeof(StoryWatcher_Adaptation), "get_TotalThreatPointsFactor")]
        public static class StoryWatcher_Patch
        {
            [HarmonyPrefix]
            public static void PrefixPatch(StoryWatcher_Adaptation __instance)
            {
                Log.Message("R1");
                
                var sDef = AccessTools.Property(typeof(StoryWatcher_Adaptation), "StorytellerDef");

                Log.Message("R2");
                
                if (sDef.GetValue(__instance, null) == null)
                {
                    Log.Error("Storyteller Def is null.");
                }
                
                Log.Message("R3");

                if (Find.Storyteller == null)
                {
                    Log.Error("Storyteller is null of Current.Game");
                }

                Log.Message("R4");
                
                if (Find.Storyteller.def == null)
                {
                    Log.Error("Storyteller def is null.");
                }
                
                Log.Message("R5");
            }
        }
    }
}