using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(LogEntry), new Type[] { typeof(LogEntryDef) })]
    public static class LogEntry_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var codes = new List<CodeInstruction>(instr);

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (codes[i].operand != AccessTools.Method(typeof(Find), "get_TickManager")) continue;

                codes[i].opcode = OpCodes.Ldfld;
                codes[i].operand = AccessTools.Field(typeof(PersistentWorldData), "TickManager");
                    
                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(PersistentWorldManager), "PersistentWorld")),
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(PersistentWorld), "WorldData"))
                };

                codes.InsertRange(i, toInsert);

                break;
            }
                
            return codes;
        }
    }
}