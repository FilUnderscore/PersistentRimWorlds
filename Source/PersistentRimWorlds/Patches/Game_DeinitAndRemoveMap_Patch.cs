using Harmony;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "DeinitAndRemoveMap")]
    public class Game_DeinitAndRemoveMap_Patch
    {
        #region Methods
        static bool Prefix(Game __instance, Map map)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame))
            {
                return true;
            }

            // TODO: Patch. Can use map.isPlayerHome()

            return true;
        }
        #endregion
    }
}