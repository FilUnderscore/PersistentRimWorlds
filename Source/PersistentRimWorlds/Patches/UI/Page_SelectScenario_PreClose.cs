using Harmony;
using RimWorld;

namespace PersistentWorlds.Patches.UI
{
    [HarmonyPatch(typeof(Page_SelectScenario), "PreClose")]
    public class Page_SelectScenario_PreClose
    {
        static void Postfix()
        {
            
        }
    }
}