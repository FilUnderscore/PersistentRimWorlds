using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "InitNewGame")]
    public class Game_InitNewGame_Patch
    {
        #region Methods
        static bool Prefix(Game __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
            {
                return true;
            }
            
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;

            var game = __instance;
            
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

            if (settlement == null)
            {
                Log.Error("Could not generate starting map because there is no player faction base.");

                GenScene.GoToMainMenu();

                return false;
            }

            var map = MapGenerator.GenerateMap(mapSize, settlement, settlement.MapGeneratorDef,
                settlement.ExtraGenStepDefs, null);
                
            game.CurrentMap = map;

            // TODO: Implement permanent death mode for colonies.
            if (__instance.InitData.permadeath)
            {
                __instance.Info.permadeathMode = true;
            }
            
            PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
            __instance.FinalizeInit();
                
            Find.CameraDriver.JumpToCurrentMapLoc(MapGenerator.PlayerStartSpot);
            Find.CameraDriver.ResetSize();

            if (Prefs.PauseOnLoad && Current.Game.InitData.startedFromEntry)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    game.tickManager.DoSingleTick();
                    game.tickManager.CurTimeSpeed = TimeSpeed.Paused;
                });
            }
            
            Find.Scenario.PostGameStart();

            /*
             * Complete research needed depending if PlayerFaction is PlayerColony or PlayerTribe.
             */
            if (Faction.OfPlayer.def.startingResearchTags != null)
            {
                foreach (var startingResearchTag in Faction.OfPlayer.def.startingResearchTags)
                {
                    foreach (var allDef in DefDatabase<ResearchProjectDef>.AllDefs)
                    {
                        if(allDef.HasTag(startingResearchTag))
                            game.researchManager.FinishProject(allDef, false, null);
                    }
                }
            }
                
            GameComponentUtility.StartedNewGame();

            persistentWorld.LoadSaver.Status =
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame;

            var colony = PersistentColony.Convert(Current.Game);

            colony.ColonyData.uniqueID = ++PersistentWorldManager.GetInstance().PersistentWorld.WorldData.NextColonyId;
            colony.ColonyData.ActiveWorldTiles.Add(map.Tile);
            
            colony.GameData.mapSize = __instance.InitData.mapSize;
            
            game.InitData = null;
            
            persistentWorld.Colony = colony;

            persistentWorld.CheckAndSetColonyData();
            
            persistentWorld.Colonies.Add(colony);
            persistentWorld.LoadedMaps.Add(map.Tile, new HashSet<PersistentColony>(){colony});
    
            return false;
        }       
        #endregion
    }
}