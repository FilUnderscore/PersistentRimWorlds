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
            if (PersistentWorldManager.WorldLoader == null)
            {
                return true;
            }
            
            PersistentWorldManager.WorldLoader.LoadWorldNow(fileName);

            return false;
        }
    }
}