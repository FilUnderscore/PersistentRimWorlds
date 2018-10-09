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
            static void Postfix(ref MapParent refee, string label)
            {
                if (PersistentWorldManager.WorldLoadSaver.Status !=
                    PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame || label != "parent")
                {
                    return;
                }

                if (Scribe.loader.curParent != null && Scribe.loader.curParent.GetType().IsValueType)
                    Log.Warning("Trying to load reference of an object of type " + (object) refee.GetType() + " with label " + label + ", but our current node is a value type. The reference won't be loaded properly. curParent=" + (object) Scribe.loader.curParent, false);
                XmlNode xmlNode = (XmlNode) Scribe.loader.curXmlParent[label];
                string targetLoadID = xmlNode == null ? (string) null : xmlNode.InnerText;

                Log.Message("Target Load ID: " + targetLoadID);
                
                refee = (MapParent) DynamicCrossRefHandler.loadables[targetLoadID];
            }

            static MethodBase TargetMethod()
            {
                return typeof(Scribe_References).GetMethods().First(m => m.Name == "Look" && m.IsGenericMethod).MakeGenericMethod(typeof(MapParent));
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

        [HarmonyPatch(typeof(Log), "Warning")]
        public static class Log_Temp_Patch
        {
            [HarmonyPrefix]
            public static void Warning(string text, bool ignoreStopLoggingLimit)
            {
                Log.ResetMessageCount();
            }
        }
    }
    #endif
}