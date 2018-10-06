using Harmony;
using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "InitNewGame")]
    public static class Game_InitNewGame_Patch
    {
        [HarmonyPrefix]
        public static bool InitNewGame_Prefix(Game __instance)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null ||
                PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Creating)
                return true;
                
            MemoryUtility.UnloadUnusedUnityAssets();

            Current.ProgramState = ProgramState.MapInitializing;
            var mapSize = new IntVec3(__instance.InitData.mapSize, 1, __instance.InitData.mapSize);
            var settlement = (Settlement) null;
            
            var settlements = Find.WorldObjects.Settlements;
            foreach (var t in settlements)
            {
                if (t.Faction != Faction.OfPlayer) continue;
                
                settlement = t;
                break;
            }
                
            if(settlement == null)
                Log.Error("Could not generate starting map because there is no player faction base.");
    
                // TODO: Use Map Size from World Info or have custom map sizes depending on colony data.
            var map = MapGenerator.GenerateMap(mapSize, settlement, settlement.MapGeneratorDef,
                settlement.ExtraGenStepDefs, null);
                
            Current.Game.CurrentMap = map;
                
            PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
            __instance.FinalizeInit();
                
            Find.CameraDriver.JumpToCurrentMapLoc(MapGenerator.PlayerStartSpot);
            Find.CameraDriver.ResetSize();
                
            Find.Scenario.PostGameStart();
                
            GameComponentUtility.StartedNewGame();

            Current.Game.InitData = null;
            PersistentWorldManager.WorldLoadSaver.Status =
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame;

            var colony = PersistentColony.Convert(Current.Game);
            
            colony.ColonyData.uniqueID = PersistentWorldManager.PersistentWorld.WorldData.NextColonyId++;
            colony.ColonyData.ActiveWorldTiles.Add(map.Tile);
            PersistentWorldManager.PersistentWorld.Colony = colony;
            colony.SetFactionData();
            PersistentWorldManager.PersistentWorld.Colonies.Add(colony);
                
            return false;
        }       
    }
}