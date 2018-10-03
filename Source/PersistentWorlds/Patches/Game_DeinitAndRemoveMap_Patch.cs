using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "DeinitAndRemoveMap")]
    public static class Game_DeinitAndRemoveMap_Patch
    {
        [HarmonyPrefix]
        public static bool DeinitAndRemoveMap_Prefix(Game __instance, Map map)
        {
            if (map.IsPlayerHome)
            {
                Log.Message("Cancel Deinit and Remove.");
                    
                return false;
            }

            return true;
        }
    }
}