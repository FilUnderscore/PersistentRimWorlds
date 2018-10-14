using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(UIRoot_Play), "UIRootOnGUI")]
    public class UIRoot_Play_UIRootOnGUI_Patch
    {
        #region Methods
        static bool Prefix(UIRoot_Play __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;
                
            return Current.Game != null && Find.GameInfo != null && Find.World != null && Find.World.UI != null;
        }
        #endregion
    }
}