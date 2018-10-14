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

        private static readonly MethodInfo GetInstanceMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "GetInstance");

        private static readonly MethodInfo PersistentWorldNotNullMethod =
            AccessTools.Method(typeof(PersistentWorldManager), "PersistentWorldNotNull");
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

                codes[i + 2].labels.Add(skipLabel1);

                var toInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, GetInstanceMethod),
                    new CodeInstruction(OpCodes.Callvirt, PersistentWorldNotNullMethod),
                    new CodeInstruction(OpCodes.Brtrue_S, skipLabel1)
                };

                codes.InsertRange(i - 1, toInsert);
                    
                break;
            }

            return codes.AsEnumerable();
        }
        #endregion
    }
}