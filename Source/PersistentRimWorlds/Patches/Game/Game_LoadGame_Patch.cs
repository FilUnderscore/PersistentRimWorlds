using Harmony;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "LoadGame")]
    public class Game_LoadGame_Patch
    {
        #region Methods
        static bool Prefix(Game __instance)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Uninitialized || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
            {
                return true;
            }
                
            var persistentWorld = PersistentWorldManager.PersistentWorld;
                
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.LoadingWorld".Translate());
                
            // Unload.
            MemoryUtility.UnloadUnusedUnityAssets();
            Current.ProgramState = ProgramState.MapInitializing;

            Scribe.mode = LoadSaveMode.LoadingVars;

            persistentWorld.LoadGame();
                
            return false;
        }
        #endregion
    }
}