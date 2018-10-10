using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(WorldPawns), "ExposeData")]
    public class WorldPawns_ExposeData_Patch
    {
        private static readonly FieldInfo pawnsForcefullyKeptAsWorldPawnsField = AccessTools
            .Field(typeof(WorldPawns), "pawnsForcefullyKeptAsWorldPawns");

        private static readonly FieldInfo pawnsAliveField = AccessTools.Field(typeof(WorldPawns), "pawnsAlive");

        private static readonly FieldInfo pawnsMothballedField =
            AccessTools.Field(typeof(WorldPawns), "pawnsMothballed");

        private static readonly FieldInfo pawnsDeadField = AccessTools.Field(typeof(WorldPawns), "pawnsDead");
        
        // Unfortunately, gotta patch like this :(
        // TODO: transpile if for first scribe collections reference.
        public static bool Prefix(WorldPawns __instance)
        {
            if (PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status ==
                PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting)
                return true;

            var pawnsForcefullyKeptAsWorldPawns = (HashSet<Pawn>) pawnsForcefullyKeptAsWorldPawnsField.GetValue(__instance);

            var pawnsAlive = (HashSet<Pawn>) pawnsAliveField.GetValue(__instance);

            var pawnsMothballed =
                (HashSet<Pawn>) pawnsMothballedField.GetValue(__instance);

            var pawnsDead = (HashSet<Pawn>) pawnsDeadField.GetValue(__instance);
            
            // Use reference files.
            //Scribe_Collections.Look<Pawn>(ref this.pawnsForcefullyKeptAsWorldPawns, true, "pawnsForcefullyKeptAsWorldPawns", LookMode.Reference);
            ScribePawns(ref pawnsForcefullyKeptAsWorldPawns, true, "pawnsForcefullyKeptAsWorldPawns",
                LookMode.Reference);

            if (pawnsForcefullyKeptAsWorldPawns != null)
            {
                pawnsForcefullyKeptAsWorldPawnsField.SetValue(__instance, pawnsForcefullyKeptAsWorldPawns);
            }
            
            Scribe_Collections.Look<Pawn>(ref pawnsAlive, "pawnsAlive", LookMode.Deep);

            if (pawnsAlive != null)
            {
                pawnsAliveField.SetValue(__instance, pawnsAlive);
            }
            
            Scribe_Collections.Look<Pawn>(ref pawnsMothballed, "pawnsMothballed", LookMode.Deep);

            if (pawnsMothballed != null)
            {
                pawnsMothballedField.SetValue(__instance, pawnsMothballed);
            }
            
            Scribe_Collections.Look<Pawn>(ref pawnsDead, true, "pawnsDead", LookMode.Deep);

            if (pawnsDead != null)
            {
                pawnsDeadField.SetValue(__instance, pawnsDead);
            }
            
            Scribe_Deep.Look<WorldPawnGC>(ref __instance.gc, "gc");
            
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
                return false;
            BackCompatibility.WorldPawnPostLoadInit(__instance, ref pawnsMothballed);
            if (pawnsForcefullyKeptAsWorldPawns.RemoveWhere((Predicate<Pawn>) (x => x == null)) != 0)
                Log.Error("Some pawnsForcefullyKeptAsWorldPawns were null after loading.", false);
            
            pawnsForcefullyKeptAsWorldPawnsField.SetValue(__instance, pawnsForcefullyKeptAsWorldPawns);
            
            if (pawnsAlive.RemoveWhere((Predicate<Pawn>) (x => x == null)) != 0)
                Log.Error("Some pawnsAlive were null after loading.", false);
            
            pawnsAliveField.SetValue(__instance, pawnsAlive);
            
            if (pawnsMothballed.RemoveWhere((Predicate<Pawn>) (x => x == null)) != 0)
                Log.Error("Some pawnsMothballed were null after loading.", false);
            
            pawnsMothballedField.SetValue(__instance, pawnsMothballed);
            
            if (pawnsDead.RemoveWhere((Predicate<Pawn>) (x => x == null)) != 0)
                Log.Error("Some pawnsDead were null after loading.", false);
            
            pawnsDeadField.SetValue(__instance, pawnsDead);
            
            if (pawnsAlive.RemoveWhere((Predicate<Pawn>) (x =>
            {
                if (x.def != null)
                    return x.kindDef == null;
                return true;
            })) != 0)
                Log.Error("Some pawnsAlive had null def after loading.", false);
            
            pawnsAliveField.SetValue(__instance, pawnsAlive);
            
            if (pawnsMothballed.RemoveWhere((Predicate<Pawn>) (x =>
            {
                if (x.def != null)
                    return x.kindDef == null;
                return true;
            })) != 0)
                Log.Error("Some pawnsMothballed had null def after loading.", false);
            
            pawnsMothballedField.SetValue(__instance, pawnsMothballed);
            
            if (pawnsDead.RemoveWhere((Predicate<Pawn>) (x =>
            {
                if (x.def != null)
                    return x.kindDef == null;
                return true;
            })) == 0)
                return false;
            Log.Error("Some pawnsDead had null def after loading.", false);
            
            pawnsDeadField.SetValue(__instance, pawnsDead);
            
            return false;
        }

        private static void ScribePawns(ref HashSet<Pawn> set, bool saveDestroyedThings, string label, LookMode lookmode)
        {
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                    var setRef = set;
                    Scribe_Collections.Look<Pawn>(ref setRef, saveDestroyedThings, label, lookmode);
                    break;
                case LoadSaveMode.LoadingVars:
                    if (Scribe.EnterNode(label))
                    {
                        var curXmlParent = Scribe.loader.curXmlParent;
                        var attribute = curXmlParent.Attributes["IsNull"];

                        if (attribute != null && attribute.Value.ToLower() == "true")
                        {
                            set = null;
                            break;
                        }

                        set = new HashSet<Pawn>();
                        var targetLoadIDList = new List<string>(curXmlParent.ChildNodes.Count);
                        var enumerator = curXmlParent.ChildNodes.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var current = (XmlNode) enumerator.Current;
                            set.Add(ReferenceSaveLoader.GetReference<Pawn>(current.InnerText));
                        }
                    }
                    break;
            }
        }
    }
}