using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame")]
    public class GameDataSaveLoader_SaveGame_Patch
    {
        // TODO: Disallow saving through normal save menu.
        static bool Prefix(string fileName)
        {
            // TODO: Possibly checking if status is not convert instead of ingame.

            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null ||
                PersistentWorldManager.WorldLoadSaver.Status ==
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
            {
                Log.Message((PersistentWorldManager.PersistentWorld == null).ToString());
                Log.Message((PersistentWorldManager.WorldLoadSaver == null).ToString());
                
                if (PersistentWorldManager.WorldLoadSaver != null)
                {
                    Log.Message(PersistentWorldManager.WorldLoadSaver.Status.ToString());
                }
                
                Log.Message("Calling true?");
                return true;
            }

            Log.Message("Calling false?");
            PersistentWorldManager.WorldLoadSaver.SaveWorld(PersistentWorldManager.PersistentWorld);
            PersistentWorldManager.WorldLoadSaver.Status = PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame;
                
            return false;
        }
    }
}