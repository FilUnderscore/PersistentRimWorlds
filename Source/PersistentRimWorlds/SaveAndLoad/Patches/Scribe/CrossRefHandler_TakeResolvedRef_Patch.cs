using System;
using System.Reflection;
using Harmony;
using UnityEngine.AI;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch]
    public class CrossRefHandler_TakeResolvedRef_Patch
    {
        /*
        static bool Prefix(CrossRefHandler __instance, string toAppendToPathRelToParent, ref ILoadReferenceable __result)
        {
            if (PersistentWorldManager.ReferenceTable == null) return true;

            try
            {
                __result = PersistentWorldManager.ReferenceTable.ResolveReference(toAppendToPathRelToParent);
            }
            catch (Exception e)
            {
                return true;
            }

            if (__result == null)
            {
                Debug.FileLog.Log("Returning because result null.");
                return true;
            }
            
            return false;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CrossRefHandler), "TakeResolvedRef", new Type[] {typeof(string)})
                .MakeGenericMethod(typeof(ILoadReferenceable));
        }
        */
    }
}