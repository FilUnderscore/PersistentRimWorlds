using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using RimWorld;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Dialog_AdvancedGameConfig), "DoWindowContents")]
    public static class Dialog_AdvancedGameConfig_DoWindowContents_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGenerator)
        {
            var codes = new List<CodeInstruction>(instr);

            /* TODO: Remove seasons option from Advanced options in world tile picker menu for Persistent World only. */
            /*
            for (var i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode != OpCodes.Endfinally) continue;
                
                codes.RemoveRange(i + 1, 86);
                
                codes.Do(c => Log.Message(c.ToString()));

                break;
            }
            */
                
            return codes.AsEnumerable();
        }
    }
}