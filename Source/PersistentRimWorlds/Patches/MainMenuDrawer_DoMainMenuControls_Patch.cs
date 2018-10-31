using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using Verse;
using FileLog = PersistentWorlds.Utils.FileLog;

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

        private static readonly MethodInfo GetGameMethod = AccessTools.Method(typeof(Current), "get_Game");

        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "GetInstance");

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "PersistentWorldNotNull");
        #endregion
        
        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var mainJumpLabel = ilGen.DefineLabel();
            
            var saveSkipLabel = ilGen.DefineLabel();
            var saveJumpLabel = ilGen.DefineLabel();
            
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand == GetGameMethod &&
                    codes[i + 5].opcode == OpCodes.Ldstr && codes[i + 5].operand as string == "Save")
                {
                    Log.Message("Patched");
                    
                    codes[i + 4].labels.Add(saveJumpLabel);
                    codes[i + 17].labels.Add(saveSkipLabel);
                    
                    var insertInstr = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                        new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                        new CodeInstruction(OpCodes.Brfalse_S, saveJumpLabel),
                        
                        new CodeInstruction(OpCodes.Ldloc_2),
                        new CodeInstruction(OpCodes.Ldstr, "SaveWorld"),
                        new CodeInstruction(OpCodes.Call, TranslateMethod),
                        new CodeInstruction(OpCodes.Ldsfld, MainMenuButtonDelegate),
                        new CodeInstruction(OpCodes.Castclass, typeof(Action)),
                        new CodeInstruction(OpCodes.Ldnull),
                        new CodeInstruction(OpCodes.Newobj, ListableOptionConstructor),
                        new CodeInstruction(OpCodes.Callvirt, AddMethod),
                        new CodeInstruction(OpCodes.Br_S, saveSkipLabel)
                    };
                    
                    codes.InsertRange(i + 4, insertInstr);
                }
                else if (codes[i].opcode == OpCodes.Call && codes[i + 4].opcode == OpCodes.Ldstr &&
                    codes[i + 4].operand as string == "ReviewScenario")
                {
                    codes[i].labels.Add(mainJumpLabel);

                    var insertInstr = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, AnyWorldsMethod),
                        new CodeInstruction(OpCodes.Brfalse_S, mainJumpLabel),

                        new CodeInstruction(OpCodes.Ldstr, "FilUnderscore.PersistentRimWorlds"),
                        new CodeInstruction(OpCodes.Call, TranslateMethod),
                        new CodeInstruction(OpCodes.Ldsfld, MainMenuButtonDelegate),
                        new CodeInstruction(OpCodes.Castclass, typeof(Action)),
                        new CodeInstruction(OpCodes.Ldnull),
                        new CodeInstruction(OpCodes.Newobj, ListableOptionConstructor),
                        new CodeInstruction(OpCodes.Stloc_3),
                        new CodeInstruction(OpCodes.Ldloc_2),
                        new CodeInstruction(OpCodes.Ldloc_3),
                        new CodeInstruction(OpCodes.Callvirt, AddMethod)
                    };

                    codes[i].labels.DoIf(label => label != mainJumpLabel, insertInstr[0].labels.Add);
                    codes[i].labels.RemoveAll(label => label != mainJumpLabel);

                    codes.InsertRange(i, insertInstr);

                    break;
                }
            }
            
            codes.Do(code => FileLog.Log(code.ToString()));

            return codes.AsEnumerable();
        }
        #endregion
    }
}