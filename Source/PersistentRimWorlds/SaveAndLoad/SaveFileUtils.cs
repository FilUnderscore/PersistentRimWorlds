using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PersistentWorlds.UI;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public static class SaveFileUtils
    {
        #region Methods
        public static bool HasPossibleSameWorldName(string[] names, string filePath)
        {
            var worldName = "";
            
            Scribe.loader.InitLoading(filePath);
            
            if (Scribe.EnterNode("game"))
            {
                if (Scribe.EnterNode("world"))
                {
                    if (Scribe.EnterNode("info"))
                    {
                        Scribe_Values.Look<string>(ref worldName, "name");
                    }
                }
            }
            
            Scribe.loader.ForceStop();
            
            return names.Any(name => worldName.EqualsIgnoreCase(name));
        }

        public static bool AnyWorlds()
        {
            var savePath = PersistentWorldLoadSaver.SaveDir;

            if (!Directory.Exists(savePath))
            {
                return false;
            }

            var info = new DirectoryInfo(savePath);

            return ((info.GetDirectories().Length > 0 || GenFilePaths.AllSavedGameFiles.Any()) && Current.ProgramState == ProgramState.Entry &&
                   GenScene.InEntryScene);

        }

        internal static IEnumerable<WorldUI.WorldUIEntry> LoadWorldEntries()
        {
            return Directory.GetDirectories(PersistentWorldLoadSaver.SaveDir)
                .Select(worldDir => new WorldUI.WorldUIEntry(new DirectoryInfo(worldDir)));
        }

        public static void DeleteDirectory(string path)
        {
            var dirInfo = new DirectoryInfo(path);

            foreach (var file in dirInfo.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
            
            foreach (var dir in Directory.GetDirectories(path))
            {
                DeleteDirectory(dir);
            }
            
            dirInfo.Delete(true);
        }

        public static void DeleteFile(string path)
        {
            var fileInfo = new FileInfo(path);
            
            fileInfo.Delete();
        }

        public static bool WorldWithNameExists(string name)
        {
            return LoadWorldEntries().Any(entry => entry.Name.EqualsIgnoreCase(name));
        }

        public static DirectoryInfo Clone(string folderPath, string newFolderPath)
        {
            var rootDirectory = Directory.CreateDirectory(newFolderPath);
            
            foreach (var dir in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(folderPath, newFolderPath));
            }

            foreach (var file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(folderPath, newFolderPath), true);
            }

            return rootDirectory;
        }
        #endregion
    }
}