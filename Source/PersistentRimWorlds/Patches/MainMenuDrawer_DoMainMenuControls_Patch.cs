using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.SaveAndLoad;
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

        private static readonly MethodInfo AnyWorldsMethod =
            AccessTools.Method(typeof(SaveFileUtils), "AnyWorlds");
        #endregion
        
        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var jumpLabel = ilGen.DefineLabel();
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;
                if (codes[i + 4].opcode != OpCodes.Ldstr) continue;
                if ((codes[i + 4].operand as string) != "ReviewScenario") continue;

                codes[i].labels.Add(jumpLabel);
                
                var insertInstr = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, AnyWorldsMethod),
                    new CodeInstruction(OpCodes.Brfalse_S, jumpLabel),
                    
                    new CodeInstruction(OpCodes.Ldstr, "FilUnderscore.PersistentRimWorlds"),
                    new CodeInstruction(OpCodes.Call,TranslateMethod),
                    new CodeInstruction(OpCodes.Ldsfld, MainMenuButtonDelegate),
                    new CodeInstruction(OpCodes.Castclass, typeof(Action)),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Newobj, ListableOptionConstructor),
                    new CodeInstruction(OpCodes.Stloc_3),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Callvirt, AddMethod)
                };

                codes[i].labels.DoIf(label => label != jumpLabel, insertInstr[0].labels.Add);
                codes[i].labels.RemoveAll(label => label != jumpLabel);
                
                codes.InsertRange(i, insertInstr);
                
                break;
            }
            
            return codes.AsEnumerable();
        }
        #endregion
    }
}