using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Xml;
using Harmony;
using RimWorld.Planet;
using UnityEngine.Rendering;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch]
    public class Scribe_References_Look_Patch
    {
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
                    
                    //Log.Message("targetloadid: " + targetLoadID);
                    //Log.Message("label: " + label);
                    
                    refee = (ILoadReferenceable) ReferenceSaveLoader.GetReference<IExposable>(targetLoadID);
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