using Harmony;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeLoader), "ForceStop")]
    public class ScribeLoader_ForceStop_Patch
    {
        #region Methods
        static bool Prefix(ScribeLoader __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading))
                return true;

            ScribeVars.Set();
            
            ScribeVars.TrickScribe();
            
            return false;
        }
        #endregion
    }
}