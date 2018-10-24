using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;
using PersistentWorlds.Utils;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData : IExposable, ILoadReferenceable
    {
        #region Fields
        // TODO: Also implement enemy raids for colonies and trading colony inventories.
        public Faction ColonyFaction;
        public int uniqueID = 0;
        
        // TODO: Allow color picking colonies.
        public Color color = Color.white;

        public Pawn Leader;
        
        // Used to load maps for colonies, 2 colonies can have the same tile loaded at the same time.
        public List<int> ActiveWorldTiles = new List<int>();
        #endregion
        
        #region Methods
        public void ExposeData()
        {
            Scribe_Values.Look(ref uniqueID, "uniqueID", -1);
            Scribe_Values.Look(ref color, "color", Color.white);
            
            switch (Scribe.mode)
            {
                case LoadSaveMode.LoadingVars when Scribe.EnterNode("faction"):
                    this.ColonyFaction = new Faction();
                    this.ColonyFaction.ExposeData();
                    Scribe.ExitNode();
                    break;
                case LoadSaveMode.LoadingVars:
                    Log.Error("No faction for colony found. Corrupt colony save?");
                    break;
                case LoadSaveMode.Saving:
                    Scribe_Deep.Look<Faction>(ref ColonyFaction, "faction");
                    break;
            }
            
            Scribe_References.Look(ref Leader, "leader");
            
            Scribe_Collections.Look(ref ActiveWorldTiles, "activeWorldTiles");
        }

        public static PersistentColonyData Convert(Game game, PersistentColonyData colonyColonyData)
        {
            var persistentColonyData = new PersistentColonyData
            {
                ColonyFaction = game.World.factionManager.OfPlayer
            };

            if (colonyColonyData == null || colonyColonyData.ActiveWorldTiles.Count == 0)
            {
                foreach (var map in game.Maps)
                {
                    if (persistentColonyData.ActiveWorldTiles.Contains(map.Tile)) continue;
                    
                    persistentColonyData.ActiveWorldTiles.Add(map.Tile);
                }
            }
            else
            {
                persistentColonyData.uniqueID = colonyColonyData.uniqueID;
                persistentColonyData.ActiveWorldTiles = colonyColonyData.ActiveWorldTiles;

                persistentColonyData.color = colonyColonyData.color;
                persistentColonyData.Leader = colonyColonyData.Leader;
            }

            return persistentColonyData;
        }
        
        public string GetUniqueLoadID()
        {
            return "Colony_" + this.uniqueID;
        }

        public override string ToString()
        {
            return $"{nameof(PersistentColonyData)} " +
                   $"({nameof(ColonyFaction)}={ColonyFaction}, " +
                   $"{nameof(uniqueID)}={uniqueID}, " +
                   $"{nameof(color)}={color}, " +
                   $"{nameof(ActiveWorldTiles)}={ActiveWorldTiles.ToDebugString()})";
        }
        #endregion
    }
}