using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Harmony;
using Verse;

namespace PersistentWorlds
{
    public static class ReferenceSaveLoader
    {
        private static readonly Dictionary<string, IExposable> references = new Dictionary<string, IExposable>();
        private const string ReferenceFile_Extension = ".ref";
        
        private static string ReferenceFolder
        {
            get
            {
                var worldFolder = PersistentWorldManager.WorldLoadSaver.GetWorldFolder();
                var referenceFolder = worldFolder + "/References";

                if (!Directory.Exists(referenceFolder))
                    Directory.CreateDirectory(referenceFolder);

                return referenceFolder;
            }
        }

        public static void SaveReferenceFile<T>(T exposable) where T : IExposable
        {
            ILoadReferenceable referencable = null;
            if (!(exposable is ILoadReferenceable)) return;

            referencable = (ILoadReferenceable) exposable;

            if (references.ContainsKey(referencable.GetUniqueLoadID()))
            {
                return;
            }
            else
            {
                references.Add(referencable.GetUniqueLoadID(), exposable);
            }
            
            Scribe.saver.loadIDsErrorsChecker.RegisterReferenced(referencable, referencable.GetUniqueLoadID());
        }

        public static void SaveReferences()
        {
            for (var i = 0; i < references.Count; i++)
            {
                var referencable = references.ElementAt(i);
                
                var file = ReferenceFolder + "/" + referencable.Key + ReferenceFile_Extension;
 
                var reffable = referencable.Value;
                
                SafeSaver.Save(file, "referencefile", delegate
                {
                    reffable.ExposeData();
                });
            }
        }

        private static T LoadReference<T>(string uniqueLoadID) where T : IExposable
        {
            var file = ReferenceFolder + "/" + uniqueLoadID + ReferenceFile_Extension;

            // TODO: Refactor.
            if (Scribe.mode != LoadSaveMode.Inactive)
            {
                ScribeVars.mode = Scribe.mode;
                ScribeVars.curXmlParent = Scribe.loader.curXmlParent;
                ScribeVars.curParent = Scribe.loader.curParent;
                ScribeVars.curPathRelToParent = Scribe.loader.curPathRelToParent;
                
                Scribe.mode = LoadSaveMode.Inactive;
                Scribe.loader.curXmlParent = null;
                Scribe.loader.curParent = null;
                Scribe.loader.curPathRelToParent = null;
            }
            
            Scribe.loader.InitLoading(file);

            var exposable = default(T);

            if (exposable == null)
                throw new NullReferenceException("ReferenceSaveLoader.LoadReference<T>(string) : IExposable of type " +
                                                 typeof(T) + " is null.");
            
            exposable.ExposeData();

            references.Add(uniqueLoadID, exposable);

            if (ScribeVars.mode != LoadSaveMode.Inactive)
            {
                ScribeVars.Reset();
            }

            return exposable;

        }

        public static T GetReference<T>(string uniqueLoadID) where T : IExposable
        {
            if (!references.ContainsKey(uniqueLoadID))
            {
                return LoadReference<T>(uniqueLoadID);
            }
            
            return (T) references[uniqueLoadID];
        }

        public static void ClearReferences()
        {
            references.Clear();
        }
    }
}