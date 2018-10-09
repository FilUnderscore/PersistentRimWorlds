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
        public static Dictionary<string, ILoadReferenceable> loadables = new Dictionary<string, ILoadReferenceable>();
        private static List<IExposable> exposables = new List<IExposable>();

        /*
         * Fields
         */

        private static readonly FieldInfo crossReferencingExposablesField =
            AccessTools.Field(typeof(CrossRefHandler), "crossReferencingExposables");
        
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
        }

        public static void FixMap(ref Map map)
        {
            //map.info.parent = loadables
        }
    }
}