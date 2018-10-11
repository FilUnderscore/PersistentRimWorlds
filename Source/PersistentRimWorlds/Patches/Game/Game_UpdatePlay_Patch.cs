using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "UpdatePlay")]
    public class Game_UpdatePlay_Patch
    {
        static void Postfix(Game __instance)
        {
            if (!PersistentWorldManager.Active() || PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                return;
            
            PersistentWorldManager.PersistentWorld.UpdateWorld();
        }
    }
}