using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(LoadedObjectDirectory), "Clear")]
    public class LoadedObjectDirectory_Clear_Patch
    {
        static bool Prefix()
        {
            return PersistentWorldManager.ReferenceTable == null || PersistentWorldManager.WorldLoadSaver == null ||
                   PersistentWorldManager.WorldLoadSaver.Status ==
                   PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting;
        }
    }
}