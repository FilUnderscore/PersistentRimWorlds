using System.Runtime.InteropServices;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(CrossRefHandler), "Clear")]
    public class CrossRefHandler_Clear_Patch
    {
        static bool Prefix(CrossRefHandler __instance, bool errorIfNotEmpty)
        {
            
            
            return false;
        }
    }
}