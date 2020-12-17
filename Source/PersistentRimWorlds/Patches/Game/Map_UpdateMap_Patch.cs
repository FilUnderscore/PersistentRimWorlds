using HarmonyLib;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Map), "MapUpdate")]
    public class Map_MapUpdate_Patch
    {
        #region Methods
        static bool Prefix(Map __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame))
                return true;

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            var colony = persistentWorld.Colony;
            
            return colony == null || colony.ColonyData.ActiveWorldTiles.Contains(__instance.Tile);
        }
        #endregion
    }
}