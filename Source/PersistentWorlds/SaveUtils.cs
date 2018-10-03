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

        public static void SaveGame(PersistentGame game)
        {
            var world = game.World;
            
            var worldDir = Directory.CreateDirectory(SaveDir + "/" + world.World.info.name);
            var coloniesDir = Directory.CreateDirectory(worldDir.FullName + "/" + "Colonies");

            var mapsDir = Directory.CreateDirectory(worldDir.FullName + "/" + "Maps");
            var worldFilePath = worldDir.FullName + "/" + world.World.info.name + ".pwf";
            
            SafeSaver.Save(worldFilePath, "world", delegate
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                game.ExposeData();
            });

            foreach (var colony in world.Colonies)
            {
                var colonySaveFile = coloniesDir.FullName + "/" + colony.Name + ".pwc";
                
                SafeSaver.Save(colonySaveFile, "colony", delegate
                {
                    colony.ExposeData();
                });
            }
        }

        public static void LoadGame(string worldName, PersistentGame game)
        {
            
        }

        public static List<PersistentColony> LoadColonies(string worldName)
        {
            var colonies = new List<PersistentColony>();
            
            var worldDir = Directory.CreateDirectory(SaveDir + "/" + worldName);
            var coloniesDir = Directory.CreateDirectory(worldDir.FullName + "/" + "Colonies");

            var mapsDir = Directory.CreateDirectory(worldDir.FullName + "/" + "Maps");
            var worldFilePath = worldDir.FullName + "/" + worldName + ".pwf";
            
            foreach (var file in coloniesDir.GetFiles("*.pwc"))
            {
                var colony = new PersistentColony();
                
                Load(file.FullName, "colony", colony.ExposeMetaData);
                
               colonies.Add(colony);
            }

            return colonies;
        }

        public static void ConvertAndSaveWorld(Game game)
        {
            var persistentGame = new PersistentGame(game);
            
            SaveGame(persistentGame);
        }
    }
}