using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.UI;
using RimWorld;
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
                    codes.RemoveRange(i, 2); // Don't need to generate/select world. Will be already loaded into memory.
                    
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
    }
}