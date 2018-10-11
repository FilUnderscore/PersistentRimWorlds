using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
    public class SavedGameLoaderNow_LoadGameFromSaveFileNow_Patch
    {
        static bool Prefix(string fileName)
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