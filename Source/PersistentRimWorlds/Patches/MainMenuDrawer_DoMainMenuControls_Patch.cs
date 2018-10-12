using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public class MainMenuDrawer_DoMainMenuControls_Patch
    {
        #region Fields
        private static readonly MethodInfo TranslateMethod =
            AccessTools.Method(typeof(Translator), "Translate", new[] {typeof(string)});

        private static readonly FieldInfo MainMenuButtonDelegate =
            AccessTools.Field(typeof(PersistentWorldsMod), "MainMenuButtonDelegate");

        private static readonly ConstructorInfo ListableOptionConstructor = AccessTools.Constructor(typeof(ListableOption),
            new[] {typeof(string), typeof(Action), typeof(string)});

        private static readonly MethodInfo AddMethod =
            AccessTools.Method(typeof(List<ListableOption>), "Add", new[] {typeof(ListableOption)});
        #endregion
        
        #region Methods
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
                    new CodeInstruction(OpCodes.Call,TranslateMethod),
                    new CodeInstruction(OpCodes.Ldsfld, MainMenuButtonDelegate),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Newobj, ListableOptionConstructor),
                    new CodeInstruction(OpCodes.Stloc_3),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Callvirt, AddMethod)
                };

                codes.InsertRange(i, insertInstr);
                    
                break;
            }

            return codes.AsEnumerable();
        }
        #endregion
    }
}