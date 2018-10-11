using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public class Game_FinalizeInit_Patch
    {
        static void Postfix(Game __instance)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
                return;

            PersistentWorldManager.WorldLoadSaver.Convert(__instance);
        }
    }
}