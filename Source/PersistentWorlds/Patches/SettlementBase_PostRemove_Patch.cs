using Harmony;
using RimWorld.Planet;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(SettlementBase), "PostRemove")]
    public static class SettlementBase_PostRemove_Patch
    {
        [HarmonyPrefix]
        public static bool PostRemove_Prefix(SettlementBase __instance)
        {
            if (__instance.Map.IsPlayerHome)
            {
                // TODO: Delete map but store all player structures, and items, as the map always generates the same due to the world seed and tile size set in world settings...
                return false;
            }

            return true;
        }
    }
}