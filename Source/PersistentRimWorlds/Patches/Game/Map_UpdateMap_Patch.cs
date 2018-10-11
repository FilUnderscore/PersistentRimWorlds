using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Map), "MapUpdate")]
    public class Map_MapUpdate_Patch
    {
        static bool Prefix(Map __instance)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null ||
                PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                return true;

            return PersistentWorldManager.PersistentWorld.Colony == null || PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Contains(__instance.Tile);
        }
    }
}