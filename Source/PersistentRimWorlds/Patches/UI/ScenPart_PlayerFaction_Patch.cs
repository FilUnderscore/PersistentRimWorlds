using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(ScenPart_PlayerFaction), "PostWorldGenerate")]
    public class ScenPart_PlayerFaction_Patch
    {
        #region Methods
        static bool Prefix(ScenPart_PlayerFaction __instance)
        {
            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver == null)
                return true;

            // In order to prevent cloned factions :/
            Find.GameInitData.playerFaction = PersistentWorldManager.PersistentWorld.WorldData.factionManager.OfPlayer;
            
            return false;
        }
        #endregion
    }
}