using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public sealed class ReferenceTable
    {
        /// <summary>
        /// Stores requested references for cross-references state.
        ///
        /// First parameter is path of file that requested.
        /// Second parameter is ReferenceRequest.
        /// </summary>
        private Dictionary<string, List<ReferenceRequest>> requestedReferences = new Dictionary<string, List<ReferenceRequest>>();
            
        /// <summary>
        /// Stores locations to references in memory.
        ///
        /// First parameter is the unique ID of the reference.
        /// Second parameter is the ReferenceEntry.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, ReferenceEntry> referenceEntryDict = new Dictionary<string, ReferenceEntry>();

        private static string ReferenceTableFileMapPath => PersistentWorldManager.WorldLoadSaver.GetWorldFolder() + "/references.reftable";

        public void LoadReferences()
        {
            using (var reader = new StreamReader(ReferenceTableFileMapPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var referenceEntry = ReferenceEntry.FromString(line);
                    
                    referenceEntryDict.Add(referenceEntry.uniqueLoadID, referenceEntry);
                }
            }
        }

        public void SaveReferences()
        {
            using (var writer = new StreamWriter(ReferenceTableFileMapPath, false))
            {
                foreach (var referenceSet in referenceEntryDict)
                {
                    writer.WriteLine(referenceSet.Value.ToString());
                }
            }
        }

        public void AddReference(ILoadReferenceable reference)
        {
            var currentFile = PersistentWorldManager.WorldLoadSaver.currentFile.FullName;
            var pathRelToParent = Scribe.loader.curPathRelToParent;
            
            var referenceEntry = new ReferenceEntry(currentFile, pathRelToParent);
            referenceEntry.LoadReference(reference);

            if (referenceEntryDict.ContainsKey(referenceEntry.uniqueLoadID))
            {
                Log.Error("There is already a reference entry with the unique load ID \"" + referenceEntry.uniqueLoadID + "\"!");
                return;
            }

            referenceEntryDict.Add(referenceEntry.uniqueLoadID, referenceEntry);
        }

        public void LoadReference(ILoadReferenceable referenceable)
        {
            referenceEntryDict[referenceable.GetUniqueLoadID()].LoadReference(referenceable);

            foreach (var referenceRequestSet in requestedReferences)
            {
                foreach (var referenceRequest in referenceRequestSet.Value)
                {
                    if (referenceRequest.uniqueLoadIDRequested == referenceable.GetUniqueLoadID())
                    {
                        referenceRequest.linkToReferenceEntry = referenceEntryDict[referenceable.GetUniqueLoadID()];
                    }
                }
            }
        }

        public void RequestReference(string label, string uniqueLoadID)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                Log.Error(nameof(RequestReference) + " called when Scribe.mode != LoadSaveMode.LoadingVars.");
                return;
            }
            
            var currentFile = PersistentWorldManager.WorldLoadSaver.currentFile.FullName;
            var pathRelToParent = Scribe.loader.curPathRelToParent + "/" + label;

            if (requestedReferences.ContainsKey(currentFile))
            {
                requestedReferences[currentFile].Add(new ReferenceRequest(pathRelToParent, uniqueLoadID));
            }
            else
            {
                requestedReferences.Add(currentFile, new List<ReferenceRequest>() { new ReferenceRequest(pathRelToParent, uniqueLoadID) });
            }
        }

        public ILoadReferenceable ResolveReference(string label)
        {
            var currentFile = PersistentWorldManager.WorldLoadSaver.currentFile.FullName;
            var pathRelToParent = Scribe.loader.curPathRelToParent + "/" + label;

            foreach (var requestedReference in requestedReferences[currentFile])
            {
                if (requestedReference.pathFromParentThatRequested == pathRelToParent)
                {
                    return requestedReference.linkToReferenceEntry.reference;
                }
            }

            throw new NullReferenceException("Could not resolve reference at " + currentFile + ":" + pathRelToParent);
        }

        private sealed class ReferenceEntry
        {
            public string fileLocation; // E.g. file path
            public string pathToReference; // E.g. .../world/info/...

            public string uniqueLoadID;
            public ILoadReferenceable reference;

            public ReferenceEntry(string fileLocation, string pathToReference)
            {
                this.fileLocation = fileLocation;
                this.pathToReference = pathToReference;
            }
            
            public void LoadReference(ILoadReferenceable reference)
            {
                uniqueLoadID = reference.GetUniqueLoadID();
                this.reference = reference;
            }

            public override string ToString()
            {
                return uniqueLoadID + ":" + fileLocation + ":" + pathToReference;
            }

            public static ReferenceEntry FromString(string str)
            {
                var args = str.Split(':');
                
                return new ReferenceEntry(args[1], args[2]) { uniqueLoadID = args[0] };
            }
        }

        private sealed class ReferenceRequest
        {
            public string pathFromParentThatRequested;
            public string uniqueLoadIDRequested;

            public ReferenceEntry linkToReferenceEntry;

            public ReferenceRequest(string pathFromParentThatRequested, string uniqueLoadIdRequested)
            {
                this.pathFromParentThatRequested = pathFromParentThatRequested;
                this.uniqueLoadIDRequested = uniqueLoadIdRequested;
            }
        }
    }
}