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

        private const string PersistentWorldFile_Extension = ".pwf";
        private const string PersistentWorldColonyFile_Extension = ".pwcf";
        private const string PersistentWorldMapFile_Extension = ".pwmf";

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
            Scribe_Deep.Look<PersistentWorldData>(ref PersistentWorldManager.PersistentWorld.WorldData, "data");
            
            PersistentWorldManager.PersistentWorld.ResetPlayerFaction();
            
            Log.Message("Loaded world data...");
        }

        /// <summary>
        /// Loads all colony data for a specific colony. Fully loads the referenced colony.
        /// </summary>
        /// <param name="colony"></param>
        public void LoadColony(ref PersistentColony colony)
        {
            var file = colony.FileInfo;

            Log.Message("Loading colony... " + Path.GetFileNameWithoutExtension(file.FullName));
            
            ScribeMultiLoader.SetScribeCurXmlParentByFilePath(file.FullName);

            Scribe_Deep.Look(ref colony, "colony");

            PersistentWorldManager.PersistentWorld.Colony = colony;
            
            Log.Message("Loaded colony.");
        }

        /// <summary>
        /// Loads some colony information for loading screens.
        /// </summary>
        public void LoadColonies()
        {
            var colonyFiles = new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFile_Extension);

            Log.Message("Loading colonies...");
            
            foreach (var colonyFile in colonyFiles)
            {                
                ScribeMultiLoader.SetScribeCurXmlParentByFilePath(colonyFile.FullName);

                var colony = new PersistentColony();
                
                if (Scribe.EnterNode("colony"))
                {
                    if (Scribe.EnterNode("data"))
                    {
                        colony.ColonyData.PreExposeData();
                        
                        Scribe.ExitNode();
                    }
                    
                    Scribe.ExitNode();
                }

                colony.FileInfo = colonyFile;
                
                PersistentWorldManager.PersistentWorld.Colonies.Add(colony);
            }
            
            Log.Message("Loaded colony data...");
        }

        public IEnumerable<Map> LoadMaps(int[] mapTiles)
        {
            var mapFiles = new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFile_Extension);
            
            Log.Message("Loading maps v2...");

            var maps = new List<Map>();

            if (Scribe.mode == LoadSaveMode.Inactive)
            {
                this.PreloadWorldColoniesMaps();
            }

            foreach (var mapFile in mapFiles)
            {
                if (!mapTiles.Any(tile => mapFile.FullName.Contains(tile.ToString()))) continue;
                
                Log.Message("Scribing map " + mapFile);
                
                if(PersistentWorldManager.WorldLoadSaver.Status != PersistentWorldLoadStatus.Ingame)
                    ScribeMultiLoader.SetScribeCurXmlParentByFilePath(mapFile.FullName);
                else
                {
                    Scribe.loader.InitLoading(mapFile.FullName);
                }
                
                var map = new Map();

                Log.Message("Status: " + Scribe.mode);
                Scribe_Deep.Look<Map>(ref map, "map");

                if (PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadStatus.Ingame)
                {
                    Scribe.loader.FinalizeLoading();
                }

                maps.Add(map);
            }

            if (PersistentWorldManager.WorldLoadSaver.Status != PersistentWorldLoadStatus.Ingame)
            {
                Scribe.loader.FinalizeLoading();
            }

            Status = PersistentWorldLoadStatus.Ingame;
            
            ScribeMultiLoader.Clear();

            Log.Message("Loaded map data...");
            
            return maps;
        }
        
        /**
         * SAVING
         */

        public void SaveWorld(PersistentWorld world)
        {
            ReferenceSaveLoader.ClearReferences();
            
            this.CreateDirectoriesIfNotExistant();

            Status = PersistentWorldLoadStatus.Saving;
            Log.Message("Saving world...");
            
            PersistentWorldManager.PersistentWorld.ConvertCurrentGameSettlements(PersistentWorldManager.PersistentWorld.Game);
            
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
                if (sameNames.ContainsKey(colony.ColonyData.ColonyFaction.Name))
                {
                    sameNames[colony.ColonyData.ColonyFaction.Name] = sameNames[colony.ColonyData.ColonyFaction.Name] + 1;
                }
                else
                {
                    sameNames.Add(colony.ColonyData.ColonyFaction.Name, 1);
                }

                var colonySaveFile = coloniesDirectory + "/" + sameNames[colony.ColonyData.ColonyFaction.Name].ToString() + colony.ColonyData.ColonyFaction.Name + PersistentWorldColonyFile_Extension;
                
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
            
            // TODO: Revert.
            PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();
            
            Log.Message("Saving references.");
            ReferenceSaveLoader.SaveReferences();
            
            Log.Message("Saved world.");
        }
        
        /**
         * CONVERT
         */
        
        public void Convert(Game game)
        {
            this.ConfigurePaths(SaveDir + "/" + game.World.info.name);
            this.CreateDirectoriesIfNotExistant();
            
            PersistentWorldManager.PersistentWorld = PersistentWorld.Convert(game);
            
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

        public string GetWorldFolder()
        {
            return this.worldFolderPath;
        }
    }
}