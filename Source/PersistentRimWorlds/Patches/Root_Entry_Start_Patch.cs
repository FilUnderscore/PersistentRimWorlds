using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Root_Entry), "Start")]
    public class Root_Entry_Start_Patch
    {
        #region Methods
        static void Prefix(Root_Entry __instance)
        {
            PersistentWorldManager.GetInstance().Clear();
        }
        #endregion
    }
}