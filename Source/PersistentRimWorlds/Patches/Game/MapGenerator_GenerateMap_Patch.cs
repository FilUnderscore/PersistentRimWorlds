using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public static class MapGenerator_GenerateMap_Patch
    {
        [HarmonyPostfix]
        public static void GenerateMap_Postfix(Map __result)
        {
            if (!PersistentWorldManager.Active())
                return;

            if (PersistentWorldManager.PersistentWorld.Colony != null)
            {
                PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Add(__result.Tile);
            }
        }
    }
}