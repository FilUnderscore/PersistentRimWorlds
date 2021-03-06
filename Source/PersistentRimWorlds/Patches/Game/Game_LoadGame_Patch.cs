using HarmonyLib;
using PersistentWorlds.SaveAndLoad;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "LoadGame")]
    public class Game_LoadGame_Patch
    {
        #region Methods
        static bool Prefix()
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting))
            {
                return true;
            }
                
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
                
            // Unload.
            MemoryUtility.UnloadUnusedUnityAssets();
            Current.ProgramState = ProgramState.MapInitializing;

            persistentWorld.LoadGame();
                
            return false;
        }
        #endregion
    }
}