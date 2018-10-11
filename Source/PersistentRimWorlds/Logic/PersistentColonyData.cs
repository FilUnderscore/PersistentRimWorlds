using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData : IExposable, ILoadReferenceable
    {
        // TODO: Also implement enemy raids for colonies and trading colony inventories.
        public Faction ColonyFaction;
        public int uniqueID = 0;

        // TODO: Allow color picking colonies.
        public Color color;
        
        // Used to load maps for colonies, 2 colonies can have the same tile loaded at the same time.
        public List<int> ActiveWorldTiles = new List<int>();

        // TODO: Preload only colony faction / color for selection, load when switching or loading.
        public void ExposeData()
        {
            Scribe_Values.Look(ref uniqueID, "uniqueID", -1);
            
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
            
            Scribe_Collections.Look(ref ActiveWorldTiles, "activeWorldTiles");
        }

        public static PersistentColonyData Convert(Game game, PersistentColonyData colonyColonyData)
        {
            var persistentColonyData = new PersistentColonyData
            {
                ColonyFaction = game.World.factionManager.OfPlayer
            };

            if (PersistentWorldManager.PersistentWorld == null || PersistentWorldManager.PersistentWorld.Colony == null)
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
            }

            return persistentColonyData;
        }
        
        public string GetUniqueLoadID()
        {
            return "Colony_" + this.uniqueID;
        }
    }
}