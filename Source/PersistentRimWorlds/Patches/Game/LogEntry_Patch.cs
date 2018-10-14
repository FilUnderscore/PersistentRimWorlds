using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch]
    public class LogEntry_Patch
    {
        #region Fields
        private static readonly MethodInfo GetTickManagerMethodFind = AccessTools.Method(typeof(Find), "get_TickManager");

        private static readonly MethodInfo GetTickManagerMethodPersistentWorldData =
            AccessTools.Method(typeof(PersistentWorldData), "get_TickManager");
        
        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "GetInstance");

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "PersistentWorldNotNull");
        
        private static readonly MethodInfo GetPersistentWorldMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "get_PersistentWorld");
        #endregion
        
        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var jumpLabel = ilGen.DefineLabel();
            var skipLabel = ilGen.DefineLabel();
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (codes[i].operand != GetTickManagerMethodFind) continue;

                codes[i].labels.Add(jumpLabel);
                codes[i + 1].labels.Add(skipLabel);
                
                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brfalse_S, jumpLabel),
                    
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, GetPersistentWorldMethod),
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(PersistentWorld), "WorldData")),
                    
                    new CodeInstruction(OpCodes.Callvirt, GetTickManagerMethodPersistentWorldData),
                    
                    new CodeInstruction(OpCodes.Br, skipLabel)
                };

                codes.InsertRange(i, toInsert);

                break;
            }
                
            return codes;
        }

        static ConstructorInfo TargetMethod()
        {
            return AccessTools.Constructor(typeof(LogEntry), new Type[] { typeof(LogEntryDef) });
        }
        #endregion
    }
}