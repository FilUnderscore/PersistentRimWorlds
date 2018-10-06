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
                
            Log.Message("Creating new colony.");
            MemoryUtility.UnloadUnusedUnityAssets();

            Current.ProgramState = ProgramState.MapInitializing;
            var mapSize = new IntVec3(__instance.InitData.mapSize, 1, __instance.InitData.mapSize);
            var settlement = (Settlement) null;
            
            var settlements = Find.WorldObjects.Settlements;
            for (var index = 0; index < settlements.Count; index++)
            {
                if (settlements[index].Faction == Faction.OfPlayer)
                {
                    settlement = settlements[index];
                    break;
                }
            }
                
            if(settlement == null)
                Log.Error("Could not generate starting map because there is no player faction base.");

            Log.Warning("MapSize: " + mapSize.ToString());
                
                // TODO: Use Map Size from World Info or have custom map sizes depending on colony data.
            var map = MapGenerator.GenerateMap(mapSize, settlement, settlement.MapGeneratorDef,
                settlement.ExtraGenStepDefs, null);
                
            Log.Message("UID: " + map.uniqueID);
                
            Current.Game.CurrentMap = map;
                
            PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
            __instance.FinalizeInit();
                
            Log.Message("Game state: " + Current.ProgramState.ToString());
                
            Log.Message((__instance == Current.Game).ToString());
            Log.Message((__instance == PersistentWorldManager.PersistentWorld.Game).ToString());
                
            Find.CameraDriver.JumpToCurrentMapLoc(MapGenerator.PlayerStartSpot);
            Find.CameraDriver.ResetSize();
                
            Find.Scenario.PostGameStart();
                
            GameComponentUtility.StartedNewGame();

            Current.Game.InitData = null;
            PersistentWorldManager.WorldLoadSaver.Status =
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame;

            var colony = PersistentColony.Convert(Current.Game);
            colony.ColonyData.uniqueID = PersistentWorldManager.PersistentWorld.WorldData.NextColonyId++;
            Log.Message("UNiqueID: " + colony.ColonyData.uniqueID.ToString());
            colony.ColonyData.ActiveWorldTiles.Add(map.Tile);
            PersistentWorldManager.PersistentWorld.Colony = colony;
            PersistentWorldManager.PersistentWorld.Colonies.Add(colony);

            Log.Message(__instance.DebugString());
            Log.Message("List things map.." + map.listerThings.AllThings.Count);
                
            return false;
        }       
    }
}