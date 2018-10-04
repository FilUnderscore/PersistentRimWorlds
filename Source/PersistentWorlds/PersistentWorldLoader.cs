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
            
            /*
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
            */

            var colonyFiles = coloniesDirectory.GetFiles("*.pwcf");
            
            for(var i = 0; i < colonyFiles.Length; i++)
            {
                var colonyFile = colonyFiles[i];
                
                Scribe.loader.InitLoading(colonyFile.FullName);
                persistentColonies.Add(LoadColonyData(i));
                Scribe.loader.ForceStop();
            }
            
            /**
             * 
             */

            List<string> files = new List<string>();

            files.Add(worldFilePath);
            mapsDirectory.GetFiles("*.pwmf").Do((FileInfo p) => files.Add(p.FullName));
            
            PersistentWorldManager.MultiLoader.InitLoading(files.ToArray());
            Log.Message("Loaded all files into MultiLoader.");
            
            PersistentWorldManager.MultiLoader.SetScribeCurXmlParentByFilePath(worldFilePath);
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
            persistentWorld.WorldData.ExposeData();
            //PersistentWorldManager.MultiLoader.CrossRefHandler = Scribe.loader.crossRefs;
            //PersistentWorldManager.MultiLoader.PostIniter = Scribe.loader.initer;
            //Scribe.loader.ForceStop();
            
            /* */

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

            //Scribe.mode = LoadSaveMode.LoadingVars;
            //Scribe.loader.crossRefs = PersistentWorldManager.MultiLoader.CrossRefHandler;
            //Scribe.loader.initer = PersistentWorldManager.MultiLoader.PostIniter;
            
            Log.Message("Persistent World File Name" + persistentWorld.fileName);
            
            foreach (var mapFile in mapsDirectory.GetFiles("*.pwmf"))
            {
                /*
                Scribe.loader.InitLoading(mapFile.FullName);
                maps.Add(LoadMapData());
                Log.Message("Resolve CrossRef");
                Scribe.loader.ForceStop();
                */
                
                Log.Message("MapFile:" + mapFile);
                
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
            
            Log.Message("Cur XML NODE NAME" + PersistentWorldManager.MultiLoader.curXmlNode.Name);
            Scribe_Deep.Look<Map>(ref map, PersistentWorldManager.MultiLoader.curXmlNode.Name);
            
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
    }
}