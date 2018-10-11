using Harmony;
using RimWorld.Planet;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(WorldGenStep_Components), "GenerateFromScribe")]
    public class WorldGenStep_Components_Patch
    {
        static bool Prefix()
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