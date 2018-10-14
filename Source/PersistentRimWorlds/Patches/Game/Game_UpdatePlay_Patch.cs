using Harmony;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "UpdatePlay")]
    public class Game_UpdatePlay_Patch
    {
        #region Methods
        static void Postfix(Game __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame))
            {
                return;
            }
            
            PersistentWorldManager.GetInstance().PersistentWorld.UpdateWorld();
        }
        #endregion
    }
}