using Harmony;
using PersistentWorlds.SaveAndLoad;
using RimWorld.Planet;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(WorldGenStep_Components), "GenerateFromScribe")]
    public class WorldGenStep_Components_Patch
    {
        #region Methods
        static bool Prefix()
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull() || !PersistentWorldManager.GetInstance().PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting))
            {
                return true;
            }
            
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;

            persistentWorld.ConstructGameWorldComponentsAndExposeComponents();
            
            return false;
        }
        #endregion
    }
}