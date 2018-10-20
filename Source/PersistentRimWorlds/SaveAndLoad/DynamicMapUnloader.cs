using System.Collections.Generic;
using Harmony;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public static class DynamicMapUnloader
    {
        #region Properties
        private static PersistentWorld PersistentWorld => PersistentWorldManager.GetInstance().PersistentWorld;
        #endregion
        
        public static void UnloadReferences(Map map, bool forced = false)
        {
            PersistentWorld.LoadSaver.ReferenceTable.ClearReferencesFor("\\Saves\\WorldHere\\Maps\\" + map.Tile + PersistentWorldLoadSaver.PersistentWorldMapFileExtension, forced);
        }

        public static void UnloadMap(Map map)
        {
            Current.Game.DeinitAndRemoveMap(map);
        }

        public static void UnloadColonyMaps(PersistentColony colony)
        {
            var maps = PersistentWorld.GetMapsForColony(colony);

            // Check to make sure we only unload maps we are sure aren't being used by any colony.
            var tilesToRemove = new HashSet<int>();

            foreach (var map in maps)
            {
                var tile = map.Tile;
                var set = PersistentWorld.LoadedMaps[tile];

                if (!set.Contains(colony) || set.Count != 1) continue;

                tilesToRemove.Add(tile);
                UnloadReferences(map, true);
                UnloadMap(map);
            }
            
            tilesToRemove.Do(tile => PersistentWorld.LoadedMaps.Remove(tile));
            tilesToRemove.Clear();
        }
    }
}