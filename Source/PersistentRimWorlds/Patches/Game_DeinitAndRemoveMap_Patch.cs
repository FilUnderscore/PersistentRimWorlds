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
            // TODO: Review
            
            /*
            if (map.IsPlayerHome)
            {
                return false;
            }
            */

            return true;
        }
    }
}