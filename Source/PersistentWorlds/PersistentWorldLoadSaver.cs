using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.Logic;
using Verse;
using Random = UnityEngine.Random;

namespace PersistentWorlds
{
    public sealed class PersistentWorldLoadSaver
    {
        /*
        public static readonly string SaveDir =
            (string) AccessTools.Method(typeof(GenFilePaths), "get_SavedGamesFolderPath", new Type[0]).Invoke(null, new object[0]);
        */

        public static readonly string PersistentWorldFile_Extension = ".pwf";
        public static readonly string PersistentWorldColonyFile_Extension = ".pwcf";
        public static readonly string PersistentWorldMapFile_Extension = ".pwmf";
        
        private string worldFolderPath;
        private DirectoryInfo worldFolderDirectoryInfo;
        
        private string coloniesDirectory;
        private string mapsDirectory;
        
        private string worldFilePath;
        
        public PersistentWorldLoadStatus Status;

        public enum PersistentWorldLoadStatus
        {
            Uninitialized,
            Creating,
            Loading,
            Finalizing,
            Ingame
        }
        
        public PersistentWorldLoadSaver(string worldFolderPath)
        {
            this.worldFolderPath = worldFolderPath;
            this.worldFolderDirectoryInfo = new DirectoryInfo(worldFolderPath);

            this.coloniesDirectory = worldFolderPath + "/" + "Colonies";
            this.mapsDirectory = worldFolderPath + "/" + "Maps";

            this.worldFilePath = worldFolderPath + "/" + this.worldFolderDirectoryInfo.Name +
                                 PersistentWorldFile_Extension;
        }
        
        /**
         * LOADING
         */

        public void PreloadWorldColoniesMaps()
        {
            var files = new List<string>();

            files.Add(this.worldFilePath);
            new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFile_Extension).Do(colonyFile => files.Add(colonyFile.FullName));
            new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFile_Extension).Do(mapFile => files.Add(mapFile.FullName));
            
            ScribeMultiLoader.InitLoading(files.ToArray());
            
            Log.Message("Preloaded World, Colonies, and Maps.");
        }
        
        public void LoadWorld()
        {
            Status = PersistentWorldLoadStatus.Loading;
            
            this.PreloadWorldColoniesMaps();
            
            Log.Message("Loading world " + worldFolderPath);
            
            // Select world to load XML node data for.
            ScribeMultiLoader.SetScribeCurXmlParentByFilePath(this.worldFilePath);
            
            // Required otherwise errors because of internal requirements.
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
            
            // Load data.
            PersistentWorldManager.PersistentWorld = new PersistentWorld();
            PersistentWorldManager.PersistentWorld.WorldData.ExposeData();
            
            Log.Message("Loaded world data...");
        }

        public void LoadColonies()
        {
            var colonyFiles = new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFile_Extension);

            Log.Message(this.coloniesDirectory);
            
            Log.Message("Loading colonies...");
            
            foreach (var colonyFile in colonyFiles)
            {
                Log.Message("Colony File: " + colonyFile.FullName);
                
                ScribeMultiLoader.SetScribeCurXmlParentByFilePath(colonyFile.FullName);
                
                var colony = new PersistentColony();
                colony.ColonyData.ExposeData();

                PersistentWorldManager.PersistentWorld.Colonies.Add(colony);
            }
            
            Log.Message("Loaded colony data...");
        }

        public void LoadMaps()
        {
            var mapFiles = new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFile_Extension);
            
            Log.Message("Loading maps...");

            foreach (var mapFile in mapFiles)
            {
                ScribeMultiLoader.SetScribeCurXmlParentByFilePath(mapFile.FullName);

                var map = new Map();
                map.ExposeData();

                PersistentWorldManager.PersistentWorld.Maps.Add(map);
            }

            Status = PersistentWorldLoadStatus.Ingame;
            // Basically ingame at this point :/
            
            Scribe.loader.FinalizeLoading();
            
            Log.Message("Loaded map data...");
        }
        
        /**
         * SAVING
         */
         
        /**
         * MISC
         */
        
        public void TransferToPlayScene()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                // TODO: Run MemoryUtility.ClearAllMapsAndWorld() when Loading world from filelist.
                Status = PersistentWorldLoadStatus.Finalizing;
                
                Current.Game = new Game();
                Current.Game.InitData = new GameInitData();
                Current.Game.InitData.gameToLoad = "PersistentWorld"; // Just to get the SavedGameLoaderNow.LoadGameFromSaveFileNow() patch to load.
            }, "Play", "LoadingLongEvent", true, null);
        }
    }
}