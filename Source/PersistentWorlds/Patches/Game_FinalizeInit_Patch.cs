using Harmony;
using PersistentWorlds.World;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public static class Game_FinalizeInit_Patch
    {
        [HarmonyPostfix]
        public static void FinalizeInit_Postfix(Game __instance)
        {
            if (!PersistentWorldConverter.Converting() || PersistentWorldManager.PersistentWorld != null) return;
                
            PersistentWorldConverter.Convert(__instance);
        }
    }
}