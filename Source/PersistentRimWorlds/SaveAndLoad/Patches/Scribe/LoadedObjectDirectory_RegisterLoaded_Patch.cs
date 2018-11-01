using System;
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
        
        #region Constructors
        static LoadedObjectDirectory_RegisterLoaded_Patch()
        {
            if(AllObjectsByLoadIdField == null)
                throw new NullReferenceException($"{nameof(AllObjectsByLoadIdField)} is null.");
        }
        #endregion
        
        #region Methods
        static bool Prefix(LoadedObjectDirectory __instance, ILoadReferenceable reffable)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
            {
                return true;
            }
            
            var allObjectsByLoadID = (Dictionary<string, ILoadReferenceable>) AllObjectsByLoadIdField.GetValue(__instance);

            return !allObjectsByLoadID.ContainsKey(reffable.GetUniqueLoadID());
        }
        #endregion
    }
}