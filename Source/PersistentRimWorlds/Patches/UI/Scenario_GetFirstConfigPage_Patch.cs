using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Scenario), "GetFirstConfigPage")]
    public class Scenario_GetFirstConfigPage_Patch
    {
        #region Fields
        private static readonly ConstructorInfo PageCreateWorldParamsConstructor =
            AccessTools.Constructor(typeof(Page_CreateWorldParams));
        
        private static readonly FieldInfo PersistentWorldField =
            AccessTools.Field(typeof(PersistentWorldManager), "PersistentWorld");

        private static readonly FieldInfo WorldLoadSaverField =
            AccessTools.Field(typeof(PersistentWorldManager), "WorldLoadSaver");
        #endregion

        #region Methods
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var codes = new List<CodeInstruction>(instr);

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Newobj) continue;
                if (codes[i].operand != PageCreateWorldParamsConstructor) continue;

                var codesToInsert = new List<CodeInstruction>();

                var skipLabel1 = ilGen.DefineLabel();
                var skipLabel2 = ilGen.DefineLabel();

                codes[i - 1].labels.Add(skipLabel1);
                codes[i + 2].labels.Add(skipLabel2);
                    
                var toInsert = new List<CodeInstruction>();

                toInsert.Add(new CodeInstruction(OpCodes.Ldsfld,
                    PersistentWorldField));
                toInsert.Add(new CodeInstruction(OpCodes.Brfalse_S, skipLabel1));

                toInsert.Add(new CodeInstruction(OpCodes.Ldsfld,
                    WorldLoadSaverField));
                toInsert.Add(new CodeInstruction(OpCodes.Brtrue_S, skipLabel2));
                    
                codes.InsertRange(i - 1, toInsert);
                    
                break;
            }

            return codes.AsEnumerable();
        }
        #endregion
    }
}