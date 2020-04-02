using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    /// <summary>
    /// This patch gets rid of "Starting Season" if New Colony is selected under the Persistent RimWorlds menu.
    /// </summary>
    [HarmonyPatch(typeof(Dialog_AdvancedGameConfig), "DoWindowContents")]
    public class Dialog_AdvancedGameConfig_DoWindowContents_Patch
    {
        #region Fields
        private static readonly MethodInfo DisposeMethod =
            AccessTools.Method(typeof(IDisposable), nameof(IDisposable.Dispose));

        private static readonly MethodInfo
            GetInstanceMethod =
                AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.GetInstance));

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.PersistentWorldNotNull));
        #endregion
        
        #region Constructors
        static Dialog_AdvancedGameConfig_DoWindowContents_Patch()
        {
            if(DisposeMethod == null)
                throw new NullReferenceException($"{nameof(DisposeMethod)} is null.");
            
            if(GetInstanceMethod == null)
                throw new NullReferenceException($"{nameof(GetInstanceMethod)} is null.");
            
            if(PersistentWorldNotNullMethod == null)
                throw new NullReferenceException($"{nameof(PersistentWorldNotNullMethod)} is null.");
        }
        #endregion
        
        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGenerator)
        {
            var codes = new List<CodeInstruction>(instr);

            var label1 = ilGenerator.DefineLabel();

            codes[201].labels.Add(label1);
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Endfinally || codes[i - 1].opcode != OpCodes.Callvirt ||
                    codes[i - 1].operand != DisposeMethod) continue;

                var codesToInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call,
                        GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brtrue_S, label1)
                };

                codes.InsertRange(i + 3, codesToInsert);
                
                break;
            }
            
            return codes.AsEnumerable();
        }
        #endregion
    }
}