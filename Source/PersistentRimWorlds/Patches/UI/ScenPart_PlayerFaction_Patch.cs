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
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;

            // In order to prevent cloned factions :/
            Find.GameInitData.playerFaction = PersistentWorldManager.GetInstance().PersistentWorld.WorldData.FactionManager.OfPlayer;
            
            return false;
        }
        #endregion
    }
}