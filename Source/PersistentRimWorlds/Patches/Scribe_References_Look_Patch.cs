using System.Linq;
using System.Reflection;
using System.Xml;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch()]
    public static class Scribe_References_Look_Patch
    {
        static void Postfix(ref ILoadReferenceable refee, string label)
        {
            if (PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame)
            {
                return;
            }
                
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
}