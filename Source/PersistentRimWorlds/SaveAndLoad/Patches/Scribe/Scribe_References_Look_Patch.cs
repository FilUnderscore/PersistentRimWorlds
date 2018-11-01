using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch]
    public class Scribe_References_Look_Patch
    {
        #region Fields
        private static readonly MethodInfo LookMethod = typeof(Scribe_References).GetMethods()
            .First(m => m.Name == "Look" && m.IsGenericMethod).MakeGenericMethod(typeof(ILoadReferenceable));
        #endregion
        
        #region Constructors
        static Scribe_References_Look_Patch()
        {
            if(LookMethod == null)
                throw new NullReferenceException($"{nameof(LookMethod)} is null.");
        }
        #endregion
        
        #region Methods
        static bool Prefix(ref ILoadReferenceable refee, string label, bool saveDestroyedThings)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting))
            {
                return true;
            }

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
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
            return LookMethod;
        }
        #endregion
    }
}