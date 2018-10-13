using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using PersistentWorlds.Patches;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public class ReferenceTable
    {
        private static readonly FieldInfo curPathField = AccessTools.Field(typeof(ScribeSaver), "curPath");

        /// <summary>
        /// Stores requested references for cross-references state.
        ///
        /// First parameter is path of file that requested. This is so it can be removed from this dictionary
        /// when no longer needed.
        ///
        /// Second parameter is the ReferenceRequest itself listed.
        /// </summary>
        //private Dictionary<string, List<ReferenceRequest>> requestedReferences =
        //    new Dictionary<string, List<ReferenceRequest>>();

        private List<ReferenceRequest> requestedReferences = new List<ReferenceRequest>();
        
        /// <summary>
        /// Stores locations to references in memory for cross-references states.
        ///
        /// First parameter is the unique load ID of the reference.
        /// Second parameter is the ReferenceEntry itself.
        /// </summary>
        private Dictionary<string, Reference> references = new Dictionary<string, Reference>();

        /// <summary>
        /// Loads the reference (first parameter) with any add-on label depending on the path relative to the parent.
        /// </summary>
        /// <param name="referenceable"></param>
        /// <param name="label"></param>
        public void LoadReferenceIntoMemory(ILoadReferenceable referenceable, string label)
        {
            // Current file of the reference to be assigned as Reference.pathOfFileContainingReference.
            var currentFile = GetCroppedFileName(PersistentWorldManager.WorldLoadSaver.currentFile.FullName);
            var pathRelToParent = "";

            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    pathRelToParent = (string) curPathField.GetValue(Scribe.saver) + "/" + label;

                    if (label == "li")
                    {
                        pathRelToParent += "[" + ScribeSaver_EnterNode_Patch.GetIndexInList(pathRelToParent, label) +
                                           "]";
                    }
                    else if(label == "thing")
                    {
                        pathRelToParent += "[" + ScribeSaver_EnterNode_Patch.GetThingIndex() + "]";
                    }

                    Debug.FileLog.Log("LoadReferenceIntoMemory SAVING (pathRelToParent=" + pathRelToParent + ")");
                    break;
                case LoadSaveMode.LoadingVars:
                    pathRelToParent = label;
                    Debug.FileLog.Log("LoadReferenceIntoMemory LOADING (pathRelToParent=" + pathRelToParent + ")");
                    break;
                default:
                    throw new InvalidProgramException("Invalid program state.");
            }
            
            var reference = new Reference(currentFile, pathRelToParent, referenceable);

            if (references.ContainsKey(referenceable.GetUniqueLoadID()))
            {
                Log.Error("There is already a reference entry with the unique load ID of \"" + referenceable.GetUniqueLoadID() + "\"");
                return;
            }

            references.Add(referenceable.GetUniqueLoadID(), reference);
        }

        public void RequestReference(string label, string uniqueLoadID)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                throw new InvalidProgramException("RequestReference called when Scribe.mode != LoadingVars.");
            }

            var currentFile = GetCroppedFileName(PersistentWorldManager.WorldLoadSaver.currentFile.FullName);
            var pathRelToParent = Scribe.loader.curPathRelToParent + "/" + label;

            var request = new ReferenceRequest(uniqueLoadID, label, Scribe.loader.curParent);

            requestedReferences.Add(request);
        }

        public ILoadReferenceable ResolveReference(string label)
        {
            var currentFile = GetCroppedFileName(PersistentWorldManager.WorldLoadSaver.currentFile.FullName);
            var pathRelToParent = Scribe.loader.curPathRelToParent + "/" + label;

            foreach (var requestedReference in requestedReferences)
            {
                var flag = requestedReference.Label == label &&
                           Equals(requestedReference.Parent, Scribe.loader.curParent);

                if (!flag) continue;
                
                return requestedReference.LoadIDRequested != null && references.ContainsKey(requestedReference.LoadIDRequested) ? references[requestedReference.LoadIDRequested].Referenceable : null;
            }
    
            throw new NullReferenceException("Reference could not be resolved. (label=" + label + ", curParent=" + Scribe.loader.curParent + ", curPathRelToParent=" + Scribe.loader.curPathRelToParent + ")");
        }

        public void ClearReferences()
        {
            // TODO: ...
        }
        
#if DEBUG
        public void DumpReferenceTable()
        {
            Debug.FileLog.Log("Dumped Reference Table ---");

            for (var i = 0; i < references.Count; i++)
            {
                Debug.FileLog.Log(i + ": (uniqueLoadID=" + references.ElementAt(i).Key + ") - " + references.ElementAt(i).Value);
            }
            
            Debug.FileLog.Log("End of Dump ---");
        }

        public void DumpReferenceRequestTable()
        {
            Debug.FileLog.Log("Dumped Reference Request Table ---");

            for (var i = 0; i < requestedReferences.Count; i++)
            {
                Debug.FileLog.Log(i + ": " + requestedReferences[i]); 
            }
            
            Debug.FileLog.Log("End of Dump ---");
        }
#endif
        // TODO: Unload existing references on loading other colony.

        private string GetCroppedFileName(string fileName)
        {
            const string indexString = "\\Saves\\";

            fileName = fileName.Substring(fileName.IndexOf(indexString, StringComparison.Ordinal) + indexString.Length);
            fileName = fileName.Substring(fileName.IndexOf("\\", StringComparison.Ordinal) + 1);

            return fileName;
        }
        
        #region Classes
        private sealed class Reference
        {
            private string pathOfFileContainingReference; // To be used for unloading reference when not needed.

            private string pathRelToParent; // Path relevant to parent. Don't know if this is used yet?
            private ILoadReferenceable reffable; // Link to reference.

            #region Properties
            public string PathOfFileContainingReference => pathOfFileContainingReference;
            public string PathRelToParent => pathRelToParent;
            
            public ILoadReferenceable Referenceable => reffable;
            #endregion
            
            public Reference(string pathOfFileContainingReference, string pathRelToParent, ILoadReferenceable reffable)
            {
                this.pathOfFileContainingReference = pathOfFileContainingReference;
                this.pathRelToParent = pathRelToParent;
                this.reffable = reffable;
            }

            public override string ToString()
            {
                return "(pathOfFileContainingReference=" + pathOfFileContainingReference + ", pathRelToParent=" +
                       pathRelToParent + ", reffable=" + reffable + ")";
            }
        }

        private sealed class ReferenceRequest
        {
            private string loadIDRequested;
            
            private string label; // Label for loading.
            private IExposable parent; // Used to check Scribe.loader.curParent is the same, then load this request.

            public string LoadIDRequested => loadIDRequested;
            public string Label => label;
            
            public IExposable Parent => parent;
            
            public ReferenceRequest(string loadIdRequested, string label, IExposable parent)
            {
                this.loadIDRequested = loadIdRequested;
                this.label = label;

                this.parent = parent;
            }

            public override string ToString()
            {
                return "(loadIDRequested=" + loadIDRequested + ", label=" + label + ", parent=" + parent +
                       ")";
            }
        }
        #endregion
    }
}