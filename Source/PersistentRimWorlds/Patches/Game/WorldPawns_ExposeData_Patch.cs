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
        /*
        public static bool Prefix(WorldPawns __instance)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars && Scribe.mode != LoadSaveMode.Saving)
            {
                Log.Message("Call prefixs");
                
                var mothballedField = AccessTools.Field(typeof(WorldPawns), "pawnsMothballed");
                var mothballed = (HashSet<Pawn>) mothballedField.GetValue(__instance);
                
                BackCompatibility.WorldPawnPostLoadInit(__instance, ref mothballed);

                if (mothballed == null)
                {
                    mothballed = new HashSet<Pawn>();
                }
                
                mothballedField.SetValue(__instance, mothballed);

                // TODO: Investigate why is this null ??/
                var aliveField = AccessTools.Field(typeof(WorldPawns), "pawnsAlive");

                if (aliveField.GetValue(__instance) == null)
                {
                    aliveField.SetValue(__instance, new HashSet<Pawn>());
                }
                
                return false;
            }

            return true;
        }
        */
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr,
            ILGenerator ilGenerator)
        {
            var codes = new List<CodeInstruction>(instr);

            var label1 = ilGenerator.DefineLabel();
            codes[0].labels.Add(label1);

            var label2 = ilGenerator.DefineLabel();
            var label3 = ilGenerator.DefineLabel();
            
            var toInsert = new List<CodeInstruction>();

            toInsert.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PersistentWorldManager), "WorldLoadSaver")));
            toInsert.Add(new CodeInstruction(OpCodes.Brfalse_S, label1));

            toInsert.Add(new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(PersistentWorldManager), "WorldLoadSaver")));
            toInsert.Add(new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(PersistentWorldLoadSaver), "Status")));
            toInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_2));
            toInsert.Add(new CodeInstruction(OpCodes.Bne_Un_S, label2));
            
            codes.InsertRange(0, toInsert);

            toInsert = new List<CodeInstruction>();

            toInsert.Add(new CodeInstruction(OpCodes.Br_S, label3));
            
            toInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
            toInsert[1].labels.Add(label2);

            toInsert.Add(new CodeInstruction(OpCodes.Ldflda,
                AccessTools.Field(typeof(WorldPawns), "pawnsForcefullyKeptAsWorldPawns")));
            toInsert.Add(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(WorldPawns_ExposeData_Patch), "Load")));
            
            codes.InsertRange(12, toInsert);

            codes[16].labels.Add(label3);
            
            //codes.Do(code => Log.Message(code.ToString(), true));
            
            return codes.AsEnumerable();
        }

        public static void Load(ref HashSet<Pawn> pawns)
        {
            if (Scribe.EnterNode("pawnsForcefullyKeptAsWorldPawns"))
            {Log.Message("Enter node");
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
                            Log.Message("Pawn: " + pawn.GetUniqueLoadID());
                            var refee = pawn;
                            Scribe_References.Look<Pawn>(ref refee, "li", true);
                        }
                        
                        break;
                    case LoadSaveMode.LoadingVars:
                        Log.Message("yes");
                        var curXmlParent = Scribe.loader.curXmlParent;
                        var attribute = curXmlParent.Attributes["IsNull"];
                        if (attribute != null && attribute.Value.ToLower() == "true")
                        {
                            pawns = null;
                            Log.Message("None");
                            break;
                        }
                        else
                        {
                            pawns = new HashSet<Pawn>();
                        }
                        Log.Message("Childs: " + curXmlParent.ChildNodes.Count);

                        foreach (var node in curXmlParent.ChildNodes)
                        {
                            var nodeText = ((XmlNode) node).InnerText;
                            Log.Message("Node: " + nodeText);
                            
                            var pawn = new Pawn();
                            Scribe_References.Look<Pawn>(ref pawn, nodeText);
                            pawns.Add(pawn);
                        }

                        break;
                    default:
                        Log.Message("Default load");
                        break;
                }
                
                Scribe.ExitNode();
            }
            else
            {
                Log.Message("Unable to enter node");
            }
            
            Log.Message("Done");
        }
    }
}