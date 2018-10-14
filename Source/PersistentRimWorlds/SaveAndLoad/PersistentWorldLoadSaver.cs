using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using Verse;
using Verse.Profile;
using Random = UnityEngine.Random;

namespace PersistentWorlds.SaveAndLoad
{
    public sealed class PersistentWorldLoadSaver
    {
        #region Fields
        public static readonly string SaveDir =
            (string) AccessTools.Method(typeof(GenFilePaths), "get_SavedGamesFolderPath", new Type[0]).Invoke(null, new object[0]);

        private PersistentWorld PersistentWorld;
        
        private const string PersistentWorldFile_Extension = ".pwf";
        private const string PersistentWorldColonyFile_Extension = ".pwcf";
        private const string PersistentWorldMapFile_Extension = ".pwmf";

        private string worldFolderPath;
        private DirectoryInfo worldFolderDirectoryInfo;
        
        private string coloniesDirectory;
        private string mapsDirectory;
        
        private string worldFilePath;
        
        public PersistentWorldLoadStatus Status = PersistentWorldLoadStatus.Uninitialized;
        public FileInfo currentFile;
        
        public ReferenceTable ReferenceTable;
        #endregion
        
        #region Enums
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
        #endregion
        
        #region Constructors
        public PersistentWorldLoadSaver(PersistentWorld world, string worldFolderPath)
        {
            this.PersistentWorld = world;
            this.ReferenceTable = new ReferenceTable(this);
            
            this.ConfigurePaths(worldFolderPath);
        }
        #endregion
        
        #region Methods
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

        private void PreloadWorldColoniesMaps()
        {
            var files = new List<string> {this.worldFilePath};

            new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFile_Extension).Do(colonyFile => files.Add(colonyFile.FullName));
            new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFile_Extension).Do(mapFile => files.Add(mapFile.FullName));
            
            ScribeMultiLoader.InitLoading(files.ToArray());
            
            Log.Message("Preloaded World, Colonies, and Maps.");
        }

        private void SetCurrentFile(string filePath)
        {
            this.currentFile = new FileInfo(filePath);
        }

        private void SetCurrentFile(FileInfo fileInfo)
        {
            this.currentFile = fileInfo;
        }
        
        public void LoadWorld()
        {
            this.ReferenceTable.ClearReferences();

            if (ScribeMultiLoader.Empty())
            {
                PreloadWorldColoniesMaps();
            }

            Status = PersistentWorldLoadStatus.Loading;
            
            this.SetCurrentFile(worldFilePath);
            
            Log.Message("Loading world " + worldFolderPath);
            
            // Select world to load XML node data for.
            ScribeMultiLoader.SetScribeCurXmlParentByFilePath(this.worldFilePath);

            // Required otherwise errors because of internal requirements.
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);

            // Load data.
            Scribe_Deep.Look<PersistentWorldData>(ref this.PersistentWorld.WorldData, "data");

            //this.PersistentWorld.LoadColonies();
            this.PersistentWorld.ResetPlayerFaction();
            
            Log.Message("Loaded world data...");
        }

        /// <summary>
        /// Loads all colony data for a specific colony. Fully loads the referenced colony.
        /// </summary>
        /// <param name="colony"></param>
        public void LoadColony(ref PersistentColony colony)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars || ScribeMultiLoader.Empty())
            {
                this.PreloadWorldColoniesMaps();
            }
            
            var file = colony.FileInfo;

            if (file == null)
            {
                throw new NullReferenceException("LoadColony(PersistentColony&): Colony.FileInfo is null.");
            }
            
            SetCurrentFile(file);
            
            Log.Message("Loading colony... " + Path.GetFileNameWithoutExtension(file.FullName));
            
            ScribeMultiLoader.SetScribeCurXmlParentByFilePath(file.FullName);

            Scribe_Deep.Look(ref colony, "colony");

            this.PersistentWorld.Colony = colony;
            colony.FileInfo = file;
            
            Log.Message("Loaded colony.");
        }

        /// <summary>
        /// Loads some colony information for loading screens.
        /// </summary>
        public void LoadColonies()
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars || ScribeMultiLoader.Empty())
            {
                this.PreloadWorldColoniesMaps();
            }
            
            var colonyFiles = new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFile_Extension);

            Log.Message("Loading colonies...");
            
            foreach (var colonyFile in colonyFiles)
            {
                this.SetCurrentFile(colonyFile);
                
                ScribeMultiLoader.SetScribeCurXmlParentByFilePath(colonyFile.FullName);

                var colony = new PersistentColony(){FileInfo = colonyFile};
                
                if (Scribe.EnterNode("colony"))
                {
                    Scribe_Deep.Look(ref colony.ColonyData, "data");
                    
                    Scribe.ExitNode();
                }

                this.PersistentWorld.Colonies.Add(colony);
            }
            
            Log.Message("Loaded colony data...");
        }
        // TODO: Store basic colony information in world file.

        public IEnumerable<Map> LoadMaps(int[] mapTiles)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars || ScribeMultiLoader.Empty())
            {
                this.PreloadWorldColoniesMaps();
            }
            
            var mapFiles = new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFile_Extension);
            
            Log.Message("Loading maps...");

            var maps = new List<Map>();

            foreach (var mapFile in mapFiles)
            {
                this.SetCurrentFile(mapFile);
                
                if (!mapTiles.Any(tile => mapFile.FullName.Contains(tile.ToString()))) continue;
                
                if(this.Status != PersistentWorldLoadStatus.Ingame)
                    ScribeMultiLoader.SetScribeCurXmlParentByFilePath(mapFile.FullName);
                else
                {
                    // Reset scribe if not already reset.
                    ScribeVars.TrickScribe();
                    
                    Scribe.loader.InitLoading(mapFile.FullName);
                }

                var map = new Map();

                Scribe_Deep.Look<Map>(ref map, "map");

                if (this.Status == PersistentWorldLoadStatus.Ingame)
                {
                    Scribe.loader.FinalizeLoading();
                }

                maps.Add(map);
            }

            if (this.Status != PersistentWorldLoadStatus.Ingame)
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

        public void SaveWorld()
        {
            // Clear references
            this.ReferenceTable.ClearReferences();
            
            this.CreateDirectoriesIfNotExistant();

            Status = PersistentWorldLoadStatus.Saving;
            
            this.PersistentWorld.ConvertCurrentGameSettlements();

            this.SaveWorldData();
            //this.SaveColonyData();
            this.SaveColoniesData();
            this.SaveMapData();
            
            this.PersistentWorld.ConvertToCurrentGameSettlements();
        }

        private void SaveWorldData()
        {
            Log.Message("Saving world data...");
            
            this.PersistentWorld.WorldData = PersistentWorldData.Convert(Current.Game);

            //this.PersistentWorld.SaveColonies();
            
            this.SetCurrentFile(new FileInfo(worldFilePath));
            
            SafeSaver.Save(this.worldFilePath, "worldfile", delegate
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                Scribe_Deep.Look<PersistentWorldData>(ref this.PersistentWorld.WorldData, "data");
            });
            
            Log.Message("Saved world data.");
        }

        private void SaveColoniesData()
        {
            Log.Message("Saving colonies data...");

            for (var i = 0; i < this.PersistentWorld.Colonies.Count; i++)
            {
                var colony = this.PersistentWorld.Colonies[i];

                var oldColonySaveFile = coloniesDirectory + "/" + colony.ColonyData.ColonyFaction.Name +
                                        PersistentWorldColonyFile_Extension;
                
                if (Equals(this.PersistentWorld.Colony, colony))
                {
                    colony = PersistentColony.Convert(this.PersistentWorld.Game, colony.ColonyData);
                }
                
                var colonySaveFile = coloniesDirectory + "/" + colony.ColonyData.ColonyFaction.Name +
                                     PersistentWorldColonyFile_Extension;
                
                // TODO: Delete old save file.
                if (oldColonySaveFile != colonySaveFile)
                {
                    File.Delete(oldColonySaveFile);
                }
                
                var colonyFile = new FileInfo(colonySaveFile);
                currentFile = colonyFile;

                if (!Equals(this.PersistentWorld.Colony, colony)) continue;
                
                SafeSaver.Save(colonySaveFile, "colonyfile", delegate { Scribe_Deep.Look(ref colony, "colony"); });
            }
            
            Log.Message("Saved colonies data.");
        }

        private void SaveColonyData()
        {
            Log.Message("Saving colony data...");

            var colony = this.PersistentWorld.Colony;
            colony.ColonyData = PersistentColonyData.Convert(Current.Game, colony.ColonyData);

            var colonySaveFile = coloniesDirectory + "/" + colony.ColonyData.ColonyFaction.Name +
                                 PersistentWorldColonyFile_Extension;
            
            var colonyFile = new FileInfo(colonySaveFile);
            
            this.SetCurrentFile(colonyFile);
            colony.FileInfo = colonyFile;
            
            SafeSaver.Save(colonySaveFile, "colonyfile", delegate
            {
                Scribe_Deep.Look(ref colony, "colony");
            });
            
            Log.Message("Saved colony data.");
        }

        private void SaveMapData()
        {
            Log.Message("Saving map data...");
            
            var maps = Current.Game.Maps;

            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                
                var mapSaveFile = mapsDirectory + "/" + map.Tile + PersistentWorldMapFile_Extension;
                this.SetCurrentFile(mapSaveFile);
                
                SafeSaver.Save(mapSaveFile, "mapfile", delegate { Scribe_Deep.Look(ref map, "map"); });
            }
            
            Log.Message("Saved map data.");
        }
        
        /**
         * CONVERT
         */
        
        public void Convert(Game game)
        {
            this.ConfigurePaths(SaveDir + "/" + game.World.info.name);
            this.CreateDirectoriesIfNotExistant();
            
            PersistentWorldManager.GetInstance().PersistentWorld.Convert(game);
            
            this.SaveWorld();
            
            GenScene.GoToMainMenu();

            this.Status = PersistentWorldLoadStatus.Uninitialized;
        }
        
        /**
         * MISC
         */
        
        public void TransferToPlayScene()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Status = PersistentWorldLoadStatus.Finalizing;

                Current.Game = new Game {InitData = new GameInitData {gameToLoad = "PersistentWorld"}}; // Just to get the SavedGameLoaderNow.LoadGameFromSaveFileNow() patch to load.
            }, "Play", "LoadingLongEvent", true, null);
        }

        public string GetWorldFolder()
        {
            return this.worldFolderPath;
        }
        #endregion
    }
}