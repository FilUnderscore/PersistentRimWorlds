using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
    public class PawnNameColorUtility_PawnNameColorOf_Patch
    {
        static bool Prefix(Pawn pawn)
        {
            // TODO: Check if persistent world and if pawn is from different colony.
            
            return true;
        }
    }
}