using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MapDeiniter), "Deinit")]
    public class MapDeiniter_Deinit_Patch
    {
        #region Fields
        private static readonly MethodInfo PassPawnsToWorldMethod =
            AccessTools.Method(typeof(MapDeiniter), "PassPawnsToWorld");

        private static readonly FieldInfo UnloadingField = AccessTools.Field(typeof(DynamicMapUnloader), "Unloading");

        private static readonly MethodInfo UnloadPawnsFromWorld =
            AccessTools.Method(typeof(DynamicMapUnloader), "UnloadPawnsFromWorld");
        #endregion
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var label1 = ilGen.DefineLabel();
            var label2 = ilGen.DefineLabel();
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (codes[i].operand != PassPawnsToWorldMethod) continue;

                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldsfld, UnloadingField),
                    new CodeInstruction(OpCodes.Brfalse_S, label1),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, UnloadPawnsFromWorld),
                    new CodeInstruction(OpCodes.Br_S, label2)
                };
                
                codes[i - 1].labels.Do(label => toInsert[0].labels.Add(label));
                codes[i - 1].labels.Clear();
                
                codes[i - 1].labels.Add(label1);
                codes[i + 1].labels.Add(label2);
                
                codes.InsertRange(i - 1, toInsert);

                break;
            }
            
            return codes.AsEnumerable();
        }
    }
}