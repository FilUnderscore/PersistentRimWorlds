using System;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "ExitNode")]
    public class ScribeSaver_ExitNode_Patch
    {
        #region Fields
        private static readonly FieldInfo WriterField = AccessTools.Field(typeof(ScribeSaver), "writer");
        
        private static readonly FieldInfo CurPathField = AccessTools.Field(typeof(ScribeSaver), "curPath");
        #endregion
        
        #region Constructors
        static ScribeSaver_ExitNode_Patch()
        {
            if(WriterField == null)
                throw new NullReferenceException($"{nameof(WriterField)} is null.");
            
            if(CurPathField == null)
                throw new NullReferenceException($"{nameof(CurPathField)} is null.");
        }
        #endregion
        
        #region Methods
        static bool Prefix(ScribeSaver __instance)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;
            
            var writer = WriterField.GetValue(__instance);

            if (writer == null) return true;

            var curPath = (string) CurPathField.GetValue(__instance);

            if (curPath.EndsWith("thing"))
            {
                // Reset thing index.
                ScribeSaver_EnterNode_Patch.ResetThingIndex();
            }
            
            var length = curPath.LastIndexOf('/');
            curPath = length <= 0 ? null : curPath.Substring(0, length);
            
            CurPathField.SetValue(__instance, curPath);
            
            return true;
        }
        #endregion
    }
}