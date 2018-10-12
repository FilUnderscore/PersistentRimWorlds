using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    /// <summary>
    /// Required patch for ReferenceTable to work.
    /// </summary>
    [HarmonyPatch(typeof(DebugLoadIDsSavingErrorsChecker), "RegisterDeepSaved")]
    public class DebugLoadIDsSavingErrorsChecker_RegisterDeepSaved_Patch
    {
        static bool Prefix(DebugLoadIDsSavingErrorsChecker __instance, object obj, string label)
        {
            if (Scribe.mode != LoadSaveMode.Saving) return true;
            
            if (obj != null && obj is ILoadReferenceable referenceable)
            {
                PersistentWorldManager.ReferenceTable.AddReference(referenceable);
            }

            return true;
        }
    }
}