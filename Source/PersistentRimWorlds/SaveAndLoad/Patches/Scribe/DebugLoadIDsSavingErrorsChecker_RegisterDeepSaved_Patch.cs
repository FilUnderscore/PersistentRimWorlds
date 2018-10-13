using System.Collections.Generic;
using System.Reflection;
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
        #region Fields
        private static readonly FieldInfo deepSavedField =
            AccessTools.Field(typeof(DebugLoadIDsSavingErrorsChecker), "deepSaved");
        #endregion
        
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

            // Fix those warnings.
            var deepSaved = (HashSet<string>) deepSavedField.GetValue(Scribe.saver.loadIDsErrorsChecker);

            return !deepSaved.Contains(referenceable.GetUniqueLoadID());
        }
        #endregion
    }
}