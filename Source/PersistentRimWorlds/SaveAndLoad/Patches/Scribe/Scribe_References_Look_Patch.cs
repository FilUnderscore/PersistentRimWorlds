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
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting))
            {
                return true;
            }

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;

            if (persistentWorld == null)
            {
                Log.Error("Persistent world is null!");

                return false;
            }
            else
            {
                if (persistentWorld.LoadSaver == null)
                {
                    Log.Error("PersistentWorld Load Saver is null.");

                    return false;
                }
                else
                {
                    if (persistentWorld.LoadSaver.ReferenceTable == null)
                    {
                        Log.Error("PersistentWorld Load Saver Reference Table is null.");

                        return false;
                    }
                    else
                    {
                        Log.Message("Nothing is null?");
                    }
                }
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
                    
                    persistentWorld.LoadSaver.ReferenceTable.RequestReference(label, targetLoadID);

                    if (refee != null)
                    {
                        Scribe.loader.crossRefs.loadIDs.RegisterLoadIDReadFromXml(targetLoadID, refee.GetType(), label);
                    }
                    break;
                case LoadSaveMode.ResolvingCrossRefs:
                    refee = persistentWorld.LoadSaver.ReferenceTable.ResolveReference(label);
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