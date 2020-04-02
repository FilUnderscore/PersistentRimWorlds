using HarmonyLib;
using PersistentWorlds.Logic.Comps;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
    public class PawnNameColorUtility_PawnNameColorOf_Patch
    {   
        static bool Prefix(Pawn pawn, ref Color __result)
        {
            // TODO: Check if persistent world and if pawn is from different colony.

            var comp = pawn.GetComp<HumanColonyThingComp>();

            if (comp == null || comp.ColonyId == -1 || Equals(comp.ColonyId,
                PersistentWorldManager.GetInstance().PersistentWorld.Colony.ColonyData.UniqueId)) return true;

            __result = PersistentWorldManager.GetInstance().PersistentWorld.GetColonyById(comp.ColonyId).ColonyData
                .Color;
                
            return false;

        }
    }
}