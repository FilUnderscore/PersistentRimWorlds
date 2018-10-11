using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Xml;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    /*
     * Gotta transpile this because there is just no other way, I've tried them all :|
     */
    [HarmonyPatch(typeof(WorldPawns), "ExposeData")]
    public class WorldPawns_ExposeData_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr,
            ILGenerator ilGenerator)
        {
            var codes = new List<CodeInstruction>(instr);

            var label1 = ilGenerator.DefineLabel();
            codes[0].labels.Add(label1);

            var label2 = ilGenerator.DefineLabel();
            var label3 = ilGenerator.DefineLabel();

            var toInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldsfld,
                    AccessTools.Field(typeof(PersistentWorldManager), "WorldLoadSaver")),
                new CodeInstruction(OpCodes.Brfalse_S, label1),
                new CodeInstruction(OpCodes.Ldsfld,
                    AccessTools.Field(typeof(PersistentWorldManager), "WorldLoadSaver")),
                new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(PersistentWorldLoadSaver), "Status")),
                new CodeInstruction(OpCodes.Ldc_I4_2),
                new CodeInstruction(OpCodes.Bne_Un_S, label2)
            };

            codes.InsertRange(0, toInsert);

            toInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Br_S, label3),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda,
                    AccessTools.Field(typeof(WorldPawns), "pawnsForcefullyKeptAsWorldPawns")),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(WorldPawns_ExposeData_Patch), "Load"))
            };

            toInsert[1].labels.Add(label2);
            
            codes.InsertRange(12, toInsert);

            codes[16].labels.Add(label3);
            
            return codes.AsEnumerable();
        }

        public static void Load(ref HashSet<Pawn> pawns)
        {
            if (!Scribe.EnterNode("pawnsForcefullyKeptAsWorldPawns")) return;
                
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    if (pawns == null)
                    {
                        Scribe.saver.WriteAttribute("IsNull", "True");
                        break;
                    }

                    foreach (var pawn in pawns)
                    {
                        var refee = pawn;
                        Scribe_References.Look<Pawn>(ref refee, "li", true);
                    }
                        
                    break;
                case LoadSaveMode.LoadingVars:
                    var curXmlParent = Scribe.loader.curXmlParent;
                    var attribute = curXmlParent.Attributes["IsNull"];
                    if (attribute != null && attribute.Value.ToLower() == "true")
                    {
                        pawns = null;
                        break;
                    }
                    else
                    {
                        pawns = new HashSet<Pawn>();
                    }
                        
                    foreach (var node in curXmlParent.ChildNodes)
                    {
                        var nodeText = ((XmlNode) node).InnerText;
                            
                        var pawn = new Pawn();
                        Scribe_References.Look(ref pawn, nodeText);
                        pawns.Add(pawn);
                    }

                    break;
            }
                
            Scribe.ExitNode();
        }
    }
}