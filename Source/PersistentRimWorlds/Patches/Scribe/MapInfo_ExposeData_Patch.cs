using System.Xml;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MapInfo), "ExposeData")]
    public static class MapInfo_ExposeData_Patch
    {
        [HarmonyPrefix]
        public static bool ExposeData_Prefix(MapInfo __instance)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
            {
                return true;
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                var size = new IntVec3();
                Scribe_Values.Look<IntVec3>(ref size, "size", new IntVec3(), false);
                __instance.Size = size;

                MapParent parent = null;
                XmlNode xmlNode = (XmlNode) Scribe.loader.curXmlParent["parent"];
                string targetLoadID = xmlNode == null ? null : xmlNode.InnerText;

                Log.Message("targetLoadID: " + targetLoadID);
                
                Scribe_References.Look<MapParent>(ref parent, targetLoadID);
                
                __instance.parent = parent;
            }

            return false;
        }
    }
}