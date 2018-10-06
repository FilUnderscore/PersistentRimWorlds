using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Ingame,
            Saving
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
            
            /*
            if (Directory.Exists(this.worldFolderPath))
            {
                Directory.Delete(this.worldFolderPath, true);
            }
            */

            if (Directory.Exists(this.coloniesDirectory))
            {
                Directory.Delete(this.coloniesDirectory, true);
            }
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
            //PersistentWorldManager.PersistentWorld.WorldData.ExposeData();
            Scribe_Deep.Look<PersistentWorldData>(ref PersistentWorldManager.PersistentWorld.WorldData, "data");
            
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
                //colony.ExposeData();
                Scribe_Deep.Look<PersistentColony>(ref colony, "colony");
                
                PersistentWorldManager.PersistentWorld.Colonies.Add(colony);
            }
            
            Log.Message("Loaded colony data...");
        }

        public void LoadMaps()
        {
            var mapFiles = new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFile_Extension);
            
            Log.Message("Loading maps...");

            var maps = new List<Map>();
            
            foreach (var mapFile in mapFiles)
            {
                ScribeMultiLoader.SetScribeCurXmlParentByFilePath(mapFile.FullName);

                var map = new Map();
//                map.ExposeData();
                Scribe_Deep.Look<Map>(ref map, "map");

                // TODO: Check if map is being used by a colony loaded right now...
                
                maps.Add(map);
            }
            
            AccessTools.Field(typeof(Game), "maps").SetValue(PersistentWorldManager.PersistentWorld.Game, maps);

            Status = PersistentWorldLoadStatus.Ingame;
            // Basically ingame at this point :/
            
            Scribe.loader.FinalizeLoading();
            ScribeMultiLoader.Clear();

            // TODO: Maybe relocate?
            PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();

            Log.Message("Loaded map data...");
        }
        
        /**
         * SAVING
         */

        public void SaveWorld(PersistentWorld world)
        {
            this.DeletePreviousDirectories();
            this.CreateDirectoriesIfNotExistant();

            Status = PersistentWorldLoadStatus.Saving;
            Log.Message("Saving world...");
            
            // If any world changes were made.
            world.WorldData = PersistentWorldData.Convert(PersistentWorldManager.PersistentWorld.Game);

            SafeSaver.Save(this.worldFilePath, "worldfile", delegate
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                //world.WorldData.ExposeData();
                Scribe_Deep.Look<PersistentWorldData>(ref world.WorldData, "data");
            });
            
            Log.Message("Saved world data.");

            var sameNames = new Dictionary<string, int>(); // Fix overwriting multiple colonies that have same name / name that hasn't been set yet.
            
            foreach (var colony in world.Colonies)
            {
                // If colony changed name or data changed..
                if(PersistentWorldManager.PersistentWorld.Colony == colony)
                    colony.ColonyData = PersistentColonyData.Convert(PersistentWorldManager.PersistentWorld.Game, colony.ColonyData);

                // TODO: Revise this fix one day.
                if (sameNames.ContainsKey(colony.AsFaction().Name))
                {
                    sameNames[colony.AsFaction().Name] = sameNames[colony.AsFaction().Name] + 1;
                }
                else
                {
                    sameNames.Add(colony.AsFaction().Name, 1);
                }

                var colonySaveFile = coloniesDirectory + "/" + sameNames[colony.AsFaction().Name].ToString() + colony.AsFaction().Name + PersistentWorldColonyFile_Extension;
                
                SafeSaver.Save(colonySaveFile, "colonyfile", delegate
                {
                    //colony.ExposeData();
                    var colony1 = colony;
                    Scribe_Deep.Look<PersistentColony>(ref colony1, "colony");
                });
            }
            
            Log.Message("Saved colony data.");

            // TODO: Save any newly created maps, if not already.
            
            foreach (var map in Current.Game.Maps)
            {
                var mapSaveFile = mapsDirectory + "/" + map.Tile.ToString() + PersistentWorldMapFile_Extension;
                
                SafeSaver.Save(mapSaveFile, "mapfile", delegate
                {
                    var map1 = map;
                    Scribe_Deep.Look<Map>(ref map1, "map");
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