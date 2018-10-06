using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "UpdatePlay")]
    public static class Game_UpdatePlay_Patch
    {
        [HarmonyPostfix]
        public static void UpdatePlay_Postfix(Game __instance)
        {
            if (!PersistentWorldManager.Active() || PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                return;
            
            PersistentWorldManager.PersistentWorld.UpdateWorld();
        }
    }
}