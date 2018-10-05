using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.Logic;
using Verse;
using Verse.Profile;
using Random = UnityEngine.Random;

namespace PersistentWorlds
{
    public sealed class PersistentWorldLoadSaver
    {
        public static readonly string SaveDir =
            (string) AccessTools.Method(typeof(GenFilePaths), "get_SavedGamesFolderPath", new Type[0]).Invoke(null, new object[0]);
     
        public static readonly string PersistentWorldFile_Extension = ".pwf";
        public static readonly string PersistentWorldColonyFile_Extension = ".pwcf";
        public static readonly string PersistentWorldMapFile_Extension = ".pwmf";
        
        private string worldFolderPath;
        private DirectoryInfo worldFolderDirectoryInfo;
        
        private string coloniesDirectory;
        private string mapsDirectory;
        
        private string worldFilePath;
        
        public PersistentWorldLoadStatus Status = PersistentWorldLoadStatus.Uninitialized;

        public enum PersistentWorldLoadStatus
        {
            Uninitialized,
            Creating,
            Converting,
            Loading,
            Finalizing,
            Ingame
        }
        
        public PersistentWorldLoadSaver(string worldFolderPath)
        {
            this.ConfigurePaths(worldFolderPath);
        }

        private void ConfigurePaths(string worldFolderPath)
        {
            this.worldFolderPath = worldFolderPath;

            this.worldFolderDirectoryInfo = new DirectoryInfo(worldFolderPath);

            this.coloniesDirectory = worldFolderPath + "/" + "Colonies";
            this.mapsDirectory = worldFolderPath + "/" + "Maps";

            this.worldFilePath = worldFolderPath + "/" + this.worldFolderDirectoryInfo.Name +
                                 PersistentWorldFile_Extension;
        }

        private void CreateDirectoriesIfNotExistant()
        {
            if (!Directory.Exists(this.worldFolderPath))
            {
                Directory.CreateDirectory(this.worldFolderPath);
            }

            if (!Directory.Exists(this.coloniesDirectory))
            {
                Directory.CreateDirectory(this.coloniesDirectory);
            }

            if (!Directory.Exists(this.mapsDirectory))
            {
                Directory.CreateDirectory(this.mapsDirectory);
            }
        }

        private void DeletePreviousDirectories()
        {
            // TODO: Review
            
            if (Directory.Exists(this.worldFolderPath))
            {
                Directory.Delete(this.worldFolderPath, true);
            }

            /*
            if (Directory.Exists(this.mapsDirectory))
            {
                Directory.Delete(this.mapsDirectory, true);
            }
            */
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
            
            /* TEST */
            // Select world to load XML node data for.
            //ScribeMultiLoader.SetScribeCurXmlParentByFilePath(this.worldFilePath);
            
            // Required otherwise errors because of internal requirements.
            //ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);

            //this.LoadColonies();
            
            // TODO: Review TEST
            
            /* TEST ^ */
            
            Log.Message("Loading world " + worldFolderPath);
            
            // Select world to load XML node data for.
            ScribeMultiLoader.SetScribeCurXmlParentByFilePath(this.worldFilePath);
            
            // Required otherwise errors because of internal requirements.
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
            
            // Load data.
            PersistentWorldManager.PersistentWorld = new PersistentWorld();
            PersistentWorldManager.PersistentWorld.WorldData.ExposeData();
            
            // TODO: Review
            //Scribe.loader.FinalizeLoading();
            
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

        public void SaveWorld(PersistentWorld world)
        {
            this.DeletePreviousDirectories();
            this.CreateDirectoriesIfNotExistant();
            
            Log.Message("Saving world...");
            
            // If any world changes were made.
            world.WorldData = PersistentWorldData.Convert(PersistentWorldManager.PersistentWorld.Game);
            
            SafeSaver.Save(this.worldFilePath, "world", delegate
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                world.WorldData.ExposeData();
            });
            
            Log.Message("Saved world data.");

            var sameNames = new Dictionary<string, int>(); // Fix overwriting multiple colonies that have same name / name that hasn't been set yet.
            
            foreach (var colony in world.Colonies)
            {
                // If colony changed name or data changed..
                if(PersistentWorldManager.PersistentWorld.Colony == colony)
                    colony.ColonyData = PersistentColonyData.Convert(PersistentWorldManager.PersistentWorld.Game);

                // TODO: Revise this fix one day.
                if (sameNames.ContainsKey(colony.ColonyData.ColonyFaction.Name))
                {
                    sameNames[colony.ColonyData.ColonyFaction.Name] = sameNames[colony.ColonyData.ColonyFaction.Name] + 1;
                }
                else
                {
                    sameNames.Add(colony.ColonyData.ColonyFaction.Name, 1);
                }

                var colonySaveFile = coloniesDirectory + "/" + sameNames[colony.ColonyData.ColonyFaction.Name].ToString() + colony.ColonyData.ColonyFaction.Name + PersistentWorldColonyFile_Extension;
                
                SafeSaver.Save(colonySaveFile, "colony", delegate
                {
                    colony.ColonyData.ExposeData();
                });
            }
            
            Log.Message("Saved colony data.");

            // TODO: Save any newly created maps, if not already.
            foreach (var map in world.Maps)
            {
                var mapSaveFile = mapsDirectory + "/" + map.Tile.ToString() + PersistentWorldMapFile_Extension;
                
                SafeSaver.Save(mapSaveFile, "map", delegate
                {
                    map.ExposeData();
                });
            }
            
            Log.Message("Saved map data.");
            
            Log.Message("Saved world.");
        }
        
        /**
         * CONVERT
         */
        
        public void Convert(Game game)
        {
            PersistentWorldManager.PersistentWorld = PersistentWorld.Convert(game);
            
            this.ConfigurePaths(SaveDir + "/" + game.World.info.name);
            this.CreateDirectoriesIfNotExistant();
            
            this.SaveWorld(PersistentWorldManager.PersistentWorld);
            
            GenScene.GoToMainMenu();
            
            // TODO: Call these when on main menu.. if called before on main menu, causes world corruption :/
//            MemoryUtility.ClearAllMapsAndWorld();
//            MemoryUtility.UnloadUnusedUnityAssets();

            this.Status = PersistentWorldLoadStatus.Uninitialized;
        }
        
        /**
         * MISC
         */
        
        public void TransferToPlayScene()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                // TODO: Run MemoryUtility.ClearAllMapsAndWorld() when Loading world from filelist.
                Status = PersistentWorldLoadStatus.Finalizing;

                Current.Game = new Game {InitData = new GameInitData {gameToLoad = "PersistentWorld"}}; // Just to get the SavedGameLoaderNow.LoadGameFromSaveFileNow() patch to load.
            }, "Play", "LoadingLongEvent", true, null);
        }
    }
}