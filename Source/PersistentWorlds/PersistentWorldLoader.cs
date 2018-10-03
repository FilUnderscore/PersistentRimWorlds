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
            
            PersistentWorldManager.PersistentWorld = new PersistentWorld();
            var persistentColonies = new List<PersistentColony>();
            
            foreach (var colonyFile in coloniesDirectory.GetFiles("*.pwcf"))
            {
                Scribe.loader.InitLoading(colonyFile.FullName);
                persistentColonies.Add(LoadColonyData());
                Scribe.loader.ForceStop();
            }
            
            Scribe.loader.InitLoading(worldFilePath);
            var persistentWorld = LoadWorldData();
            persistentWorld.fileName = fileName;
            Scribe.loader.ForceStop();
            
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
                Log.Message("Resolve CrossRef");
                Scribe.loader.ForceStop();
            }

            persistentWorld.Maps = maps;
        }

        private PersistentWorld LoadWorldData()
        {
            PersistentWorld persistentWorld = new PersistentWorld();
            PersistentWorldManager.PersistentWorld = persistentWorld;
            
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
            
            Scribe_Deep.Look<Map>(ref map, "map");
            
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