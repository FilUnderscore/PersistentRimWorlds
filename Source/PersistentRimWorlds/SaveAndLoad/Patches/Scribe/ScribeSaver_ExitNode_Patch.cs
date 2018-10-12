using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "ExitNode")]
    public class ScribeSaver_ExitNode_Patch
    {
        #region Methods
        static bool Prefix(ScribeSaver __instance)
        {
            return true;
        }
        #endregion
    }
}