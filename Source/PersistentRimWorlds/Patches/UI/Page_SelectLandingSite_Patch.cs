using Harmony;
using PersistentWorlds.UI;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Page_SelectLandingSite), "PreOpen")]
    public static class Page_SelectLandingSite_Patch
    {
        [HarmonyPrefix]
        public static void SelectLandingSite_Prefix(Page_SelectLandingSite __instance)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver.Status != PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading)
                return;

            PersistentWorldManager.PersistentWorld.Game.Scenario = Current.Game.Scenario;
            PersistentWorldManager.PersistentWorld.Game.storyteller = Current.Game.storyteller;
            Current.Game = PersistentWorldManager.PersistentWorld.Game;
                
            // TODO: Review
            Current.Game.InitData = new GameInitData();
                
            PersistentWorldManager.WorldLoadSaver.LoadMaps();
                
            PersistentWorldManager.PersistentWorld.Game.World.pathGrid = new WorldPathGrid();
            //PersistentWorldManager.PersistentWorld.Game.World.grid.StandardizeTileData();
            //PersistentWorldManager.PersistentWorld.Game.World.FinalizeInit();
            Current.Game.Scenario.PostWorldGenerate();

            PersistentWorldManager.WorldLoadSaver.Status =
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Creating;

            PersistentWorldManager.PersistentWorld.AddColoniesToWorldMap();
            
            // TODO: Review
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_Main));
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_FileList));
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_ColonySelection));
        }
    }
}