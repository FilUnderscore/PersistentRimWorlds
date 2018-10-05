using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Scenario), "GetFirstConfigPage")]
    public static class Scenario_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GetFirstConfigPage_Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Newobj) continue;
                if (codes[i].operand != AccessTools.Constructor(typeof(Page_CreateWorldParams))) continue;

                var codesToInsert = new List<CodeInstruction>();

                var skipLabel1 = ilGen.DefineLabel();
                var skipLabel2 = ilGen.DefineLabel();

                codes[i - 1].labels.Add(skipLabel1);
                codes[i + 2].labels.Add(skipLabel2);
                    
                var toInsert = new List<CodeInstruction>();

                toInsert.Add(new CodeInstruction(OpCodes.Ldsfld,
                    AccessTools.Field(typeof(PersistentWorldManager), "PersistentWorld")));
                toInsert.Add(new CodeInstruction(OpCodes.Brfalse_S, skipLabel1));

                toInsert.Add(new CodeInstruction(OpCodes.Ldsfld,
                    AccessTools.Field(typeof(PersistentWorldManager), "WorldLoadSaver")));
                toInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, skipLabel2));
                    
                codes.InsertRange(i - 1, toInsert);
                    
                break;
            }

            return codes.AsEnumerable();
        }
    }
}