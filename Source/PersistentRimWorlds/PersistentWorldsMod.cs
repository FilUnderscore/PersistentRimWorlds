// Persistent RimWorlds - Persistent Worlds RimWorld Mod
// Copyright (C) 2018-2021 Filip Jerkovic [under GPLv3 license]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

using PersistentWorlds.UI;
using FileLog = PersistentWorlds.Utils.FileLog;
using RimWorld.QuestGen;
using RimWorld;

namespace PersistentWorlds
{
    /// <summary>
    /// The main mod class that interacts with RimWorld when it is loaded.
    /// </summary>
    public sealed class PersistentWorldsMod : Mod
    {
        #region Constructors
        /// <summary>
        /// Main mod constructor that initializes Harmony patches.
        /// </summary>
        /// <param name="content"></param>
        public PersistentWorldsMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("me.filunderscore.persistentrimworlds");

#if DEBUG
            Harmony.DEBUG = true;
            
            FileLog.StartLoggingToFile("debug_log.txt");
#endif

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        #endregion

        #region Methods
        public static void PatchSaveMenu()
        {
            Find.WindowStack.Add(new Dialog_PersistentWorlds_SaveWorld());
        }
        #endregion
    }
}