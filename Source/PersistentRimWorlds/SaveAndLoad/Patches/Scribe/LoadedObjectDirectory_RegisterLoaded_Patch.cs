using System.Collections.Generic;
using System.Reflection;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    // Silence some errors..
    [HarmonyPatch(typeof(LoadedObjectDirectory), "RegisterLoaded")]
    public class LoadedObjectDirectory_RegisterLoaded_Patch
    {
        #region Fields
        private static readonly FieldInfo AllObjectsByLoadIdField =
            AccessTools.Field(typeof(LoadedObjectDirectory), "allObjectsByLoadID");
        #endregion
        
        #region Methods
        static bool Prefix(LoadedObjectDirectory __instance, ILoadReferenceable reffable)
        {
            var allObjectsByLoadID = (Dictionary<string, ILoadReferenceable>) AllObjectsByLoadIdField.GetValue(__instance);

            return !allObjectsByLoadID.ContainsKey(reffable.GetUniqueLoadID());
        }
        #endregion
    }
}