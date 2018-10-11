using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds
{
    public static class ReferenceSaveLoader
    {
        // TODO: Append references to files that call Look first instead of loading from separate files to have less files.
        
        private static readonly Dictionary<string, Dictionary<string, IExposable>> references = new Dictionary<string, Dictionary<string, IExposable>>();
        //private const string ReferenceFile_Extension = ".ref";
        
        /*
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
        */

        public static void SaveReferenceFile<T>(T exposable) where T : IExposable
        {
            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Log.Error("SaveReferenceFile called when Scribe mode is not saving.");
                return;
            }
            
            ILoadReferenceable referencable = null;
            if (!(exposable is ILoadReferenceable)) return;

            referencable = (ILoadReferenceable) exposable;

            var file = PersistentWorldManager.WorldLoadSaver.currentFile.FullName;
            
            /*
            if (references.ContainsKey(referencable.GetUniqueLoadID()))
            {
                return;
            }
            else
            {
                references.Add(referencable.GetUniqueLoadID(), exposable);

                if (filesForReferences.ContainsKey(PersistentWorldManager.WorldLoadSaver.currentFile.FullName))
                    filesForReferences[PersistentWorldManager.WorldLoadSaver.currentFile.FullName]
                        .Add(referencable.GetUniqueLoadID());
                else
                {
                    filesForReferences.Add(PersistentWorldManager.WorldLoadSaver.currentFile.FullName,
                        new List<string>() {referencable.GetUniqueLoadID()});
                }
            }
            */

            if (references.ContainsKey(file))
            {
                if (references[file].ContainsKey(referencable.GetUniqueLoadID()))
                {
                    return;
                }
                else
                {
                    references[file].Add(referencable.GetUniqueLoadID(), exposable);
                }
            }
            else
            {
                // TODO: Check if reference exists in another file...
                
                references.Add(file, new Dictionary<string, IExposable>() {{referencable.GetUniqueLoadID(), exposable}});
            }
            
            Scribe.saver.loadIDsErrorsChecker.RegisterReferenced(referencable, referencable.GetUniqueLoadID());
            Scribe.saver.loadIDsErrorsChecker.RegisterDeepSaved(referencable, referencable.GetUniqueLoadID());
        }

        public static void SaveReferencesForCurrentFile()
        {
            var file = PersistentWorldManager.WorldLoadSaver.currentFile.FullName;

            if (!references.ContainsKey(file))
            {
                Log.Message("Returning for " + file);
                return;
            }
            
            var reference = references[file];
            Scribe_Collections.Look(ref reference, "references", LookMode.Value, LookMode.Deep);
        }
        
        /*
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
        */

        private static T LoadReference<T>(string uniqueLoadID) where T : IExposable, new()
        {
            /*
            var file = ReferenceFolder + "/" + uniqueLoadID + ReferenceFile_Extension;

            if (Scribe.mode != LoadSaveMode.Inactive)
            {
                ScribeVars.Set();
                
                ScribeVars.TrickScribe();
            }
            
            Scribe.loader.InitLoading(file);

            var exposable = new T();

            if (exposable == null)
                throw new NullReferenceException("ReferenceSaveLoader.LoadReference<T>(string) : IExposable of type " +
                                                 typeof(T) + " is null.");
            
            exposable.ExposeData();

            references.Add(uniqueLoadID, exposable);

            if (ScribeVars.mode != LoadSaveMode.Inactive)
            {
                ScribeVars.Reset();
            }
            else
            {
                // Clear up errors.
                ScribeVars.TrickScribe();
            }

            return exposable;
            */

            return default(T);
        }

        public static T GetReference<T>(string uniqueLoadID) where T : IExposable, new()
        {
            if (!references.ContainsKey(uniqueLoadID))
            {
                return LoadReference<T>(uniqueLoadID);
            }
            
            foreach (var reference in references[uniqueLoadID])
            {
                if (reference.Key == uniqueLoadID)
                {
                    return (T) reference.Value;
                }
            }
            
            Log.Error("Reference " + uniqueLoadID + " is null.");

            return default(T);
        }

        public static void ClearReferences()
        {
            references.Clear();
        }
    }
}