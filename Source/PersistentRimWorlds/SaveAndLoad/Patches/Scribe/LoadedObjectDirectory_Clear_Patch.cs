using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(LoadedObjectDirectory), "Clear")]
    public class LoadedObjectDirectory_Clear_Patch
    {
        static bool Prefix()
        {
            return PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting);
        }
    }
}