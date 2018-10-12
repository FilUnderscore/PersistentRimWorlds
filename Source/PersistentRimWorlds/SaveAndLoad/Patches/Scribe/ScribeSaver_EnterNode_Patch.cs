using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "EnterNode")]
    public class ScribeSaver_EnterNode_Patch
    {
        #region Methods
        static bool Prefix(ScribeSaver __instance, string nodeName)
        {
            return true;
        }
        #endregion
    }
}