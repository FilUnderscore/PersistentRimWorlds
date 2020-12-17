using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(GenGameEnd), "EndGameDialogMessage", new Type[] { typeof(string), typeof(bool), typeof(Color) })]
    public class GenGameEnd_EndGameDialogMessage_Patch
    {
        #region Fields
        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.GetInstance));
        
        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), nameof(PersistentWorldManager.PersistentWorldNotNull));

        private static readonly MethodInfo TranslateMethod =
            AccessTools.Method(typeof(Translator), nameof(Translator.Translate), new[] {typeof(string)});

        private static readonly ConstructorInfo DiaOptionConstructor =
            AccessTools.Constructor(typeof(DiaOption), new[] {typeof(string)});
        
        private static readonly FieldInfo AbandonColonyButtonDelegate =
            AccessTools.Field(typeof(PersistentWorld), nameof(PersistentWorld.AbandonColonyButtonDelegate));

        private static readonly FieldInfo ActionField = AccessTools.Field(typeof(DiaOption), nameof(DiaOption.action));

        private static readonly FieldInfo ResolveTreeField =
            AccessTools.Field(typeof(DiaOption), nameof(DiaOption.resolveTree));

        private static readonly FieldInfo OptionsField = AccessTools.Field(typeof(DiaNode), nameof(DiaNode.options));
        
        private static readonly MethodInfo AddMethod =
            AccessTools.Method(typeof(List<DiaOption>), nameof(List<DiaOption>.Add),
                new[] {typeof(DiaOption)});

        private static readonly MethodInfo BypassTranslateTaggedStringMethod =
            AccessTools.Method(typeof(GenGameEnd_EndGameDialogMessage_Patch),
                nameof(GenGameEnd_EndGameDialogMessage_Patch.BypassTranslateTaggedString),
                new[] {typeof(string)});
        #endregion
        
        #region Constructors
        static GenGameEnd_EndGameDialogMessage_Patch()
        {
            if (GetInstanceMethod == null)
                throw new NullReferenceException($"{nameof(GetInstanceMethod)} is null.");

            if (PersistentWorldNotNullMethod == null)
                throw new NullReferenceException($"{nameof(PersistentWorldNotNullMethod)} is null.");

            if (TranslateMethod == null)
                throw new NullReferenceException($"{nameof(TranslateMethod)} is null.");

            if (DiaOptionConstructor == null)
                throw new NullReferenceException($"{nameof(DiaOptionConstructor)} is null.");

            if (AbandonColonyButtonDelegate == null)
                throw new NullReferenceException($"{nameof(AbandonColonyButtonDelegate)} is null.");

            if (ActionField == null)
                throw new NullReferenceException($"{nameof(ActionField)} is null.");

            if (ResolveTreeField == null)
                throw new NullReferenceException($"{nameof(ResolveTreeField)} is null.");

            if (OptionsField == null)
                throw new NullReferenceException($"{nameof(OptionsField)} is null.");

            if (AddMethod == null)
                throw new NullReferenceException($"{nameof(AddMethod)} is null.");

            if (BypassTranslateTaggedStringMethod == null)
                throw new NullReferenceException($"{nameof(BypassTranslateTaggedStringMethod)} is null.");
        }
        #endregion

        #region Methods
        static string BypassTranslateTaggedString(string toTranslate)
        {
            return toTranslate.Translate();
        }
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var jumpLabel = ilGen.DefineLabel();
            var skipLabel = ilGen.DefineLabel();
            var skip2Label = ilGen.DefineLabel();
            
            // TODO: 0.1.4.0
            for (var i = 0; i < codes.Count; i++)
            {
                // i should start at 3.
                if (codes[i].opcode != OpCodes.Stloc_0 && codes[i + 1].opcode != OpCodes.Ldarg_1)
                {
                    continue;
                }

                codes[i + 15].labels.Add(skipLabel);
                
                codes[i + 38].labels.Add(skip2Label);

                codes[i + 2].operand = jumpLabel;

                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brfalse_S, jumpLabel),
                    new CodeInstruction(OpCodes.Ldstr, "FilUnderscore.PersistentRimWorlds.Colony.Abandon"),
                    new CodeInstruction(OpCodes.Call, BypassTranslateTaggedStringMethod),
                    new CodeInstruction(OpCodes.Newobj, DiaOptionConstructor),
                    new CodeInstruction(OpCodes.Stloc_3),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldsfld, AbandonColonyButtonDelegate),
                    new CodeInstruction(OpCodes.Stfld, ActionField),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Stfld, ResolveTreeField),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldfld, OptionsField),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Callvirt, AddMethod),
                    new CodeInstruction(OpCodes.Br_S, skip2Label)
                };
                
                codes.InsertRange(i + 1, toInsert);
                
                toInsert[0].labels.Add(jumpLabel);

                break;
            }
            
            return codes.AsEnumerable();
        }
        #endregion
    }
}