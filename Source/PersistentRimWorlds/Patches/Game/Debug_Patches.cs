using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
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
        [HarmonyPatch(typeof(ParentRelationUtility), "GetFather")]
        public static class Patch_1
        {
            [HarmonyPrefix]
            public static void Prefix_Test(Pawn pawn)
            {
                if (pawn == null)
                {
                    Log.Error("Pawn null wut");
                }
                
                if (pawn.relations == null)
                {
                    Log.Error("Pawn rel null");
                }

                if (pawn.relations.DirectRelations == null)
                {
                    Log.Error("Dir null");
                }

                foreach (var r in pawn.relations.DirectRelations)
                {
                    if (r == null)
                    {
                        Log.Error("R null");
                    }

                    if (r.def == null)
                    {
                        Log.Error("R DEF NULL");
                    }

                    if (r.otherPawn == null)
                    {
                        Log.Error("Other pawn null");
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(ParentRelationUtility), "GetMother")]
        public static class Patch_2
        {
            [HarmonyPrefix]
            public static void Prefix_Test(Pawn pawn)
            {
                if (pawn == null)
                {
                    Log.Error("Pawn null wut");
                }
                
                if (pawn.relations == null)
                {
                    Log.Error("Pawn rel null");
                }

                if (pawn.relations.DirectRelations == null)
                {
                    Log.Error("Dir null");
                }
            }
        }
    }
    #endif
}