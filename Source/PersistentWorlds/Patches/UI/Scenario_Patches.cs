using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.Logic;
using PersistentWorlds.UI;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    public static class Scenario_Patches
    {
        [HarmonyPatch(typeof(Scenario), "GetFirstConfigPage")]
        public static class Scenario_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> GetFirstConfigPage_Transpiler(IEnumerable<CodeInstruction> instr)
            {
                var codes = new List<CodeInstruction>(instr);

                for (var i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode != OpCodes.Newobj) continue;
                    if (codes[i].operand != AccessTools.Constructor(typeof(Page_CreateWorldParams))) continue;

                    var codesToInsert = new List<CodeInstruction>();

                    //codes[i].operand = AccessTools.Constructor(typeof(Page_PersistentWorlds_SelectWorldList));
                    
                    // TODO: Add a branch (IF) to check if we have loaded a persistent world, if not show this menu then.
                    codes.RemoveRange(i, 3); // Don't need to generate/select world. Will be already loaded into memory.
                    
                    break;
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Page_SelectScenario), "BeginScenarioConfiguration")]
        public static class Page_SelectScenario_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> SelectScenario_Transpiler(IEnumerable<CodeInstruction> instr)
            {
                var codes = new List<CodeInstruction>(instr);

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Page_SelectLandingSite), "PreOpen")]
        public static class Page_SelectLandingSite_Patch
        {
            [HarmonyPrefix]
            public static void SelectLandingSite_Prefix(Page_SelectLandingSite __instance)
            {
                if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver.Status != PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading)
                    return;

                PersistentWorldManager.PersistentWorld.Game.Scenario = Current.Game.Scenario;
                Current.Game = PersistentWorldManager.PersistentWorld.Game;
                
                // TODO: Review
                Current.Game.InitData = new GameInitData();
                PersistentWorldManager.WorldLoadSaver.LoadMaps();
                
                Log.Message("Test 2");
                PersistentWorldManager.PersistentWorld.Game.World.pathGrid = new WorldPathGrid();
                //PersistentWorldManager.PersistentWorld.Game.World.grid.StandardizeTileData();
                //PersistentWorldManager.PersistentWorld.Game.World.FinalizeInit();
                //Find.Scenario.PostWorldGenerate();
                if (Current.Game.Scenario == null)
                {
                    Log.Error("game scenario null.");
                }
                else
                {
                    Log.Error("Game scenario not null.");
                    Current.Game.Scenario.PostWorldGenerate();
                }

                // TODO: Review
                Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_Main));
                Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_FileList));
                Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_ColonySelection));
            }
        }

        /* TODO: DEBUG */
        [HarmonyPatch(typeof(Settlement), "get_Material")]
        public static class Settlement_Patch
        {
            [HarmonyPrefix]
            public static void Patch_Prefix_Settle(Settlement __instance)
            {
                if (AccessTools.Field(typeof(Settlement), "cachedMat").GetValue(__instance) == null)
                {
                    Log.Message("Cached mat is null.");
                }

                if (__instance.Faction == null)
                {
                    Log.Message("Faction is null.");
                }
                else
                {
                    if (__instance.Faction.def == null)
                    {
                        Log.Message("Faction def.");
                    }
                }

                // TODO: Problem is that worldgenerator generates factions and crossref are handled between factionmanager and worldobjectholder.
                // TODO: We need to figure out how to cross-reference without interfering with ScribeLoader.
                
                
            }
        }

        [HarmonyPatch(typeof(Dialog_AdvancedGameConfig), "DoWindowContents")]
        public static class Dialog_AdvancedGameConfig_DoWindowContents_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGenerator)
            {
                var codes = new List<CodeInstruction>(instr);

                /* TODO: Remove seasons option from Advanced options in world tile picker menu for Persistent World only. */
                /*
                for (var i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode != OpCodes.Endfinally) continue;
                    
                    codes.RemoveRange(i + 1, 86);
                    
                    codes.Do(c => Log.Message(c.ToString()));

                    break;
                }
                */
                
                return codes.AsEnumerable();
            }
        }
    }
}