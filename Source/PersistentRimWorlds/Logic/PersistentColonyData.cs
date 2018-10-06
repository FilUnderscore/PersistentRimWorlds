using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData : IExposable, ILoadReferenceable
    {
        public PersistentColonyGameData GameData = new PersistentColonyGameData();

        // TODO: Implement support for 'Colonies' instead of using Factions and custom settlement world objects.
        // TODO: Also implement enemy raids for colonies and trading colony inventories.
        public Faction ColonyFaction;
        public int uniqueID = 0;
        
        // Used to load maps for colonies, 2 colonies can have the same tile loaded at the same time.
        public List<int> ActiveWorldTiles = new List<int>();
        
        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref uniqueID, "uniqueID", -1, false);
            
            Scribe_Deep.Look<PersistentColonyGameData>(ref GameData, "gameData");
            
            Scribe_Collections.Look<int>(ref ActiveWorldTiles, "activeWorldTiles");
            
            // Scribe_Deep.Look<Faction>(ref ColonyFaction, "faction");
            // Don't need to cross-ref.
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (Scribe.EnterNode("faction"))
                {
                    this.ColonyFaction = new Faction();
                    this.ColonyFaction.ExposeData();
                }
                else
                {
                    Log.Message("No Faction :/");
                }
            }
            else if(Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Deep.Look<Faction>(ref ColonyFaction, "faction");
            }
        }

        public static PersistentColonyData Convert(Game game, PersistentColonyData colonyColonyData)
        {
            var persistentColonyData = new PersistentColonyData
            {
                // TODO: Review
                ColonyFaction = game.World.factionManager.OfPlayer,
                GameData = PersistentColonyGameData.Convert(game)
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