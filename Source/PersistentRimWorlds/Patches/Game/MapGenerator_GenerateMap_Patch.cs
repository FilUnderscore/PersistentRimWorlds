using System.Collections.Generic;
using HarmonyLib;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public class MapGenerator_GenerateMap_Patch
    {
        #region Methods
        static void Postfix(Map __result)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return;

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            var colony = persistentWorld.Colony;
            
            if (colony == null) return;

            colony.ColonyData.ActiveWorldTiles.Add(__result.Tile);

            persistentWorld.LoadedMaps.Add(__result.Tile, new HashSet<PersistentColony>(){colony});
        }
        #endregion
    }
}