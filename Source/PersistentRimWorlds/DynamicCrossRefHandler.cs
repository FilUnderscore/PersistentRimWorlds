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
        private static LoadedObjectDirectory LoadedObjectDirectory;
        
        // Run on world load before Scribe.loader.FinalizeLoading()
        public static void LoadUpBeforeScribeLoaderClear()
        {
            
        }
    }

    public static class ReferencePatches
    {
        [HarmonyPatch(typeof(Scribe_References), "Look")]
        public static class Scribe_References_Look_Patch
        {
            [HarmonyPrefix]
            public static void Look(ILoadReferenceable ___refee, string label)
            {
                
            }
        }
    }
}