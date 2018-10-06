using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame")]
    public static class GameDataSaveLoader_SaveGame_Patch
    {
        // TODO: Disallow saving through normal save menu.
        [HarmonyPrefix]
        public static bool SaveGame_Prefix(string fileName)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null ||
                PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                return true;
                
            PersistentWorldManager.WorldLoadSaver.SaveWorld(PersistentWorldManager.PersistentWorld);
                
            return false;
        }
    }
}