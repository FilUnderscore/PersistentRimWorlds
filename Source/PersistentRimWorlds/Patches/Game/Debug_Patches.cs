using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using Harmony;
using JetBrains.Annotations;
using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Video;
using Verse;

namespace PersistentWorlds.Patches
{
    #if DEBUG
    public static class Debug_Patches
    {
        [HarmonyPatch()]
        public static class Look_Patch_Class
        {
            static void Postfix(ref ILoadReferenceable refee, string label)
            {
                if (PersistentWorldManager.WorldLoadSaver.Status !=
                    PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                {
                    return;
                }
                
                Log.Message("Current Mode: " + Scribe.mode.ToString());

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (Scribe.loader.curParent != null && Scribe.loader.curParent.GetType().IsValueType)
                        Log.Warning(
                            "Trying to load reference of an object of type " + (object) refee.GetType() +
                            " with label " + label +
                            ", but our current node is a value type. The reference won't be loaded properly. curParent=" +
                            (object) Scribe.loader.curParent, false);
                    XmlNode xmlNode = (XmlNode) Scribe.loader.curXmlParent[label];
                    string targetLoadID = xmlNode == null ? (string) null : xmlNode.InnerText;

                    Log.Message("Target Load ID: " + targetLoadID);

                    //refee = DynamicCrossRefHandler.loadables[targetLoadID];
                    if(!DynamicCrossRefHandler.requests.ContainsKey(Scribe.loader.curPathRelToParent + "/" + label))
                        DynamicCrossRefHandler.requests.Add(Scribe.loader.curPathRelToParent + "/" + label, targetLoadID);
                }
                else if (Scribe.mode == LoadSaveMode.Saving)
                {
                    refee = DynamicCrossRefHandler.loadables[
                        DynamicCrossRefHandler.requests[Scribe.loader.curPathRelToParent + "/" + label]];
                }
                
                Log.ResetMessageCount();
            }

            static MethodBase TargetMethod()
            {
                return typeof(Scribe_References).GetMethods().First(m => m.Name == "Look" && m.IsGenericMethod).MakeGenericMethod(typeof(ILoadReferenceable));
            }
        }

        /*
        [HarmonyPatch()]
        public static class Look_FactionPatch
        {
            static void Postfix(ref Faction refee, string label)
            {
                
            }

            static MethodBase TargetMethod()
            {
                return typeof(Scribe_References).GetMethods().First(m => m.Name == "Look" && m.IsGenericMethod)
                    .MakeGenericMethod(typeof(Faction));
            }
        }
        */

        /*
        [HarmonyPatch(typeof(Log), "Warning")]
        public static class Log_Temp_Patch
        {
            [HarmonyPrefix]
            public static void Warning(string text, bool ignoreStopLoggingLimit)
            {
                Log.ResetMessageCount();
            }
        }
        */

        [HarmonyPatch(typeof(MapInfo), "ExposeData")]
        public static class MapInfo_ExposeData_Patch
        {
            [HarmonyPrefix]
            public static bool ExposeData_Prefix(MapInfo __instance)
            {
                if (PersistentWorldManager.WorldLoadSaver.Status !=
                    PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
                {
                    return true;
                }

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    var size = new IntVec3();
                    Scribe_Values.Look<IntVec3>(ref size, "size", new IntVec3(), false);
                    Log.Message("Size: " + size.ToString());
                    __instance.Size = size;

                    MapParent parent = null;
                    XmlNode xmlNode = (XmlNode) Scribe.loader.curXmlParent["parent"];
                    string targetLoadID = xmlNode == null ? null : xmlNode.InnerText;

                    parent = (MapParent) DynamicCrossRefHandler.loadables[targetLoadID];
                    __instance.parent = parent;
                }

                return false;
            }
        }
    }
    #endif
}