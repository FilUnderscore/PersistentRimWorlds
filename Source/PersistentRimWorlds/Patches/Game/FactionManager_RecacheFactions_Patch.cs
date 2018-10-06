using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using PersistentWorlds.Logic;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    /*
    [HarmonyPatch(typeof(FactionManager), "RecacheFactions")]
    public static class FactionManager_RecacheFactions_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RecacheFactions_Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            var activeLabel = ilGen.DefineLabel();
            var skipLabel = ilGen.DefineLabel();
            
            codes[0].labels.Add(activeLabel);
            codes[28].labels.Add(skipLabel);
            
            var toInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PersistentWorldManager), "Active")),
                new CodeInstruction(OpCodes.Brfalse_S, activeLabel),
                
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldsfld,
                    AccessTools.Field(typeof(PersistentWorldManager), "PersistentWorld")),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PersistentWorld), "Colony")),
                new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.Method(typeof(PersistentColony), "AsFaction")),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(FactionManager), "ofPlayer")),
                new CodeInstruction(OpCodes.Br_S, skipLabel)
            };

            codes.InsertRange(0, toInsert);

            return codes.AsEnumerable();
        }
    }
    */
}