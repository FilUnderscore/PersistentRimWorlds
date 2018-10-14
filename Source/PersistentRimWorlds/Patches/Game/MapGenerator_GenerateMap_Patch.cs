using System.Collections.Generic;
using Harmony;
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

            if (persistentWorld.Maps.ContainsKey(colony))
            {
                persistentWorld.Maps[colony].Add(__result.Tile);
            }
            else
            {
                persistentWorld.Maps.Add(colony, new List<int>() {__result.Tile});
            }
        }
        #endregion
    }
}