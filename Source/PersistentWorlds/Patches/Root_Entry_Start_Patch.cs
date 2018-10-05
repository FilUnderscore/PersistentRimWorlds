using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Root_Entry), "Start")]
    public static class Root_Entry_Start_Patch
    {
        [HarmonyPrefix]
        public static void Start_Prefix(Root_Entry __instance)
        {
            PersistentWorldManager.PersistentWorld = null;
            PersistentWorldManager.WorldLoadSaver = null;
        }
    }
}