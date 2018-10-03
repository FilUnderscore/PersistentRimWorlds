using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.Logic;
using RimWorld;
using Verse;

namespace PersistentWorlds
{
    public class PersistentWorldLoader
    {
        public static readonly string SaveDir =
            (string) AccessTools.Method(typeof(GenFilePaths), "get_SavedGamesFolderPath", new Type[0]).Invoke(null, new object[0]);
        
        public void LoadWorldNow(string fileName)
        {
            Log.Message("PersistentWorlds - Loading world " + fileName);

            LoadGame(fileName);
        }

        private void LoadGame(string fileName)
        {
            var worldDirectory = Directory.CreateDirectory(SaveDir + "/" + fileName);
            var coloniesDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Colonies");
            var mapsDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Maps");
            
            var worldFilePath = worldDirectory.FullName + "/" + fileName + ".pwf";
            
            Log.Message("PersistentWorlds - Loading after Dir set.");
            
            Scribe.loader.InitLoading(worldFilePath);
            var persistentWorld = LoadWorldData();
            Scribe.loader.FinalizeLoading();

            Log.Message("PersistentWorlds - WorldData loaded.");
            
            var persistentMaps = new List<PersistentMap>();
            
            foreach (var mapFile in mapsDirectory.GetFiles("*.pwmf"))
            {
                Scribe.loader.InitLoading(mapFile.FullName);
                persistentMaps.Add(LoadMapData());
                Scribe.loader.FinalizeLoading();
            }
            
            Log.Message("PersistentWorlds - MapData loaded.");

            var persistentColonies = new List<PersistentColony>();
            
            foreach (var colonyFile in coloniesDirectory.GetFiles("*.pwcf"))
            {
                Scribe.loader.InitLoading(colonyFile.FullName);
                persistentColonies.Add(LoadColonyData());
                Scribe.loader.FinalizeLoading();
            }
            
            Log.Message("PersistentWorlds - ColonyData loaded.");

            persistentWorld.Maps = persistentMaps;
            persistentWorld.Colonies = persistentColonies;
            
            Log.Message("PersistentWorlds - Loading World...");
            
            PersistentWorldManager.PersistentWorld = persistentWorld;
            persistentWorld.LoadWorld();
        }

        private PersistentWorld LoadWorldData()
        {
            
        }
        
        private PersistentMap LoadMapData()
        {
            
        }
        
        private PersistentColony LoadColonyData()
        {
            
        }
    }
}