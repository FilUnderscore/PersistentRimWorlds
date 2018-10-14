using System.Reflection;
using System.Runtime.InteropServices;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "ExitNode")]
    public class ScribeSaver_ExitNode_Patch
    {
        #region Fields
        private static readonly FieldInfo writerField = AccessTools.Field(typeof(ScribeSaver), "writer");
        
        private static readonly FieldInfo curPathField = AccessTools.Field(typeof(ScribeSaver), "curPath");
        #endregion
        
        #region Methods
        static bool Prefix(ScribeSaver __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;
            
            var writer = writerField.GetValue(__instance);

            if (writer == null) return true;

            var curPath = (string) curPathField.GetValue(__instance);

            if (curPath.EndsWith("thing"))
            {
                // Reset thing index.
                ScribeSaver_EnterNode_Patch.ResetThingIndex();
            }
            
            var length = curPath.LastIndexOf('/');
            curPath = length <= 0 ? null : curPath.Substring(0, length);
            
            curPathField.SetValue(__instance, curPath);
            
            return true;
        }
        #endregion
    }
}