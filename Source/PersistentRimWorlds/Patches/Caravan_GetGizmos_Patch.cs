using System.Collections.Generic;
using Harmony;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Caravan), "GetGizmos")]
    public static class Caravan_GetGizmos_Patch
    {
        [HarmonyPostfix]
        public static void GetGizmos_Postfix(ref IEnumerable<Gizmo> __result, Caravan __instance)
        {
            if (Find.WorldObjects.AnyWorldObjectAt(__instance.Tile, WorldObjectDefOf.AbandonedSettlement))
            {
                var gizmos = new List<Gizmo>(__result);

                gizmos.RemoveAll(g => g.GetType() == typeof(Command_Settle));
                gizmos.Insert(0, new BaseResettleCommand(__instance));
                    
                __result = gizmos;
            }
        }
    }
}