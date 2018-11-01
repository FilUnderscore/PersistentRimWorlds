using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using PersistentWorlds.Patches;
using Verse;
using PersistentWorlds.Utils;
using FileLog = PersistentWorlds.Utils.FileLog;

namespace PersistentWorlds.SaveAndLoad
{
    /// <summary>
    /// Stores information regarding references in dictionaries/lists for loading persistent worlds.
    /// This is a workaround to the Scribe system that RimWorld uses while not impacting it on normal games.
    /// </summary>
    public sealed class ReferenceTable
    {
        #region Static Fields
        private static readonly FieldInfo CurPathField = AccessTools.Field(typeof(ScribeSaver), "curPath");

        private static readonly FieldInfo LoadedObjectDirectoryField =
            AccessTools.Field(typeof(CrossRefHandler), "loadedObjectDirectory");

        private static readonly FieldInfo AllObjectsByLoadIdField =
            AccessTools.Field(typeof(LoadedObjectDirectory), "allObjectsByLoadID");
        #endregion
        
        #region Static Constructors
        /// <summary>
        /// Static constructor that checks for whether fields exist in a latest RimWorld update, if not then alert
        /// for debugging purposes.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        static ReferenceTable()
        {
            if(CurPathField == null) 
                throw new NullReferenceException($"{nameof(CurPathField)} is null.");
            
            if(LoadedObjectDirectoryField == null) 
                throw new NullReferenceException($"{nameof(LoadedObjectDirectoryField)} + is null.");
            
            if(AllObjectsByLoadIdField == null) 
                throw new NullReferenceException($"{nameof(AllObjectsByLoadIdField)} is null.");
        }
        #endregion
        
        #region Fields
        /// <summary>
        /// Stores requested references for cross-references state.
        ///
        /// First parameter is path of file that requested. This is so it can be removed from this dictionary
        /// when no longer needed.
        ///
        /// Second parameter is the ReferenceRequest itself listed.
        /// </summary>
        private readonly List<ReferenceRequest> requestedReferences = new List<ReferenceRequest>();
        
        /// <summary>
        /// Stores locations to references in memory for cross-references states.
        ///
        /// First parameter is the unique load ID of the reference.
        /// Second parameter is the ReferenceEntry itself.
        /// </summary>
        private readonly Dictionary<string, Reference> references = new Dictionary<string, Reference>();

        /// <summary>
        /// Instance of the PersistentWorldLoadSaver.
        /// </summary>
        private PersistentWorldLoadSaver loadSaver;
        #endregion
        
        #region Constructors
        /// <summary>
        /// The ReferenceTable class constructor.
        /// </summary>
        /// <param name="persistentWorldLoadSaver"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ReferenceTable(PersistentWorldLoadSaver persistentWorldLoadSaver)
        {
            this.loadSaver = persistentWorldLoadSaver;
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Loads the reference (first parameter) with any add-on label depending on the path relative to the parent.
        /// </summary>
        /// <param name="referenceable"></param>
        /// <param name="label"></param>
        public void LoadReferenceIntoMemory(ILoadReferenceable referenceable, string label)
        {
            // Current file of the reference to be assigned as Reference.pathOfFileContainingReference.
            var currentFile = GetCroppedFileName(this.loadSaver.CurrentFile.FullName);
            var pathRelToParent = "";

            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    pathRelToParent = (string) CurPathField.GetValue(Scribe.saver) + "/" + label;

                    if (label == "li")
                    {
                        pathRelToParent += "[" + ScribeSaver_EnterNode_Patch.GetIndexInList(pathRelToParent, label) +
                                           "]";
                    }
                    else if(label == "thing")
                    {
                        pathRelToParent += "[" + ScribeSaver_EnterNode_Patch.GetThingIndex() + "]";
                    }

                    FileLog.Log("LoadReferenceIntoMemory SAVING (pathRelToParent=" + pathRelToParent + ")");
                    break;
                case LoadSaveMode.LoadingVars:
                    pathRelToParent = label;
                    FileLog.Log("LoadReferenceIntoMemory LOADING (pathRelToParent=" + pathRelToParent + ")");
                    break;
                default:
                    throw new InvalidProgramException("Invalid program state.");
            }
            
            var reference = new Reference(currentFile, referenceable);

            if (references.ContainsKey(referenceable.GetUniqueLoadID()))
            {
                Log.Error("There is already a reference entry with the unique load ID of \"" + referenceable.GetUniqueLoadID() + "\"");
                return;
            }

            references.Add(referenceable.GetUniqueLoadID(), reference);
        }

        /// <summary>
        /// Called to request a reference from certain label with IExposable parent with request being uniqueLoadID.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="uniqueLoadId"></param>
        /// <exception cref="InvalidProgramException"></exception>
        public void RequestReference(string label, string uniqueLoadId)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                throw new InvalidProgramException("RequestReference called when Scribe.mode != LoadingVars.");
            }

            var request = new ReferenceRequest(uniqueLoadId, label, Scribe.loader.curParent);
            
            requestedReferences.Add(request);
        }

        /// <summary>
        /// Called to resolve the reference to the previously mentioned label as in RequestReference(string, string).
        /// </summary>
        /// <param name="label"></param>
        /// <returns>ILoadReferenceable - Resolved reference</returns>
        /// <exception cref="NullReferenceException">Thrown when the Reference cannot be resolved, because
        /// it doesn't exist.</exception>
        public ILoadReferenceable ResolveReference(string label)
        {
            for (var i = 0; i < requestedReferences.Count; i++)
            {
                var requestedReference = requestedReferences[i];
                
                var flag = requestedReference.Label == label &&
                           Equals(requestedReference.Parent, Scribe.loader.curParent);

                if (!flag) continue;

                requestedReferences.Remove(requestedReference);
                
                return requestedReference.LoadIDRequested != null && references.ContainsKey(requestedReference.LoadIDRequested) ? references[requestedReference.LoadIDRequested].Referenceable : null;
            }
    
            throw new NullReferenceException("Reference could not be resolved. (label=" + label + ", curParent=" + Scribe.loader.curParent + ", curPathRelToParent=" + Scribe.loader.curPathRelToParent + ")");
        }

        /// <summary>
        /// Do a quick check to see whether or not a loaded reference with the specific uniqueLoadID exists or not
        /// in the dictionary.
        /// </summary>
        /// <param name="uniqueLoadId"></param>
        /// <returns></returns>
        public bool ContainsReferenceWithLoadId(string uniqueLoadId)
        {
            return references.ContainsKey(uniqueLoadId);
        }

        /// <summary>
        /// Clears the references from memory (stored in the dictionaries/lists.)
        /// </summary>
        public void ClearReferences()
        {
            references.Clear();
            requestedReferences.Clear();
        }

        /// <summary>
        /// Clears the references from memory that are loaded from a certain file path.
        /// </summary>
        /// <param name="filePath">The file path that references were loaded from that will be cleared.</param>
        /// <param name="forced">Force clear references.</param>
        /// <exception cref="InvalidProgramException">Thrown if Scribe.mode is not equal to
        /// LoadSaveMode.LoadingVars</exception>
        public void ClearReferencesFor(string filePath, bool forced = false)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars && !forced)
            {
                throw new InvalidProgramException("ClearReferencesFor(string): Invalid program state. Scribe.mode != LoadingVars.");
            }
            
            var file = GetCroppedFileName(filePath);

            var loadedObjectDirectory = LoadedObjectDirectoryField.GetValue(Scribe.loader.crossRefs);
            var allObjectsByLoadIdDict =
                (Dictionary<string, ILoadReferenceable>) AllObjectsByLoadIdField.GetValue(loadedObjectDirectory);
            
            for(var i = 0; i < references.Count; i++)
            {
                var reference = references.ElementAt(i);

                if (reference.Value.PathOfFileContainingReference != file) continue;

                references.Remove(reference.Key);

                if (allObjectsByLoadIdDict.ContainsKey(reference.Key))
                {
                    allObjectsByLoadIdDict.Remove(reference.Key);
                }
            }
        }
        
        // Debug code here.
        
#if DEBUG
        /// <summary>
        /// Dumps the contents of the Reference Table in the debug_log.txt file in the RimWorld game folder.
        /// </summary>
        public void DumpReferenceTable()
        {
            FileLog.Log("Dumped Reference Table ---");

            for (var i = 0; i < references.Count; i++)
            {
                FileLog.Log(i + ": (uniqueLoadID=" + references.ElementAt(i).Key + ") - " + references.ElementAt(i).Value);
            }
            
            FileLog.Log("End of Dump ---");
        }

        /// <summary>
        /// Dumps the contents of the Reference Request Table in the debug_log.txt file in the RimWorld game folder.
        /// </summary>
        public void DumpReferenceRequestTable()
        {
            FileLog.Log("Dumped Reference Request Table ---");

            for (var i = 0; i < requestedReferences.Count; i++)
            {
                FileLog.Log(i + ": " + requestedReferences[i]); 
            }
            
            FileLog.Log("End of Dump ---");
        }
#endif
        public override string ToString()
        {
            return $"{nameof(ReferenceTable)} " +
                   $"({nameof(references)}={references.ToDebugString()}, " +
                   $"{nameof(requestedReferences)}={requestedReferences.ToDebugString()})";
        }

        /// <summary>
        /// Crops the file name to a smaller length so that all the unnecessary information before the world folder name
        /// are cropped, including the world folder name.
        /// </summary>
        /// <param name="fileName">The filename that will be cropped.</param>
        /// <returns>The cropped filename.</returns>
        private static string GetCroppedFileName(string fileName)
        {
            const string indexString = "\\Saves\\";

            fileName = fileName.Substring(fileName.IndexOf(indexString, StringComparison.Ordinal) + indexString.Length);
            fileName = fileName.Substring(fileName.IndexOf("\\", StringComparison.Ordinal) + 1);

            return fileName;
        }
        #endregion

        #region Classes
        /// <summary>
        /// Used to be stored in the references dictionary for referencing during loading.
        /// </summary>
        private sealed class Reference
        {
            #region Fields
            /// <summary>
            /// To be used for unloading references when they are no longer needed. This contains the path of the file
            /// containing this reference.
            /// </summary>
            private readonly string pathOfFileContainingReference;

            //private readonly string pathRelToParent; // Path relevant to parent. Don't know if this is used yet?
            
            /// <summary>
            /// Link to the reference in memory.
            /// </summary>
            private readonly ILoadReferenceable reffable;
            #endregion
            
            #region Properties
            public string PathOfFileContainingReference => pathOfFileContainingReference;

            public ILoadReferenceable Referenceable => reffable;
            #endregion
            
            #region Constructors
            /// <summary>
            /// The main constructor for the Reference class.
            /// </summary>
            /// <param name="pathOfFileContainingReference">The path of the file containing this reference.</param>
            /// <param name="reffable">The link to the loaded reference in memory.</param>
            public Reference(string pathOfFileContainingReference, ILoadReferenceable reffable)
            {
                this.pathOfFileContainingReference = pathOfFileContainingReference;
                this.reffable = reffable;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Converts this class to a string representation.
            /// </summary>
            /// <returns>This class in a string representation.</returns>
            public override string ToString()
            {
                return $"{nameof(ReferenceTable)}.{nameof(Reference)} " +
                       $"({nameof(pathOfFileContainingReference)}={pathOfFileContainingReference}, " +
                       $"{nameof(reffable)}={reffable})";
            }
            #endregion
        }

        /// <summary>
        /// Used for requesting a reference that hasn't been loaded yet but knows the load ID of that reference.
        /// </summary>
        private sealed class ReferenceRequest
        {
            #region Fields
            /// <summary>
            /// The requested load ID.
            /// </summary>
            private readonly string loadIdRequested;
            
            /// <summary>
            /// The label from which this request is from. Used for identification further on when loading.
            /// </summary>
            private readonly string label;
            
            /// <summary>
            /// Used to track which request this is, by checking it against Scribe.loader.curParent to resolve the
            /// request.
            /// </summary>
            private readonly IExposable parent;
            #endregion
            
            #region Properties
            public string LoadIDRequested => loadIdRequested;
            public string Label => label;
            
            public IExposable Parent => parent;
            #endregion
            
            #region Constructors
            /// <summary>
            /// The main constructor for the ReferenceRequest class.
            /// </summary>
            /// <param name="loadIdRequested">The load ID that is being requested.</param>
            /// <param name="label">The label that this request can be identified with.</param>
            /// <param name="parent">The parent that allows this request to be tracked, comparing
            /// Scribe.loader.curParent with this parent.</param>
            public ReferenceRequest(string loadIdRequested, string label, IExposable parent)
            {
                this.loadIdRequested = loadIdRequested;
                this.label = label;

                this.parent = parent;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Returns a string representation of this class.
            /// </summary>
            /// <returns>Returns a string representation of this class.</returns>
            public override string ToString()
            {
                return $"{nameof(ReferenceTable)}.{nameof(ReferenceRequest)} " +
                       $"({nameof(loadIdRequested)}={loadIdRequested}, " +
                       $"{nameof(label)}={label}, " +
                       $"{nameof(parent)}={parent})";
            }
            #endregion
        }
        #endregion
    }
}