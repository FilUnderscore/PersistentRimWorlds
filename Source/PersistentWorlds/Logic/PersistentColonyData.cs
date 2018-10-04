﻿using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData : IExposable
    {
        public PersistentColonyGameData GameData = new PersistentColonyGameData();

        public string ColonyName;
        public Pawn ColonyLeader; // TODO: Support mod like Psychology with Mayor or RelationsTab in the future.
        
        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref ColonyName, "name");
            Scribe_Deep.Look<Pawn>(ref ColonyLeader, "leader");
            
            Scribe_Deep.Look<PersistentColonyGameData>(ref GameData, "gameData");
        }

        public static PersistentColonyData Convert(Game game)
        {
            var persistentColonyData = new PersistentColonyData();

            persistentColonyData.ColonyName = game.World.factionManager.OfPlayer.Name;
            
            persistentColonyData.GameData = PersistentColonyGameData.Convert(game);
            
            return persistentColonyData;
        }
    }
}