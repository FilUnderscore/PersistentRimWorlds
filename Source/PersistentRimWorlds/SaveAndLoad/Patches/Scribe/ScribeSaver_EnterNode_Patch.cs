using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    /// <summary>
    /// This method needs to be patched for the ReferenceTable to work properly.
    /// </summary>
    [HarmonyPatch(typeof(ScribeSaver), "EnterNode")]
    public class ScribeSaver_EnterNode_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var codes = new List<CodeInstruction>(instr);
            
            var label = new Label();

            for (var i = 0; i < codes.Count - 1; i++)
            {
                if (i + 1 == codes.Count && codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Ret)
                {
                    codes[i].labels.Remove(label);
                    break;
                }

                if (codes[i].opcode != OpCodes.Ldsfld ||
                    codes[i].operand != AccessTools.Field(typeof(UnityData), "isDebugBuild") ||
                    codes[i + 1].opcode != OpCodes.Brfalse) continue;
                
                codes[i + 2].labels.Add(codes[i].labels[0]);
                codes[i].labels.RemoveAt(0);
                
                label = (Label) codes[i + 1].operand;
                codes.RemoveRange(i, 2);
            }

            return codes.AsEnumerable();
        }
    }
}