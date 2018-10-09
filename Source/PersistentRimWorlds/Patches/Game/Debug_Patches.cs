using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;
using JetBrains.Annotations;
using PersistentWorlds.Logic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Video;
using Verse;

namespace PersistentWorlds.Patches
{
    #if DEBUG
    public static class Debug_Patches
    {
        [HarmonyPatch(typeof(Map), "ExposeData")]
        public static class Map_ExposeData
        {
            [HarmonyPrefix]
            public static bool Prefix(Map __instance)
            {
                Log.Message("Scribe mode: " + Scribe.mode.ToString());

              if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && __instance.compressor == null)
              {
                return false;
              }
              
              Log.Message("M1");
                Scribe_Values.Look<int>(ref __instance.uniqueID, "uniqueID", -1, false);
              Log.Message("M2");
                Scribe_Deep.Look<MapInfo>(ref __instance.info, "mapInfo");
              Log.Message("M5");

                switch (Scribe.mode)
                {
                  case LoadSaveMode.Saving:
                    __instance.compressor = new MapFileCompressor(__instance);
                    __instance.compressor.BuildCompressedString();
                    AccessTools.Method(typeof(Map), "ExposeComponents").Invoke(__instance, new object[0]);
                    __instance.compressor.ExposeData();
                    HashSet<string> stringSet = new HashSet<string>();
                    if (Scribe.EnterNode("things"))
                    {
                      try
                      {
                        foreach (Thing allThing in __instance.listerThings.AllThings)
                        {
                          try
                          {
                            if (allThing.def.isSaveable)
                            {
                              if (!allThing.IsSaveCompressible())
                              {
                                if (stringSet.Contains(allThing.ThingID))
                                  Log.Error("Saving Thing with already-used ID " + allThing.ThingID, false);
                                else
                                  stringSet.Add(allThing.ThingID);
                                Thing target = allThing;
                                Scribe_Deep.Look<Thing>(ref target, "thing");
                              }
                            }
                          }
                          catch (Exception ex)
                          {
                            Log.Error("Exception saving " + (object) allThing + ": " + (object) ex, false);
                          }
                        }
                      }
                      finally
                      {
                        Scribe.ExitNode();
                      }
                    }
                    else
                      Log.Error("Could not enter the things node while saving.", false);
                    __instance.compressor = (MapFileCompressor) null;
                    return false;
                  case LoadSaveMode.LoadingVars:
                    __instance.ConstructComponents();
                    __instance.regionAndRoomUpdater.Enabled = false;
                    __instance.compressor = new MapFileCompressor(__instance);
                    break;
                }
              Log.Message("M3");

                        AccessTools.Method(typeof(Map), "ExposeComponents").Invoke(__instance, new object[0]);
              Log.Message("M6");
                DeepProfiler.Start("Load compressed things");
              Log.Message("M7");

              if (__instance.compressor == null)
              {
                Log.Error("Null compressor");
                __instance.compressor = new MapFileCompressor(__instance);
              }
              else
              {
                Log.Message("Not a problem");
              }
              
              __instance.compressor.ExposeData();
              Log.Message("M8");  
              DeepProfiler.End();
              Log.Message("M9");
                DeepProfiler.Start("Load non-compressed things");
                        var loadedFullThings = (List<Thing>) AccessTools.Field(typeof(Map), "loadedFullThings").GetValue(__instance);
                Scribe_Collections.Look<Thing>(ref loadedFullThings, "things", LookMode.Deep, new object[0]);
              AccessTools.Field(typeof(Map), "loadedFullThings").SetValue(__instance, loadedFullThings);
                DeepProfiler.End();
          
                return false;
            }
        }
    }
    #endif
}