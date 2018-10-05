using System.Linq;
using Harmony;
using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.Patches
{
    public class Game_Patches
    {
        [HarmonyPatch(typeof(Game), "LoadGame")]
        public static class Game_LoadGame_Patch
        {
            [HarmonyPrefix]
            public static bool LoadGame_Prefix(Game __instance)
            {
                if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Uninitialized)
                {
                    return true;
                }
                
                var persistentWorld = PersistentWorldManager.PersistentWorld;
                
                LongEventHandler.SetCurrentEventText("LoadingPersistentWorld".Translate());
                
                // Unload.
                MemoryUtility.UnloadUnusedUnityAssets();
                Current.ProgramState = ProgramState.MapInitializing;

                Log.Warning("Setting ScribeMode to LoadingVars in Game_LoadGame_Patch to test.");
                Scribe.mode = LoadSaveMode.LoadingVars;
                
                persistentWorld.ExposeAndFillGameSmallComponents();

                persistentWorld.LoadGameWorldAndMaps();
                
                return false;
            }
        }

        [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame")]
        public static class GameDataSaveLoader_SaveGame_Patch
        {
            // TODO: Disallow saving through normal save menu.
            [HarmonyPrefix]
            public static bool SaveGame_Prefix(string fileName)
            {
                if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null ||
                    PersistentWorldManager.WorldLoadSaver.Status !=
                    PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                    return true;
                
                PersistentWorldManager.WorldLoadSaver.SaveWorld(PersistentWorldManager.PersistentWorld);
                Log.Message("World saved.");
                
                return false;
            }
        }

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
                
                PersistentWorldManager.WorldLoadSaver.LoadMaps();
                
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
                colony.ColonyData.ActiveWorldTiles.Add(map.Tile);
                PersistentWorldManager.PersistentWorld.Colony = colony;
                PersistentWorldManager.PersistentWorld.Colonies.Add(colony);
                
                Log.Message(__instance.DebugString());
                Log.Message("List things map.." + map.listerThings.AllThings.Count);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(Map), "FinalizeInit")]
        public static class Map_FinalizeInit_Patch
        {
            [HarmonyPrefix]
            public static void FinalizeInit_Prefix(Map __instance)
            {
                Log.Message("Lister all things.." + __instance.listerThings.AllThings.Count);
            }

            [HarmonyPostfix]
            public static void FinalizeInit_Postfix(Map __instance)
            {
                Log.Message("Lister all things POST.." + __instance.listerThings.AllThings.Count);
            }
        }

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

                return PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Contains(__instance.Tile);
            }
        }

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
}