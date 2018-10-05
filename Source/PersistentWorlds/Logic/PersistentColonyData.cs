using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData : IExposable
    {
        public PersistentColonyGameData GameData = new PersistentColonyGameData();

        public Faction ColonyFaction;
        public Pawn ColonyLeader; // TODO: Support mod like Psychology with Mayor or RelationsTab in the future.
        
        public void ExposeData()
        {
            Scribe_Deep.Look<Faction>(ref ColonyFaction, "faction");
            Scribe_Deep.Look<Pawn>(ref ColonyLeader, "leader");
            
            Scribe_Deep.Look<PersistentColonyGameData>(ref GameData, "gameData");
        }

        public static PersistentColonyData Convert(Game game)
        {
            var persistentColonyData = new PersistentColonyData();

            persistentColonyData.ColonyFaction = game.World.factionManager.OfPlayer;
            
            persistentColonyData.GameData = PersistentColonyGameData.Convert(game);
            
            return persistentColonyData;
        }
    }
}