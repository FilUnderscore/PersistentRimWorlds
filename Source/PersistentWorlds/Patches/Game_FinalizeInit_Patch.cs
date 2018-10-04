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
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
                return;

            PersistentWorldManager.WorldLoadSaver.Convert(__instance);
        }
    }
}