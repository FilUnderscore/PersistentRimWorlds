using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(PawnDestinationReservationManager), "ExposeData")]
    public class PawnDestinationReservationManager_ExposeData_Patch
    {
        static bool Prefix()
        {
            // TODO: Patch.
            return true;
        }
    }
}