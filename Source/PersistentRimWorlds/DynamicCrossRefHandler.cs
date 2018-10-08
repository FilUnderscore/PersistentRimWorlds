using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds
{
    /*
     * Dynamically cross-reference objects during runtime.
     */
    public class DynamicCrossRefHandler
    {
        private static Dictionary<string, ILoadReferenceable> loadables = new Dictionary<string, ILoadReferenceable>();
        private static List<IExposable> exposables = new List<IExposable>();

        private static List<IdRecord> idsRead = new List<IdRecord>();
        private static List<IdListRecord> idListsRead = new List<IdListRecord>();
        
        /*
         * Fields
         */
        
        private static readonly FieldInfo allObjectsByLoadIDField =
            AccessTools.Field(typeof(LoadedObjectDirectory), "allObjectsByLoadID");
        
        private static readonly FieldInfo loadedObjectDirectoryField =
            AccessTools.Field(typeof(CrossRefHandler), "loadedObjectDirectory");

        private static readonly FieldInfo crossReferencingExposablesField =
            AccessTools.Field(typeof(CrossRefHandler), "crossReferencingExposables");

        private static readonly FieldInfo idsReadField = AccessTools.Field(typeof(LoadIDsWantedBank), "idsRead");
        private static readonly FieldInfo idListsReadField = AccessTools.Field(typeof(LoadIDsWantedBank), "idListsRead");

        /*
         * Lists
         */
        
        private static MethodInfo countMethod = AccessTools.Method(typeof(List<>), "get_Count");
        private static readonly MethodInfo getItemMethod = AccessTools.Method(typeof(List<>), "get_Item", new Type[] { typeof(int) });
        
        /*
         * Reflection stuff.
         */
        // TODO: Try type by name access tools.
        //private static readonly Type IdRecordType = Type.GetType("Verse.LoadIDsWantedBank.IdRecord, Assembly-CSharp");

        //private static readonly Type IdListRecordType =            Type.GetType("Verse.LoadIDsWantedBank.IdListRecord, Assembly-CSharp");

        private static readonly Type IdRecordType = AccessTools.TypeByName("Verse.LoadIDsWantedBank.IdRecord");
        private static readonly Type IdListRecordType = AccessTools.TypeByName("Verse.LoadIDsWantedBank.IdListRecord");
        
        private static readonly FieldInfo targetLoadIDField_1 = AccessTools.Field(IdRecordType, "targetLoadID");
        private static readonly FieldInfo targetTypeField_1 = AccessTools.Field(IdRecordType, "targetType");
        private static readonly FieldInfo pathRelToParentField_1 = AccessTools.Field(IdRecordType, "pathRelToParent");
        private static readonly FieldInfo parentField_1 = AccessTools.Field(IdRecordType, "parent");

        private static readonly FieldInfo targetLoadIDsField_2 = AccessTools.Field(IdListRecordType, "targetLoadIDs");

        private static readonly FieldInfo pathRelToParentField_2 =
            AccessTools.Field(IdListRecordType, "pathRelToParent");

        private static readonly FieldInfo parentField_2 = AccessTools.Field(IdListRecordType, "parent");
        
        // Run on world load before Scribe.loader.FinalizeLoading()
        public static void LoadUpBeforeScribeLoaderClear()
        {   
            Log.Message("Load up");
            var list = (List<IExposable>) crossReferencingExposablesField.GetValue(Scribe.loader.crossRefs);
            list.Do(item => exposables.Add(item));

            foreach (var referencingExposable in exposables)
            {
                var reffable = referencingExposable as ILoadReferenceable;

                if (reffable != null)
                {
                    loadables.Add(reffable.GetUniqueLoadID(), reffable);
                }
            }

            loadLists();
        }

        private static void loadLists()
        {
            Log.Message("Load lists");
            
            var idsReadList = idsReadField.GetValue(Scribe.loader.crossRefs.loadIDs);
            var idListsRead = idListsReadField.GetValue(Scribe.loader.crossRefs.loadIDs);

            countMethod = countMethod.MakeGenericMethod(IdRecordType);
            
            var list1Count = (int) countMethod.Invoke(idsReadList, new object[0]);

            countMethod = countMethod.MakeGenericMethod(IdListRecordType);
            
            var list2Count = (int) countMethod.Invoke(idListsRead, new object[0]);

            Log.Message("List 1 count: " + list1Count);
            Log.Message("List 2 count: " + list2Count);
            
            for (var i = 0; i < list1Count; i++)
            {
                var value = getItemMethod.Invoke(idsReadList, new object[] {i});

                var targetLoadID = (string) targetLoadIDField_1.GetValue(value);
                var targetType = (System.Type) targetTypeField_1.GetValue(value);
                var pathRelToParent = (string) pathRelToParentField_1.GetValue(value);
                var parent = (IExposable) parentField_1.GetValue(value);
                
                var idRecord = new IdRecord(targetLoadID, targetType, pathRelToParent, parent);
                idsRead.Add(idRecord);
            }
            
            Log.Message("Done");
        }

        public static void Resolve()
        {
            Log.Message("Resolve");
            var loadedObjectDirectory = (LoadedObjectDirectory) loadedObjectDirectoryField.GetValue(Scribe.loader.crossRefs);
            var dict = (Dictionary<string, ILoadReferenceable>) allObjectsByLoadIDField.GetValue(loadedObjectDirectory);
            
            loadables.Do(pair => dict.Add(pair));

            Scribe.mode = LoadSaveMode.ResolvingCrossRefs;
            
            foreach (var referencingExposables in exposables)
            {
                try
                {
                    Scribe.loader.curParent = referencingExposables;
                    Scribe.loader.curPathRelToParent = null;
                    referencingExposables.ExposeData();
                }
                catch (Exception ex)
                {
                    Log.Error("Cross ref ex: " + ex);
                }
            }

            Scribe.mode = LoadSaveMode.Inactive;
            Scribe.loader.crossRefs.Clear(true);
        }

        private struct IdRecord
        {
            public string targetLoadID;
            public System.Type targetType;
            public string pathRelToParent;
            public IExposable parent;

            public IdRecord(string targetLoadID, System.Type targetType, string pathRelToParent, IExposable parent)
            {
                this.targetLoadID = targetLoadID;
                this.targetType = targetType;
                this.pathRelToParent = pathRelToParent;
                this.parent = parent;
            }
        }
        
        private struct IdListRecord
        {
            public List<string> targetLoadIDs;
            public string pathRelToParent;
            public IExposable parent;

            public IdListRecord(List<string> targetLoadIDs, string pathRelToParent, IExposable parent)
            {
                this.targetLoadIDs = targetLoadIDs;
                this.pathRelToParent = pathRelToParent;
                this.parent = parent;
            }
        }
    }
}