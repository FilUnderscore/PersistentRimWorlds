using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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
            AccessTools.Method(typeof(Translator), nameof(Translator.Translate), new[] {typeof(string)});

        private static readonly FieldInfo MainMenuButtonDelegate =
            AccessTools.Field(typeof(PersistentWorldsMod), nameof(PersistentWorldsMod.MainMenuButtonDelegate));

        private static readonly ConstructorInfo ListableOptionConstructor = AccessTools.Constructor(
            typeof(ListableOption), new[] {typeof(string), typeof(Action), typeof(string)});

        private static readonly MethodInfo AddMethod =
            AccessTools.Method(typeof(List<ListableOption>), nameof(List<ListableOption>.Add),
                new[] {typeof(ListableOption)});

        private static readonly MethodInfo AnyWorldsMethod =
            AccessTools.Method(typeof(SaveFileUtils), nameof(SaveFileUtils.AnyWorlds));

        private static readonly MethodInfo GetGameMethod =
            AccessTools.Property(typeof(Current), nameof(Current.Game)).GetGetMethod();

        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.GetInstance));

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.PersistentWorldNotNull));

        private static readonly FieldInfo SaveMenuButtonDelegate =
            AccessTools.Field(typeof(PersistentWorldsMod), nameof(PersistentWorldsMod.SaveMenuButtonDelegate));
        #endregion
        
        #region Constructors
        static MainMenuDrawer_DoMainMenuControls_Patch()
        {
            if(TranslateMethod == null)
                throw new NullReferenceException($"{nameof(TranslateMethod)} is null.");
            
            if(MainMenuButtonDelegate == null)
                throw new NullReferenceException($"{nameof(MainMenuButtonDelegate)} is null.");
            
            if(ListableOptionConstructor == null)
                throw new NullReferenceException($"{nameof(ListableOptionConstructor)} is null.");
            
            if(AddMethod == null)
                throw new NullReferenceException($"{nameof(AddMethod)} is null.");
            
            if(AnyWorldsMethod == null)
                throw new NullReferenceException($"{nameof(AnyWorldsMethod)} is null.");
            
            if(GetGameMethod == null)
                throw new NullReferenceException($"{nameof(GetGameMethod)} is null.");
            
            if(GetInstanceMethod == null)
                throw new NullReferenceException($"{nameof(GetInstanceMethod)} is null.");
            
            if(PersistentWorldNotNullMethod == null)
                throw new NullReferenceException($"{nameof(PersistentWorldNotNullMethod)} is null.");
            
            if(SaveMenuButtonDelegate == null)
                throw new NullReferenceException($"{nameof(SaveMenuButtonDelegate)} is null.");
        }
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
                    codes[i + 4].labels.Add(saveJumpLabel);
                    codes[i + 17].labels.Add(saveSkipLabel);
                    
                    var insertInstr = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                        new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                        new CodeInstruction(OpCodes.Brfalse_S, saveJumpLabel),
                        
                        new CodeInstruction(OpCodes.Ldloc_2),
                        new CodeInstruction(OpCodes.Ldstr, "FilUnderscore.PersistentRimWorlds.Save.World"),
                        new CodeInstruction(OpCodes.Call, TranslateMethod),
                        new CodeInstruction(OpCodes.Ldsfld, SaveMenuButtonDelegate),
                        new CodeInstruction(OpCodes.Castclass, typeof(Action)),
                        new CodeInstruction(OpCodes.Ldnull),
                        new CodeInstruction(OpCodes.Newobj, ListableOptionConstructor),
                        new CodeInstruction(OpCodes.Callvirt, AddMethod),
                        new CodeInstruction(OpCodes.Br_S, saveSkipLabel)
                    };
                    
                    codes.InsertRange(i + 4, insertInstr);
                }
                else if (codes[i].opcode == OpCodes.Call && codes[i].operand == GetGameMethod && 
                         codes[i + 4].opcode == OpCodes.Ldstr && codes[i + 4].operand as string == "LoadGame")
                {
                    var label = (Label) codes[i + 3].operand;
                    
                    var insertInstr = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                        new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                        new CodeInstruction(OpCodes.Brtrue_S, label)
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
            
            return codes.AsEnumerable();
        }
        #endregion
    }
}