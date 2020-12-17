using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(GenGameEnd), "EndGameDialogMessage", new Type[] { typeof(string), typeof(bool), typeof(Color) })]
    public class GenGameEnd_EndGameDialogMessage_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            // TODO: 0.1.4.0
            
            return codes.AsEnumerable();
        }
    }
}