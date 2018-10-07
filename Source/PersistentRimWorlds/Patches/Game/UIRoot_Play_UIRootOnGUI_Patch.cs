using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(UIRoot_Play), "UIRootOnGUI")]
    public static class UIRoot_Play_UIRootOnGUI_Patch
    {
        [HarmonyPrefix]
        public static bool UIRootOnGUI_Prefix(UIRoot_Play __instance)
        {
            return Current.Game != null && Find.GameInfo != null && Find.World != null && Find.World.UI != null;
        }
    }
}