using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "UpdatePlay")]
    public class Game_UpdatePlay_Patch
    {
        #region Methods
        static void Postfix(Game __instance)
        {
            if (!PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame))
            {
                return;
            }
            
            PersistentWorldManager.GetInstance().PersistentWorld.UpdateWorld();
        }
        #endregion
    }
}