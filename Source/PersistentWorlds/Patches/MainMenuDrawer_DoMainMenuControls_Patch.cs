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
    public static class MainMenuDrawer_DoMainMenuControls_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DoMainMenuControls_Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instr);

            for (var i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (codes[i + 4].opcode != OpCodes.Ldstr) continue;
                if ((codes[i + 4].operand as string) != "ReviewScenario") continue;

                if (Current.ProgramState != ProgramState.Entry) break;
                    
                var insertInstr = new List<CodeInstruction>();
                insertInstr.Add(new CodeInstruction(OpCodes.Ldstr, "Persistent Worlds"));
                insertInstr.Add(new CodeInstruction(OpCodes.Ldsfld, typeof(PersistentWorldsMod).GetField("MainMenuButtonDelegate")));
                    
                insertInstr.Add(new CodeInstruction(OpCodes.Ldnull));
                insertInstr.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Verse.ListableOption), new Type[] { typeof(string), typeof(System.Action), typeof(string) })));
                insertInstr.Add(new CodeInstruction(OpCodes.Stloc_3));
                insertInstr.Add(new CodeInstruction(OpCodes.Ldloc_2));
                insertInstr.Add(new CodeInstruction(OpCodes.Ldloc_3));
                insertInstr.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Verse.ListableOption>), "Add", new Type[] { typeof(ListableOption) })));

                codes.InsertRange(i, insertInstr);
                    
                break;
            }

            return codes.AsEnumerable();
        }
    }
}