using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(LoadedObjectDirectory), "Clear")]
    public class LoadedObjectDirectory_Clear_Patch
    {
        static bool Prefix()
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;
            
            return PersistentWorldManager.GetInstance().PersistentWorldNotNull() && PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting);
        }
    }
}