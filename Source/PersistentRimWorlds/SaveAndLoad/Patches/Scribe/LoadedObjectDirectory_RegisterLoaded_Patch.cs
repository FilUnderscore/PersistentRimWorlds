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
        private static readonly FieldInfo allObjectsByLoadIDField =
            AccessTools.Field(typeof(LoadedObjectDirectory), "allObjectsByLoadID");
        
        static bool Prefix(LoadedObjectDirectory __instance, ILoadReferenceable reffable)
        {
            var allObjectsByLoadID = (Dictionary<string, ILoadReferenceable>) allObjectsByLoadIDField.GetValue(__instance);

            return !allObjectsByLoadID.ContainsKey(reffable.GetUniqueLoadID());
        }
    }
}