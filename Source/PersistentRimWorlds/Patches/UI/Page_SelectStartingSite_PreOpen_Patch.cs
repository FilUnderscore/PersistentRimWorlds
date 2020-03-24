using HarmonyLib;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.UI;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Page_SelectStartingSite), "PreOpen")]
    public class Page_SelectLandingSite_PreOpen_Patch
    {
        #region Methods
        static void Prefix()
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading))
                return;
   
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            persistentWorld.Game.Scenario = Current.Game.Scenario;
            persistentWorld.Game.storyteller = Current.Game.storyteller;
            Current.Game = persistentWorld.Game;
            
            Current.Game.InitData = new GameInitData();
            
            Scribe.loader.FinalizeLoading();
            
            persistentWorld.Game.World.pathGrid = new WorldPathGrid();
            Current.Game.Scenario.PostWorldGenerate();

            persistentWorld.LoadSaver.Status =
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Creating;
        }
        #endregion
    }
}