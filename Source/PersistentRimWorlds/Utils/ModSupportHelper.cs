using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PersistentWorlds.Utils
{
    public class ModSupportHelper
    {
        private static bool _psychologyLoaded = true;
        
        private static HediffDef _mayorHediffDef;
        
        // Adapted from Fluffy's Relations Tab.
        // https://github.com/fluffy-mods/RelationsTab/blob/8455fe579633b8d62fd7ce304a13114ea8e803a2/Source/Fluffy_Relations/RelationsHelper.cs#L84
        public static bool IsPsychologyLoaded()
        {
            if (_mayorHediffDef == null && _psychologyLoaded)
            {
                _mayorHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("Mayor");
                _psychologyLoaded = _mayorHediffDef != null;

                return IsPsychologyLoaded();
            }

            return _psychologyLoaded;
        }

        public static Pawn GetPsychologyMayor()
        {
            var mayors =
                PawnsFinder.AllMapsAndWorld_Alive.Where(p => p.health.hediffSet.HasHediff(_mayorHediffDef));

            var mayorsList = mayors.ToList();
            
            return mayorsList.Count == 0 ? null : mayorsList.OrderByDescending(p => p.MapHeld?.mapPawns.ColonistCount).First();
        }
    }
}