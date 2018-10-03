using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds
{
    public class SaveUtils
    {
        public static readonly string SaveDir =
            (string) AccessTools.Method(typeof(GenFilePaths), "get_SavedGamesFolderPath", new Type[0]).Invoke(null, new object[0]);
        
        public static void Save(string filePath, string rootElementName, Action saveAction)
        {
            SafeSaver.Save(filePath, rootElementName, saveAction);
        }

        public static void Load(string filePath, string rootElementName, Action loadAction)
        {
            Scribe.loader.InitLoading(filePath);
            
            //Scribe.EnterNode(rootElementName);
            // TODO: Print current element...
            loadAction();
            
            Scribe.loader.FinalizeLoading();
        }

        public static void SaveWorld(PersistentWorld world)
        {
            var fileName = world.Game.World.info.name;
            
            var worldDir = Directory.CreateDirectory(SaveDir + "/" + fileName);
            var coloniesDir = Directory.CreateDirectory(worldDir.FullName + "/" + "Colonies");

            var mapsDir = Directory.CreateDirectory(worldDir.FullName + "/" + "Maps");
            var worldFilePath = worldDir.FullName + "/" + fileName + ".pwf";
            
            SafeSaver.Save(worldFilePath, "world", delegate
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                world.WorldData.ExposeData();
            });

            foreach (var colony in world.Colonies)
            {
                var colonySaveFile = coloniesDir.FullName + "/" + colony.ToString() + ".pwcf";
                
                SafeSaver.Save(colonySaveFile, "colony", delegate
                {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    colony.ColonyData.ExposeData();
                });
            }

            foreach (var map in world.Maps)
            {
                var mapSaveFile = mapsDir.FullName + "/" + map.Index.ToString() + ".pwmf";
                
                SafeSaver.Save(mapSaveFile, "map", delegate
                {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    var target = map;
                    Scribe_Deep.Look<Map>(ref target, "map", new object[0]);
                });
            }
        }
        
        public static List<PersistentColony> LoadColonies(string fileName)
        {
            var worldDirectory = Directory.CreateDirectory(SaveDir + "/" + fileName);
            var coloniesDirectory = Directory.CreateDirectory(worldDirectory.FullName + "/" + "Colonies");
            
            var persistentColonies = new List<PersistentColony>();
            
            foreach (var colonyFile in coloniesDirectory.GetFiles("*.pwcf"))
            {
                Scribe.loader.InitLoading(colonyFile.FullName);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
                persistentColonies.Add(LoadColonyData());
                Scribe.loader.FinalizeLoading();
            }

            return persistentColonies;
        }
        
        private static PersistentColony LoadColonyData()
        {
            PersistentColony persistentColony = new PersistentColony();
            
            Log.Warning("Calling ExposeData on PersistentColonyData soon...");
            persistentColony.ColonyData = new PersistentColonyData();
            persistentColony.ColonyData.ExposeData();

            return persistentColony;
        }

        public static void ConvertAndSaveWorld(Game game)
        {
            var persistentGame = PersistentWorld.Convert(game);
            PersistentWorldManager.PersistentWorld = persistentGame;
            Log.Message("Converted GAME WOWOADOAJIOD");
            SaveWorld(persistentGame);
            Log.Message("Saved game. Returning.");
            GenScene.GoToMainMenu();
        }
    }
}