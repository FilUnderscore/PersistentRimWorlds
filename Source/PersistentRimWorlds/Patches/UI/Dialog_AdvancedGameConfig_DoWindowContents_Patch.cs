using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using RimWorld;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Dialog_AdvancedGameConfig), "DoWindowContents")]
    public class Dialog_AdvancedGameConfig_DoWindowContents_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGenerator)
        {
            var codes = new List<CodeInstruction>(instr);

            /* TODO: Remove seasons option from Advanced options in world tile picker menu for Persistent World only. */
                
            return codes.AsEnumerable();
        }
    }
}