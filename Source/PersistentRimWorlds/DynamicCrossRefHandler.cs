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
        
        //private static MethodInfo countMethod = AccessTools.Method(typeof(List<>), "get_Count");
        //private static readonly MethodInfo getItemMethod = AccessTools.Method(typeof(List<>), "get_Item", new Type[] { typeof(int) });
        
        /*
         * Reflection stuff.
         */
        // TODO: Try type by name access tools.
        //private static readonly Type IdRecordType = Type.GetType("Verse.LoadIDsWantedBank.IdRecord, Assembly-CSharp");

        //private static readonly Type IdListRecordType =            Type.GetType("Verse.LoadIDsWantedBank.IdListRecord, Assembly-CSharp");

        private static readonly Type IdRecordType = typeof(LoadIDsWantedBank).GetNestedType("IdRecord", BindingFlags.NonPublic);
        private static readonly Type IdListRecordType = typeof(LoadIDsWantedBank).GetNestedType("IdListRecord", BindingFlags.NonPublic);
        
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
            Log.Message("Load up ");
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
            
            var idsReadListL = idsReadField.GetValue(Scribe.loader.crossRefs.loadIDs);
            var idListsReadL = idListsReadField.GetValue(Scribe.loader.crossRefs.loadIDs);

            var countMethod = idsReadListL.GetType().GetMethod("get_Count");
            var list1Count = (int) countMethod.Invoke(idsReadListL, new object[0]);

            countMethod = idListsRead.GetType().GetMethod("get_Count");
            var list2Count = (int) countMethod.Invoke(idListsRead, new object[0]);

            Log.Message("List 1 count: " + list1Count);
            Log.Message("List 2 count: " + list2Count);

            var getItemMethod = idsReadListL.GetType().GetMethod("get_Item", new Type[] {typeof(int)});
            for (var i = 0; i < list1Count; i++)
            {
                var value = getItemMethod.Invoke(idsReadListL, new object[] {i});

                var targetLoadID = (string) targetLoadIDField_1.GetValue(value);
                var targetType = (System.Type) targetTypeField_1.GetValue(value);
                var pathRelToParent = (string) pathRelToParentField_1.GetValue(value);
                var parent = (IExposable) parentField_1.GetValue(value);
                
                var idRecord = new IdRecord(targetLoadID, targetType, pathRelToParent, parent);
                idsRead.Add(idRecord);
            }

            getItemMethod = idListsRead.GetType().GetMethod("get_Item", new Type[] {typeof(int)});
            for (var i = 0; i < list2Count; i++)
            {
                var value = getItemMethod.Invoke(idListsRead, new object[] {i});

                var targetLoadIDs = (List<string>) targetLoadIDsField_2.GetValue(value);
                var pathRelToParent = (string) pathRelToParentField_2.GetValue(value);
                var parent = (IExposable) parentField_2.GetValue(value);
                
                var idListRecord = new IdListRecord(targetLoadIDs, pathRelToParent, parent);
                idListsRead.Add(idListRecord);
            }
            
            Log.Message("Done");
        }

        private static void resolveLists()
        {
            Log.Message("Resolve lists.");
            var idsReadListL = idsReadField.GetValue(Scribe.loader.crossRefs.loadIDs);
            var idListsReadL = idListsReadField.GetValue(Scribe.loader.crossRefs.loadIDs);

            var addItemMethod = idsReadListL.GetType().GetMethod("Add", new Type[] { IdRecordType });
            var newRecordCtor = AccessTools.Constructor(IdRecordType, new Type[] { typeof(string), typeof(Type), typeof(string), typeof(IExposable) });

            foreach (var record in idsRead)
            {
                var newRecord = newRecordCtor.Invoke(new object[] { record.targetLoadID, record.targetType, record.pathRelToParent, record.parent });
                addItemMethod.Invoke(idsReadListL, new object[] { newRecord });
            }

            addItemMethod = idListsReadL.GetType().GetMethod("Add", new Type[] { IdListRecordType });
            newRecordCtor = AccessTools.Constructor(IdListRecordType,
                new Type[] {typeof(List<string>), typeof(string), typeof(IExposable)});
            
            foreach (var record in idListsRead)
            {
                var newRecord = newRecordCtor.Invoke(new object[]
                    {record.targetLoadIDs, record.pathRelToParent, record.parent});
                addItemMethod.Invoke(idListsReadL, new object[] {newRecord});
            }
            
            Log.Message("Done");
        }

        public static void Resolve()
        {
            Log.Message("Resolve");
            var loadedObjectDirectory = (LoadedObjectDirectory) loadedObjectDirectoryField.GetValue(Scribe.loader.crossRefs);
            var dict = (Dictionary<string, ILoadReferenceable>) allObjectsByLoadIDField.GetValue(loadedObjectDirectory);
            
            loadables.Do(pair => dict.Add(pair));

            resolveLists();
            
            Scribe.mode = LoadSaveMode.ResolvingCrossRefs;
            
            foreach (var referencingExposables in exposables)
            {
                if (referencingExposables == null)
                {
                    continue;
                }
                
                
                Scribe.loader.curParent = referencingExposables;
                Scribe.loader.curPathRelToParent = null;
                referencingExposables.ExposeData();
            }

            Scribe.mode = LoadSaveMode.Inactive;
            Scribe.loader.crossRefs.Clear(false);
            
            Log.Message("Done2");
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