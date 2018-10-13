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
        #region Methods
        static bool Prefix(DebugLoadIDsSavingErrorsChecker __instance, object obj, string label)
        {
            if (Scribe.mode != LoadSaveMode.Saving) return true;
            
            // TODO: Please look at this spam.
            if (obj is Thing && !(obj is Pawn))
            {
                return true;
            }

            if (obj == null || !(obj is ILoadReferenceable referenceable)) return true;

            if (!PersistentWorldManager.ReferenceTable.ContainsReferenceWithLoadId(referenceable.GetUniqueLoadID()))
            {
                PersistentWorldManager.ReferenceTable.LoadReferenceIntoMemory(referenceable, label);
            }

            return true;
        }
        #endregion
    }
}