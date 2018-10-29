using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    internal static class PersistentWorldMenuUI
    {
        private static readonly Texture2D OpenFolder = ContentFinder<Texture2D>.Get("UI/OpenFolder");

        public static void DrawWorldList(ref Rect inRect, List<PersistentWorldUIEntry> worldEntries, List<SaveGameUIEntry> saveGameEntries)
        {
            
        }

        internal class PersistentWorldUIEntry
        {
            private string path;
            private string name;

            public PersistentWorldUIEntry(string path, string name)
            {
                this.path = path;
                this.name = name;
            }

            public PersistentWorldUIEntry(DirectoryInfo directoryInfo) : this(directoryInfo.FullName, directoryInfo.Name)
            {
            }

            public PersistentWorldUIEntry(string directory) : this(new DirectoryInfo(directory))
            {
            }
        }

        internal class SaveGameUIEntry
        {
            private string path;

            public SaveGameUIEntry(string path)
            {
                this.path = path;
            }
        }
    }
}