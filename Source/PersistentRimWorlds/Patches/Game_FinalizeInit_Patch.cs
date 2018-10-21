using Harmony;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public class Game_FinalizeInit_Patch
    {
        #region Methods
        static void Postfix(Game __instance)
        {
            if (PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting))
            {
                PersistentWorldManager.GetInstance().PersistentWorld.LoadSaver.Convert(__instance);
            }
            else
            {
                // Toggle colonies tab.
                DefDatabase<MainButtonDef>.GetNamed("Colonies").buttonVisible =
                    PersistentWorldManager.GetInstance().PersistentWorldNotNull();
            }
        }
        #endregion
    }
}