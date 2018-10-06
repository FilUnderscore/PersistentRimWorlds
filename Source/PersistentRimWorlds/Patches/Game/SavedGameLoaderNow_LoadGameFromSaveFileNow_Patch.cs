using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
    public static class SavedGameLoaderNow_LoadGameFromSaveFileNow_Patch
    {
        [HarmonyPrefix]
        public static bool LoadGameFromSaveFileNow_Prefix(string fileName)
        {
            // Is Persistent World being loaded??
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status != PersistentWorldLoadSaver.PersistentWorldLoadStatus.Finalizing)
            {
                return true;
            }
            
            PersistentWorldManager.PersistentWorld.LoadWorld();
            
            return false;
        }
    }
}