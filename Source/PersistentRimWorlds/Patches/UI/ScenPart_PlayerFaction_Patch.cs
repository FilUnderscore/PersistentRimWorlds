using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(ScenPart_PlayerFaction), "PostWorldGenerate")]
    public class ScenPart_PlayerFaction_Patch
    {
        #region Fields
        private static readonly FieldInfo FactionDefField =
            AccessTools.Field(typeof(ScenPart_PlayerFaction), "factionDef");
        #endregion
        
        #region Constructors
        static ScenPart_PlayerFaction_Patch()
        {
            if(FactionDefField == null)
                throw new NullReferenceException($"{nameof(FactionDefField)} is null.");
        }
        #endregion
        
        #region Methods
        static bool Prefix(ScenPart_PlayerFaction __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;

            // In order to prevent cloned factions :/
            PersistentWorldManager.GetInstance().PersistentWorld
                .SetPlayerFactionVarsToNewGeneratedFaction((FactionDef) FactionDefField.GetValue(__instance));
            
            Find.GameInitData.playerFaction = PersistentWorldManager.GetInstance().PersistentWorld.WorldData.FactionManager.OfPlayer;
            FactionGenerator.EnsureRequiredEnemies(Find.GameInitData.playerFaction);
            
            return false;
        }
        #endregion
    }
}