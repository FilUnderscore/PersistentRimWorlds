using System.Linq;
using System.Reflection;
using System.Xml;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch]
    public class Scribe_References_Look_Patch
    {
        static bool Prefix(ref ILoadReferenceable refee, string label, bool saveDestroyedThings)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status ==
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
            {
                return true;
            }
            
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    if (refee == null)
                    {
                        Scribe.saver.WriteElement(label, "null");
                        break;
                    }

                    if (refee is Thing thing &&
                        Scribe_References.CheckSaveReferenceToDestroyedThing(thing, label, saveDestroyedThings))
                        break;
                    
                    Scribe.saver.WriteElement(label, refee.GetUniqueLoadID());
                    Scribe.saver.loadIDsErrorsChecker.RegisterReferenced(refee, label);
                    break;
                case LoadSaveMode.LoadingVars:
                    var xmlNode = (XmlNode) Scribe.loader.curXmlParent[label];
                    var targetLoadID = xmlNode?.InnerText;
                    
                    PersistentWorldManager.ReferenceTable.RequestReference(label, targetLoadID);
                    break;
                case LoadSaveMode.ResolvingCrossRefs:
                    refee = PersistentWorldManager.ReferenceTable.ResolveReference(label);
                    break;
            }
            
            return false;
        }

        static MethodBase TargetMethod()
        {
            return typeof(Scribe_References).GetMethods().First(m => m.Name == "Look" && m.IsGenericMethod)
                .MakeGenericMethod(typeof(ILoadReferenceable));
        }
    }
}