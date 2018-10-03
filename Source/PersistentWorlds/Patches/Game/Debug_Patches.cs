using System;
using System.Collections.Generic;
using Harmony;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Patches
{
    public static class Debug_Patches
    {
        [HarmonyPatch(typeof(WorldGrid), "GetTileNeighbor")]
        public static class WorldGrid_GetTileNeighbor_Patch
        {
            [HarmonyPrefix]
            public static void GetTileNeighbor_Prefix(WorldGrid __instance, int tileID, int adjacentId)
            {
                Log.Warning("GetTileNeighbor DEBUG - tileID: " + tileID.ToString() + " ADJID: " + adjacentId.ToString());
            }
        }

        [HarmonyPatch(typeof(WorldGrid), "GetTileNeighbors")]
        public static class WorldGrid_GetTileNeighbors_Patch
        {
            [HarmonyPrefix]
            public static void GetTileNeighbors_Prefix(WorldGrid __instance, int tileID, List<int> outNeighbors)
            {
                Log.Message("TileID: " + tileID.ToString());
                Log.Message("TileIDToNeighbors_offsets: " + __instance.tileIDToNeighbors_offsets.Count.ToString());
                Log.Message("TileIDToNeighbors_values: " + __instance.tileIDToNeighbors_values.Count.ToString());

                Log.Message("PlanetGen Offsets: " + ((List<int>)AccessTools.Field(typeof(PlanetShapeGenerator), "tileIDToNeighbors_offsets").GetValue(null)).Count.ToString());
                Log.Message("PlanetGen Values: " + ((List<int>)AccessTools.Field(typeof(PlanetShapeGenerator), "tileIDToNeighbors_values").GetValue(null)).Count.ToString());
                
                var testNeighbors = new List<int>();
                
                try
                {
                    PackedListOfLists.GetList<int>(__instance.tileIDToNeighbors_offsets, __instance.tileIDToNeighbors_values, tileID, testNeighbors);
                }
                catch (Exception e)
                {
                    Log.Error("Error getting list????");
                    throw;
                }
                
                Log.Message("TestNeighboirs size: " + testNeighbors.Count.ToString());
                Log.Message("");
            }
        }

        [HarmonyPatch(typeof(PlanetShapeGenerator), "Generate")]
        public static class PlanetShapeGenerator_Generate_Patch
        {
            [HarmonyPrefix]
            public static void Generate_Prefix(int subdivisionsCount,  List<Vector3> outVerts,
                List<int> outTileIDToVerts_offsets,  List<int> outTileIDToNeighbors_offsets,
                 List<int> outTileIDToNeighbors_values, float radius, Vector3 viewCenter, float viewAngle)
            {
                Log.Message("Called?");
                Log.Message("A: " + subdivisionsCount.ToString() + " B: " + radius.ToString() + " C: " + viewCenter.ToString() + " D: " + viewAngle.ToString());
            }
        }
    }
}