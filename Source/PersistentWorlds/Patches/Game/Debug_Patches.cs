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
    }
}