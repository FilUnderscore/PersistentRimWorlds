using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(DebugLoadIDsSavingErrorsChecker), "CheckForErrorsAndClear")]
    public class DebugLoadIDsSavingErrorsChecker_CheckForErrorsAndClear_Patch
    {
        // Fix warnings, because other way is making a multi saver for ScribeSaver and that's too much.
        static bool Prefix(DebugLoadIDsSavingErrorsChecker __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull()) return true;

            __instance.Clear();
            return false;
        }
    }
}