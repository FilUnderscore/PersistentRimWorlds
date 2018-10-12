using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "ExitNode")]
    public class ScribeSaver_ExitNode_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var codes = new List<CodeInstruction>(instr);

            var label = new Label();
            
            for (var i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i + 1].opcode == OpCodes.Ret && i + 1 == codes.Count)
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
            
            codes.Do(code => Log.Message(code.ToString()));
            
            return codes.AsEnumerable();
        }
    }
}