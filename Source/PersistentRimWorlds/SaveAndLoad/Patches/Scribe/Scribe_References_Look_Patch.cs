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
        private static readonly MethodInfo GetReferenceMethod = AccessTools.Method(typeof(ReferenceSaveLoader),
            "GetReference", new[] {typeof(string)});
        
        static bool Prefix(ref ILoadReferenceable refee, string label)
        {
            if ((!(refee is Pawn) && !(refee is WorldObject) && !(refee is IExposable)) || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Uninitialized || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
            {
                return true;
            }

            var exposable = (IExposable) refee;
            
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    ReferenceSaveLoader.SaveReferenceFile(exposable);
                    
                    Scribe.saver.WriteElement(label, refee.GetUniqueLoadID());
                    break;
                case LoadSaveMode.LoadingVars:
                    var xmlNode = (XmlNode) Scribe.loader.curXmlParent[label];
                    var targetLoadID = xmlNode == null ? label : xmlNode.InnerText;
                    
#if DEBUG
                    // Reference Debugging.
                    //Log.Message("targetloadid: " + targetLoadID);
                    //Log.Message("label: " + label);
                    //Log.Message("Type: " + exposable.GetType());
#endif
                    
                    // Prevent default(T) being null in generic method.
                    var originalType = exposable.GetType();
                    var genericMethod = GetReferenceMethod.MakeGenericMethod(originalType);
                    refee = (ILoadReferenceable) genericMethod.Invoke(null, new object[] { targetLoadID });
                    
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