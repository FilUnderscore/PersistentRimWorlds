using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /*
        public void LoadWorldNow(string fileName)
        {
            Log.Message("PersistentWorlds - Loading world " + fileName);

            Current.Game = PersistentWorldManager.PersistentWorld.Game;
            PersistentWorldManager.LoadStatus = PersistentWorldManager.PersistentWorldLoadStatus.Loading;
            
            LoadGame(fileName);
        }

        private void LoadGame(string fileName)
        {
            var worldDirectory = Directory.CreateDirectory(SaveDir + "/" + fileName);
            var coloniesDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Colonies");
            var mapsDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Maps");
            
            var worldFilePath = worldDirectory.FullName + "/" + fileName + ".pwf";

            var persistentWorld = PersistentWorldManager.PersistentWorld;
            
            var persistentColonies = new List<PersistentColony>();
            
            List<string> files = new List<string>();

            files.Add(worldFilePath);
            mapsDirectory.GetFiles("*.pwmf").Do((FileInfo p) => files.Add(p.FullName));
            
            var colonyFiles = coloniesDirectory.GetFiles("*.pwcf");
            colonyFiles.Do((FileInfo p) => files.Add(p.FullName));
            
            PersistentWorldManager.MultiLoader.InitLoading(files.ToArray());

            for(var i = 0; i < colonyFiles.Length; i++)
            {
                var colonyFile = colonyFiles[i];
                
                PersistentWorldManager.MultiLoader.SetScribeCurXmlParentByFilePath(colonyFile.FullName);
                persistentColonies.Add(LoadColonyData(i));
            }
            
            Log.Message("Loaded all files into MultiLoader.");
            
            PersistentWorldManager.MultiLoader.SetScribeCurXmlParentByFilePath(worldFilePath);
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
            persistentWorld.WorldData.ExposeData();

            persistentWorld.fileName = fileName;
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

            Log.Message("Persistent World File Name" + persistentWorld.fileName);
            
            foreach (var mapFile in mapsDirectory.GetFiles("*.pwmf"))
            {
                PersistentWorldManager.MultiLoader.SetScribeCurXmlParentByFilePath(mapFile.FullName);
                maps.Add(LoadMapData());
            }
            
            Scribe.loader.FinalizeLoading();

            persistentWorld.Maps = maps;
        }

        private Map LoadMapData()
        {
            Log.Warning("Calling LoadMapData.");

            Map map = new Map();
            map.ExposeData();
            
            return map;
        }
        
        private PersistentColony LoadColonyData(int i)
        {
            PersistentColony persistentColony = new PersistentColony();
            
            Log.Warning("Checkout this line!!!");
            // TODO: Investigate... why we need index - 1?
            if (PersistentWorldManager.LoadColonyIndex - 1 == i)
                PersistentWorldManager.PersistentWorld.colony = persistentColony;

            Log.Warning("Calling ExposeData on PersistentColonyData soon...");
            persistentColony.ColonyData = new PersistentColonyData();
            persistentColony.ColonyData.ExposeData();

            return persistentColony;
        }
        */
    }
}