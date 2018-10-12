using System.Xml;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(MapInfo), "ExposeData")]
    public class MapInfo_ExposeData_Patch
    {
        #region Methods
        static bool Prefix(MapInfo __instance)
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

                MapParent parent = new MapParent();
                XmlNode xmlNode = (XmlNode) Scribe.loader.curXmlParent["parent"];
                string targetLoadID = xmlNode == null ? null : xmlNode.InnerText;

                Log.Message("targetLoadID: " + targetLoadID);
                
                Scribe_References.Look<MapParent>(ref parent, targetLoadID);
            }
            else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                __instance.parent = (MapParent) PersistentWorldManager.ReferenceTable.ResolveReference("parent");
            }

            return false;
        }
        #endregion
    }
}