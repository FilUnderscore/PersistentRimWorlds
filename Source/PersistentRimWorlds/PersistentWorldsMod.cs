using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using Verse;

using PersistentWorlds.UI;

namespace PersistentWorlds
{
    /// <summary>
    /// The main mod class that interacts with RimWorld when it is loaded.
    /// </summary>
    public sealed class PersistentWorldsMod : Mod
    {
        // Must be public or IL transpiler throws error of ldsfld NULL.
        public static Delegate MainMenuButtonDelegate = new Action(PatchMainMenu);
        
        /// <summary>
        /// Main mod constructor that initializes Harmony patches.
        /// </summary>
        /// <param name="content"></param>
        public PersistentWorldsMod(ModContentPack content) : base(content)
        {
            var harmony = HarmonyInstance.Create("me.filunderscore.persistentrimworlds");

#if DEBUG
            HarmonyInstance.DEBUG = true;
#endif

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Patches the button of "Persistent RimWorlds", more of a delegate called by "MainMenuButtonDelegate".
        /// </summary>
        private static void PatchMainMenu()
        {
            Find.WindowStack.Add(new Dialog_PersistentWorlds_Main());
        }
    }
}