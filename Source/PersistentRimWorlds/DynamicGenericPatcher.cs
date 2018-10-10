using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Harmony.ILCopying;
using RimWorld;
using Verse;

namespace PersistentWorlds
{
    public static class DynamicGenericPatcher
    {
        public static void PatchScribeCollectionsLook(HarmonyInstance harmony)
        {
            LongEventHandler.SetCurrentEventText("Patching methods");
            
            var method = typeof(Scribe_Collections).GetMethods().First(m =>
                m.Name == "Look" && m.IsGenericMethod && m.GetParameters()[1].ParameterType == typeof(bool));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(asm => !(asm.ManifestModule is ModuleBuilder)))
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (type == typeof(DynamicGenericPatcher)) continue;
                    
                    if (type.Namespace.ToLower().Contains("rimworld") || type.Namespace.ToLower().Contains("verse") ||
                        type.Namespace.ToLower().Contains("persistentworlds"))
                    {   
                        var genericMethod = method.MakeGenericMethod(type);
                        var patchedGenericMethod = typeof(DynamicGenericPatcher).GetMethod("Look_Patch")
                            .MakeGenericMethod(type);

                        Log.Message("Patching: " + type.Name, true);
                        
                        harmony.Patch(genericMethod, new HarmonyMethod(patchedGenericMethod));
                    }
                }
            }
            
            Log.ResetMessageCount();
        }

        public static bool Look_Patch<T>(ref List<T> list, bool saveDestroyedThings, string label, LookMode lookMode,
            params object[] ctorArgs)
        {
            if (lookMode != LookMode.Reference)
            {
                return true;
            }
            
            Log.Message("Reference");

            return false;
        }
    }
}