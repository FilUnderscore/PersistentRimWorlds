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
        private static readonly FieldInfo TicksAbsField = AccessTools.Field(typeof(LogEntry), "ticksAbs");
        
        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "GetInstance");

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "PersistentWorldNotNull");

        private static readonly MethodInfo GetTicksAbsMethod = AccessTools.Method(typeof(GenTicks), "get_TicksAbs");

        private static readonly MethodInfo GetTickManagerMethod = AccessTools.Method(typeof(Find), "get_TickManager");

        private static readonly FieldInfo GameStartAbsTickField =
            AccessTools.Field(typeof(TickManager), "gameStartAbsTick");
        #endregion
        
        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var jumpLabel = ilGen.DefineLabel();
            var skipLabel = ilGen.DefineLabel();
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Stfld) continue;
                if (codes[i].operand != TicksAbsField) continue;

                codes[i + 1].labels.Add(jumpLabel);
                codes[i + 7].labels.Add(skipLabel);
                
                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brfalse_S, jumpLabel),
                    
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, GetTicksAbsMethod),
                    new CodeInstruction(OpCodes.Stfld, TicksAbsField)
                };

                codes.InsertRange(i + 1, toInsert);
                var count = toInsert.Count;
                
                toInsert = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brtrue_S, skipLabel)
                };
                
                codes.InsertRange(i + 1 + count + 2, toInsert);

                break;
            }
            
            codes.Do(code => Log.Message(code.ToString()));
            
            return codes;
        }

        static ConstructorInfo TargetMethod()
        {
            return AccessTools.Constructor(typeof(LogEntry), new Type[] { typeof(LogEntryDef) });
        }
        #endregion
    }
}