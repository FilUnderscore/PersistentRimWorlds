using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Root_Entry), "Start")]
    public class Root_Entry_Start_Patch
    {
        static void Prefix(Root_Entry __instance)
        {
            PersistentWorldManager.PersistentWorld = null;
            PersistentWorldManager.WorldLoadSaver = null;
        }
    }
}