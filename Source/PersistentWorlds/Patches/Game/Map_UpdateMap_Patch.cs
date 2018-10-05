using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Map), "MapUpdate")]
    public static class Map_MapUpdate_Patch
    {
        [HarmonyPrefix]
        public static bool MapUpdate_Prefix(Map __instance)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null ||
                PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                return true;

            if (PersistentWorldManager.PersistentWorld.Colony == null)
            {
                return true;
            }
                
            Log.Message("Current Tile: " + __instance.Tile);

            foreach (var tile in PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles)
            {
                Log.Message("Tile: " + tile);
            }

            return PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Contains(__instance.Tile);
        }
    }
}