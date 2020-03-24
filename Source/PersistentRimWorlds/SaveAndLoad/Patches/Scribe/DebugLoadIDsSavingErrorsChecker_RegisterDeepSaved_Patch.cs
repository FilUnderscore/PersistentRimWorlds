using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
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
        
        #region Constructors
        static DebugLoadIDsSavingErrorsChecker_RegisterDeepSaved_Patch()
        {
            if(deepSavedField == null)
                throw new NullReferenceException($"{nameof(deepSavedField)} is null.");
        }
        #endregion
        
        #region Methods
        static bool Prefix(object obj, string label)
        {
            if (Scribe.mode != LoadSaveMode.Saving || !PersistentWorldManager.GetInstance().PersistentWorldNotNull()) return true;
            
            // TODO: Please look at this spam.
            if (obj is Thing && !(obj is Pawn))
            {
                return true;
            }

            if (obj == null || !(obj is ILoadReferenceable referenceable)) return true;

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            var loadSaver = persistentWorld.LoadSaver;
            
            if (!loadSaver.ReferenceTable.ContainsReferenceWithLoadId(referenceable.GetUniqueLoadID()))
            {
                loadSaver.ReferenceTable.LoadReferenceIntoMemory(referenceable, label);
            }

            // Fix those warnings.
            var deepSaved = (HashSet<string>) deepSavedField.GetValue(Scribe.saver.loadIDsErrorsChecker);

            return !deepSaved.Contains(referenceable.GetUniqueLoadID());
        }
        #endregion
    }
}