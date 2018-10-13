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
        static bool Prefix(ref ILoadReferenceable refee, string label)
        {
            // TODO: Please investigate.
            //if ((!(refee is Pawn) && !(refee is WorldObject) && !(refee is IExposable)) || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Uninitialized || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
            //{
            //    return true;
            //}

            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status ==
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
            {
                return true;
            }
            
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    //ReferenceSaveLoader.SaveReferenceFile(exposable);
                    //PersistentWorldManager.ReferenceTable.AddReference((ILoadReferenceable) exposable);
                    
                    Scribe.saver.WriteElement(label, refee.GetUniqueLoadID());
                    break;
                case LoadSaveMode.LoadingVars:
                    var xmlNode = (XmlNode) Scribe.loader.curXmlParent[label];
                    var targetLoadID = xmlNode?.InnerText;
                    
#if DEBUG
                    /*
                    // Reference Debugging.
                    Log.Message("targetloadid: " + targetLoadID);
                    Log.Message("label: " + label);

                    if (refee != null)
                    {
                        Log.Message("Type: " + refee.GetType());
                    }
                    else
                    {
                        Log.Message("Refee is null.");
                    }
                    */
#endif
                    
                    // Prevent default(T) being null in generic method.
                    //var originalType = exposable.GetType();
                    //var genericMethod = GetReferenceMethod.MakeGenericMethod(originalType);
                    //refee = (ILoadReferenceable) genericMethod.Invoke(null, new object[] { targetLoadID });
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