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
            foreach (var dir in Directory.GetDirectories(path))
            {
                DeleteDirectory(path);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception)
            {
                DeleteDirectory(path);
            }
        }
        #endregion
    }
}