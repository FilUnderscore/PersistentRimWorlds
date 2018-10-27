using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Harmony;
using PersistentWorlds.Logic;
using RimWorld;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public sealed class PersistentWorldLoadSaver
    {
        #region Fields
        public static readonly string SaveDir =
            (string) AccessTools.Method(typeof(GenFilePaths), "get_SavedGamesFolderPath", new Type[0]).Invoke(null, new object[0]);

        private readonly PersistentWorld persistentWorld;
        
        private const string PersistentWorldFileExtension = ".pwf";
        private const string PersistentWorldColonyFileExtension = ".pwcf";
        public const string PersistentWorldMapFileExtension = ".pwmf";

        private string worldFolderPath;
        private DirectoryInfo worldFolderDirectoryInfo;
        
        private string coloniesDirectory;
        private string mapsDirectory;
        
        private string worldFilePath;
        
        public PersistentWorldLoadStatus Status = PersistentWorldLoadStatus.Uninitialized;
        public FileInfo CurrentFile;
        
        public readonly ReferenceTable ReferenceTable;
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
            this.persistentWorld = world;
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
                                 PersistentWorldFileExtension;
        }

        private void CreateDirectoriesIfNotExistent()
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

            new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFileExtension).Do(colonyFile => files.Add(colonyFile.FullName));
            new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFileExtension).Do(mapFile => files.Add(mapFile.FullName));
            
            ScribeMultiLoader.InitLoading(files.ToArray());
            
            Log.Message("Preloaded World, Colonies, and Maps.");
        }

        private void SetCurrentFile(string filePath)
        {
            this.CurrentFile = new FileInfo(filePath);
        }

        private void SetCurrentFile(FileInfo fileInfo)
        {
            this.CurrentFile = fileInfo;
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
            Scribe_Deep.Look<PersistentWorldData>(ref this.persistentWorld.WorldData, "data");

            this.persistentWorld.ResetPlayerFaction(FactionDefOf.PlayerColony);
            
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
            
            var file = colony.FileInfo ?? new FileInfo(GetColonySaveFilePath(colony));

            SetCurrentFile(file);
            
            Log.Message("Loading colony... " + Path.GetFileNameWithoutExtension(file.FullName));
            
            ScribeMultiLoader.SetScribeCurXmlParentByFilePath(file.FullName);

            Scribe_Deep.Look(ref colony, "colony");

            this.persistentWorld.Colony = colony;
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
            
            var colonyFiles = new DirectoryInfo(this.coloniesDirectory).GetFiles("*" + PersistentWorldColonyFileExtension);

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

                this.persistentWorld.Colonies.Add(colony);
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
            
            var mapFiles = new DirectoryInfo(this.mapsDirectory).GetFiles("*" + PersistentWorldMapFileExtension);
            
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
            
            this.CreateDirectoriesIfNotExistent();

            Status = PersistentWorldLoadStatus.Saving;
            
            this.persistentWorld.ConvertCurrentGameSettlements();

            this.SaveWorldData();
            this.SaveColony(ref this.persistentWorld.Colony);
            this.SaveMapData();
            
            this.persistentWorld.ConvertToCurrentGameSettlements();
        }

        private void SaveWorldData()
        {
            Log.Message("Saving world data...");
            
            this.persistentWorld.WorldData = PersistentWorldData.Convert(Current.Game);

            //this.PersistentWorld.SaveColonies();
            
            this.SetCurrentFile(new FileInfo(worldFilePath));
            
            SafeSaver.Save(this.worldFilePath, "worldfile", delegate
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                Scribe_Deep.Look(ref this.persistentWorld.WorldData, "data");
            });
            
            Log.Message("Saved world data.");
        }

        private string GetColonySaveFilePath(PersistentColony colony)
        {
            return coloniesDirectory + "/" + colony.ColonyData.ColonyFaction.Name + "_" + colony.ColonyData.UniqueId +
                   PersistentWorldColonyFileExtension;
        }

        public void SaveColonyAndColonyMapsData(ref PersistentColony colony)
        {
            Log.Message("Saving colony and colony maps data...");
            
            SaveColony(ref colony);

            SaveColonyMapData(colony);
            
            Log.Message("Saved colony and colony maps data.");
        }

        private void SaveColony(ref PersistentColony colony)
        {
            Log.Message("Saving colony data...");

            var oldColonySaveFile = colony.FileInfo ?? new FileInfo(GetColonySaveFilePath(colony));
            
            colony = PersistentColony.Convert(this.persistentWorld.Game, colony.ColonyData);
            
            var colonySaveFile = GetColonySaveFilePath(colony);
            var colonyFile = new FileInfo(colonySaveFile);

            if (!oldColonySaveFile.FullName.EqualsIgnoreCase(colonyFile.FullName))
            {
                File.Delete(oldColonySaveFile.FullName);
            }
            
            this.SetCurrentFile(colonyFile);
            
            ReferenceTable.ClearReferencesFor(colonySaveFile, true);

            var @ref = colony;

            SafeSaver.Save(colonySaveFile, "colonyfile", delegate { Scribe_Deep.Look(ref @ref, "colony"); });
            colony.FileInfo = colonyFile;

            colony = @ref;

            Log.Message("Saved colony data.");
        }

        private void SaveColonyMapData(PersistentColony colony)
        {
            Log.Message("Saving colony map data...");

            var maps = Current.Game.Maps;
            
            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                
                var set = persistentWorld.LoadedMaps[map.Tile];

                if (!set.Contains(colony) || set.Count != 1) continue;

                var mapSaveFile = mapsDirectory + "/" + map.Tile + PersistentWorldMapFileExtension;
                this.SetCurrentFile(mapSaveFile);
                    
                ReferenceTable.ClearReferencesFor(mapSaveFile, true);
                
                SafeSaver.Save(mapSaveFile, "mapfile", delegate
                {
                    Scribe_Deep.Look(ref map, "map");
                });
            }
            
            Log.Message("Saved colony map data.");
        }

        private void SaveMapData()
        {
            Log.Message("Saving map data...");
            
            var maps = Current.Game.Maps;

            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                
                var mapSaveFile = mapsDirectory + "/" + map.Tile + PersistentWorldMapFileExtension;
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
            this.CreateDirectoriesIfNotExistent();
            
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
        #endregion
        
        #region Structs
        private struct ColonyNameInfo
        {
            public FileInfo OldFileInfo;
            public FileInfo NewFileInfo;
            
            public ColonyNameInfo(FileInfo oldFileInfo, FileInfo newFileInfo)
            {
                OldFileInfo = oldFileInfo;
                NewFileInfo = newFileInfo;
            }
        }
        #endregion
    }
}