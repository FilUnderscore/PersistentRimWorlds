using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public class MainMenuDrawer_DoMainMenuControls_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instr);

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (codes[i + 4].opcode != OpCodes.Ldstr) continue;
                if ((codes[i + 4].operand as string) != "ReviewScenario") continue;

                var insertInstr = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldstr, "FilUnderscore.PersistentRimWorlds"),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Translator), "Translate", new Type[] {typeof(string)})),
                    new CodeInstruction(OpCodes.Ldsfld, typeof(PersistentWorldsMod).GetField("MainMenuButtonDelegate")),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Newobj,
                        AccessTools.Constructor(typeof(Verse.ListableOption),
                            new Type[] {typeof(string), typeof(System.Action), typeof(string)})),
                    new CodeInstruction(OpCodes.Stloc_3),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Method(typeof(List<Verse.ListableOption>), "Add",
                            new Type[] {typeof(ListableOption)}))
                };

                codes.InsertRange(i, insertInstr);
                    
                break;
            }

            return codes.AsEnumerable();
        }
    }
}