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
            persistentWorld.fileName = fileName;
            Scribe.loader.FinalizeLoading();

            Log.Message("PersistentWorlds - WorldData loaded.");
            
            var persistentColonies = new List<PersistentColony>();
            
            foreach (var colonyFile in coloniesDirectory.GetFiles("*.pwcf"))
            {
                Scribe.loader.InitLoading(colonyFile.FullName);
                persistentColonies.Add(LoadColonyData());
                Scribe.loader.FinalizeLoading();
            }
            
            Log.Message("PersistentWorlds - ColonyData loaded.");

            persistentWorld.Colonies = persistentColonies;
            
            Log.Message("PersistentWorlds - Loading World...");
            
            persistentWorld.LoadWorld();
        }

        

        public void LoadMaps(PersistentWorld persistentWorld)
        {
            Log.Warning("Calling LoadMaps in Loader.");
            
            var fileName = persistentWorld.fileName;
            
            var worldDirectory = Directory.CreateDirectory(SaveDir + "/" + fileName);
            var coloniesDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Colonies");
            var mapsDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Maps");
            
            var worldFilePath = worldDirectory.FullName + "/" + fileName + ".pwf";
            
            var maps = new List<Map>();
            
            foreach (var mapFile in mapsDirectory.GetFiles("*.pwmf"))
            {
                Scribe.loader.InitLoading(mapFile.FullName);
                maps.Add(LoadMapData());
                Scribe.loader.FinalizeLoading();
            }
            
            Log.Warning("MapData loaded.");

            persistentWorld.Maps = maps;
        }

        private PersistentWorld LoadWorldData()
        {
            PersistentWorld persistentWorld = new PersistentWorld();
            PersistentWorldManager.PersistentWorld = persistentWorld;
            
            Log.Message("Run GameHeader");
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
            
            Log.Warning("Calling ExposeData on PersistentWorldData soon...");
            persistentWorld.WorldData = new PersistentWorldData();
            persistentWorld.WorldData.ExposeData();

            return persistentWorld;
        }
        
        private Map LoadMapData()
        {
            Log.Warning("Calling LoadMapData.");
            
            Map map = new Map();
            
            if(Scribe.EnterNode("map"))
            {
                Log.Warning("Entering Map Node");
                map.ExposeData();
            }
            return map;
        }
        
        private PersistentColony LoadColonyData()
        {
            PersistentColony persistentColony = new PersistentColony();
            
            Log.Warning("Calling ExposeData on PersistentColonyData soon...");
            persistentColony.ColonyData = new PersistentColonyData();
            persistentColony.ColonyData.ExposeData();

            return persistentColony;
        }
    }
}