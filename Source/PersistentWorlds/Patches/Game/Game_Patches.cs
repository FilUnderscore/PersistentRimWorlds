using Harmony;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.Patches
{
    public class Game_Patches
    {
        [HarmonyPatch(typeof(Game), "LoadGame")]
        public static class Game_LoadGame_Patch
        {
            [HarmonyPrefix]
            public static bool LoadGame_Prefix(Game __instance)
            {
                if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Uninitialized)
                {
                    return true;
                }
                
                var persistentWorld = PersistentWorldManager.PersistentWorld;
                
                LongEventHandler.SetCurrentEventText("LoadingPersistentWorld".Translate());
                
                // Unload.
                MemoryUtility.UnloadUnusedUnityAssets();
                Current.ProgramState = ProgramState.MapInitializing;

                Log.Warning("Setting ScribeMode to LoadingVars in Game_LoadGame_Patch to test.");
                Scribe.mode = LoadSaveMode.LoadingVars;
                
                persistentWorld.ExposeAndFillGameSmallComponents();

                persistentWorld.LoadGameWorldAndMaps();
                
                return false;
            }
        }

        //[HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame")]
        public static class GameDataSaveLoader_SaveGame_Patch
        {
            // TODO: Disallow saving through normal save menu.
        }
    }
}