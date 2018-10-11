using Harmony;
using PersistentWorlds.SaveAndLoad;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeLoader), "ForceStop")]
    public class ScribeLoader_ForceStop_Patch
    {
        static bool Prefix(ScribeLoader __instance)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status !=
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading)
                return true;

            ScribeVars.mode = Scribe.mode;
            ScribeVars.curParent = Scribe.loader.curParent;
            ScribeVars.curXmlParent = Scribe.loader.curXmlParent;
            ScribeVars.curPathRelToParent = Scribe.loader.curPathRelToParent;
            
            Scribe.mode = LoadSaveMode.Inactive;
            Scribe.loader.curParent = null;
            Scribe.loader.curXmlParent = null;
            Scribe.loader.curPathRelToParent = null;
            
            return false;
        }
    }
}