using HarmonyLib;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
    public class SavedGameLoaderNow_LoadGameFromSaveFileNow_Patch
    {
        #region Methods
        static bool Prefix(string fileName)
        {
            // Is Persistent World being loaded??
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Finalizing))
            {
                return true;
            }
            
            PersistentWorldManager.GetInstance().PersistentWorld.LoadWorld();
            
            return false;
        }
        #endregion
    }
}