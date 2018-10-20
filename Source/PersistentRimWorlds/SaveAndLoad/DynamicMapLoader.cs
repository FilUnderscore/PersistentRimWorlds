using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using PersistentWorlds.Logic;
using RimWorld;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public static class DynamicMapLoader
    {
        #region Fields
        private static readonly FieldInfo ReservedDestinationsField =
            AccessTools.Field(typeof(PawnDestinationReservationManager), "reservedDestinations");
        #endregion
        
        #region Properties
        private static PersistentWorld PersistentWorld => PersistentWorldManager.GetInstance().PersistentWorld;
        #endregion
        
        #region Methods
        #region Map Loading Methods
        public static IEnumerable<Map> LoadMaps(params int[] tiles)
        {
            Current.ProgramState = ProgramState.MapInitializing;

            Log.Message("Tiles: " + tiles.Length);
            
            var tileSet = new List<int>(tiles);
            tileSet.RemoveAll(tile => PersistentWorld.LoadedMaps.ContainsKey(tile));
            
            Log.Message("Set count: " + tileSet.Count);
            
            var maps = new List<Map>(PersistentWorld.LoadSaver.LoadMaps(tileSet.ToArray()));

            Log.Message("Map count: " + maps.Count);
            
            foreach (var map in maps)
            {
                Current.Game.Maps.Add(map);

                SetupLoadedMap(map);
            }

            Current.ProgramState = ProgramState.Playing;

            return maps.AsEnumerable();
        }
        
        public static Map LoadMap(int tile)
        {
            return LoadMaps(tile).First();    
        }
        
        public static IEnumerable<Map> LoadColonyMaps(PersistentColony colony)
        {
            var maps = LoadMaps(colony.ColonyData.ActiveWorldTiles.ToArray());
            
            foreach (var map in maps)
            {
                PersistentWorld.LoadedMaps.Add(map.Tile, new HashSet<PersistentColony>(){colony});
                
                yield return map;
            }
        }
        #endregion
        
        #region Map Data Loading Methods
        private static void SetupLoadedMap(Map map)
        {
            /*
            * Register in case as to not cause problems.
            */
            
            var reservedDestinations =
                (Dictionary<Faction, PawnDestinationReservationManager.PawnDestinationSet>)
                ReservedDestinationsField.GetValue(map.pawnDestinationReservationManager);

            foreach (var faction in Find.FactionManager.AllFactions)
            {
                if (!reservedDestinations.ContainsKey(faction))
                {
                    map.pawnDestinationReservationManager.RegisterFaction(faction);
                }
            }
            
            /*
             * Regenerate map data and finalize loading.
             */
            
            map.mapDrawer.RegenerateEverythingNow();
            map.FinalizeLoading();
            map.Parent.FinalizeLoading();
        }
        #endregion
        #endregion
    }
}