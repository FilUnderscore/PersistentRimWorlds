using HarmonyLib;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame")]
    public class GameDataSaveLoader_SaveGame_Patch
    {
        #region Methods
        static bool Prefix(string fileName)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting))
            {
                return true;
            }

            PersistentWorldManager.GetInstance().PersistentWorld.LoadSaver.SaveWorld();
            PersistentWorldManager.GetInstance().PersistentWorld.LoadSaver.Status = PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame;
                
            return false;
        }
        #endregion
    }
}