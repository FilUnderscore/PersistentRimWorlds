using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "FinalizeSaving")]
    public class ScribeSaver_FinalizeSaving_Patch
    {
        #region Fields
        private static readonly FieldInfo LoadIDsErrorsCheckerField =
            AccessTools.Field(typeof(ScribeSaver), nameof(ScribeSaver.loadIDsErrorsChecker));

        private static readonly MethodInfo CheckForErrorsAndClearMethod =
            AccessTools.Method(typeof(DebugLoadIDsSavingErrorsChecker),
                nameof(DebugLoadIDsSavingErrorsChecker.CheckForErrorsAndClear));

        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.GetInstance));

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.PersistentWorldNotNull));
        #endregion
        
        #region Constructors
        static ScribeSaver_FinalizeSaving_Patch()
        {
            if (LoadIDsErrorsCheckerField == null)
                throw new NullReferenceException($"{nameof(LoadIDsErrorsCheckerField)} is null.");

            if (CheckForErrorsAndClearMethod == null)
                throw new NullReferenceException($"{nameof(CheckForErrorsAndClearMethod)} is null.");
            
            if(GetInstanceMethod == null)
                throw new NullReferenceException($"{nameof(GetInstanceMethod)} is null.");
            
            if(PersistentWorldNotNullMethod == null)
                throw new NullReferenceException($"{nameof(PersistentWorldNotNullMethod)} is null.");
        }
        #endregion
        
        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var label1 = ilGen.DefineLabel();
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldarg_0) continue;
                if (codes[i + 1].opcode != OpCodes.Ldfld || codes[i + 1].operand != LoadIDsErrorsCheckerField) continue;

                if (codes[i + 2].opcode != OpCodes.Callvirt ||
                    codes[i + 2].operand != CheckForErrorsAndClearMethod) continue;

                codes[i + 3].labels.Add(label1);

                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brtrue_S, label1)
                };

                codes.InsertRange(i, toInsert);

                break;
            }
            
            return codes.AsEnumerable();
        }
        #endregion
    }
}