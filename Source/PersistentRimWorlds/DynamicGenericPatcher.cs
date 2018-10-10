using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Harmony.ILCopying;
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
                    if (type.Namespace.ToLower().Contains("rimworld") || type.Namespace.ToLower().Contains("verse") ||
                        type.Namespace.ToLower().Contains("persistentworlds"))
                    {

                        var genericMethod = method.MakeGenericMethod(type);
                        var patchedGenericMethod = CreateDynamicMethod(genericMethod, type);

                        harmony.Patch(genericMethod, new HarmonyMethod(patchedGenericMethod));
                    }
                }
            }
            
            Log.ResetMessageCount();
        }

        private static DynamicMethod CreateDynamicMethod(MethodInfo method, Type type)
        {
            Log.Message("Patching DynamicMethod (Scribe_Collections.Look<>) for type: " + type.Name, true);
            
            var parameters = new List<Type>();

            parameters.Add(typeof(List<>).MakeGenericType(type).MakeByRefType());
            
            for (var i = 1; i < method.GetParameters().Length; i++)
            {
                var param = method.GetParameters()[i];
                parameters.Add(param.GetType());
            }
            
            var dynamicMethod = new DynamicMethod(method.Name, typeof(bool), parameters.ToArray());
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldc_I4_1);
            ilGenerator.Emit(OpCodes.Ret);
            
            return dynamicMethod;
        }
    }
}