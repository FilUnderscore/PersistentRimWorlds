using System.Collections.Generic;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public class MapGenerator_GenerateMap_Patch
    {
        static void Postfix(Map __result)
        {
            if (!PersistentWorldManager.Active())
                return;

            if (PersistentWorldManager.PersistentWorld.Colony != null)
            {
                PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Add(__result.Tile);

                if (PersistentWorldManager.PersistentWorld.Maps.ContainsKey(PersistentWorldManager.PersistentWorld
                    .Colony))
                {
                    PersistentWorldManager.PersistentWorld.Maps[PersistentWorldManager.PersistentWorld.Colony]
                        .Add(__result.Tile);
                }
                else
                {
                    PersistentWorldManager.PersistentWorld.Maps.Add(PersistentWorldManager.PersistentWorld.Colony,
                        new List<int>() {__result.Tile});
                }
            }
        }
    }
}