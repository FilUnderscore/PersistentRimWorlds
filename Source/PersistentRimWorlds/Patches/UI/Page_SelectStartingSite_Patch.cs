using Harmony;
using PersistentWorlds.UI;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Page_SelectStartingSite), "PreOpen")]
    public class Page_SelectLandingSite_Patch
    {
        static void Prefix(Page_SelectStartingSite __instance)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.WorldLoadSaver.Status != PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading)
                return;

            PersistentWorldManager.PersistentWorld.Game.Scenario = Current.Game.Scenario;
            PersistentWorldManager.PersistentWorld.Game.storyteller = Current.Game.storyteller;
            Current.Game = PersistentWorldManager.PersistentWorld.Game;
                
            Current.Game.InitData = new GameInitData();
            
            Scribe.loader.FinalizeLoading();
                
            // TODO: Review
            //PersistentWorldManager.WorldLoadSaver.LoadMaps();  
            
            PersistentWorldManager.PersistentWorld.Game.World.pathGrid = new WorldPathGrid();
            Current.Game.Scenario.PostWorldGenerate();

            PersistentWorldManager.WorldLoadSaver.Status =
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Creating;

            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_Main));
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_FileList));
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_ColonySelection));
        }
    }
}