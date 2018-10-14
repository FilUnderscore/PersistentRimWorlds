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
        #region Methods
        static void Prefix(Page_SelectStartingSite __instance)
        {
            Log.Message("Call 1");
            
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Loading))
                return;

            Log.Message("Call 2");
            
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

            if (persistentWorld == null)
            {
                Log.Error("NULL WORLD");
            }
            else
            {
                Log.Message("NOT NULL WORLD");
            }
            
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_Main));
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_FileList));
            Find.WindowStack.TryRemove(typeof(Dialog_PersistentWorlds_LoadWorld_ColonySelection));
        }
        #endregion
    }
}