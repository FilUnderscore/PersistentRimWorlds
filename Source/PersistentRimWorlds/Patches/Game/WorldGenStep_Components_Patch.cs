using Harmony;
using RimWorld.Planet;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(WorldGenStep_Components), "GenerateFromScribe")]
    public static class WorldGenStep_Components_Patch
    {
        [HarmonyPrefix]
        public static bool GenerateFromScribe_Prefix()
        {
            if (PersistentWorldManager.PersistentWorld == null)
            {
                return true;
            }
            
            var persistentWorld = PersistentWorldManager.PersistentWorld;

            persistentWorld.ConstructGameWorldComponentsAndExposeComponents();
            
            return false;
        }
    }
}