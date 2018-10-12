using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    #if DEBUG
    public class Debug_Patches
    {
        #region Classes

        [HarmonyPatch(typeof(RimWorld.Planet.World), "ConstructComponents")]
        public static class World_ConstructComponents_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(RimWorld.Planet.World __instance)
            {
                Log.Message("1");
                __instance.worldObjects = new WorldObjectsHolder();
                __instance.factionManager = new FactionManager();
                __instance.worldPawns = new WorldPawns();
                __instance.gameConditionManager = new GameConditionManager(__instance);
                __instance.storyState = new StoryState((IIncidentTarget) __instance);
                __instance.renderer = new WorldRenderer();
                __instance.UI = new WorldInterface();
                __instance.debugDrawer = new WorldDebugDrawer();
                __instance.dynamicDrawManager = new WorldDynamicDrawManager();
                __instance.pathFinder = new WorldPathFinder();
                __instance.pathPool = new WorldPathPool();
                __instance.reachability = new WorldReachability();
                __instance.floodFiller = new WorldFloodFiller();
                __instance.ticksAbsCache = new ConfiguredTicksAbsAtGameStartCache();
                Log.Message("2");
                __instance.components.Clear();
                Log.Message("3");
                AccessTools.Method(typeof(RimWorld.Planet.World), "FillComponents").Invoke(__instance, new object[0]);
                Log.Message("4");
                return false;
            }
        }
        
        #endregion
    }
    #endif
}