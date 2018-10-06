using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    /*
    [HarmonyPatch(typeof(ScenPart_PlayerFaction), "PostWorldGenerate")]
    public static class ScenPart_PlayerFaction_Patch
    {
        [HarmonyPrefix]
        public static bool PostWorldGenerate_Prefix(ScenPart_PlayerFaction __instance)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null)
                return true;

            // TODO: Review.
            Find.GameInitData.playerFaction = PersistentWorldManager.PersistentWorld.WorldData.factionManager.OfPlayer;
            
            return false;
        }
    }
    */
}