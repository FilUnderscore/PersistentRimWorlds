using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "DeinitAndRemoveMap")]
    public class Game_DeinitAndRemoveMap_Patch
    {
        static bool Prefix(Game __instance, Map map)
        {
            // TODO: Patch. Can use map.isPlayerHome()
            
            return true;
        }
    }
}