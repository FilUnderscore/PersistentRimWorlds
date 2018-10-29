using System;
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

        private static Vector2 scrollPosition;
        
        public static void DrawWorldList(ref Rect inRect, float margin, List<UIEntry> worldEntries, List<UIEntry> saveGameEntries, Action<string> loadWorld, Action<string> convertWorld)
        {
            const int perRow = 3;
            var gap = (int) margin;
            
            UITools.DrawBoxGridView(out var viewRect, out var outRect, ref inRect, ref scrollPosition, perRow, gap,
                (i, boxRect) =>
                {
                    var selectedList = i >= worldEntries.Count ? saveGameEntries : worldEntries;
                    i = i >= worldEntries.Count ? i - worldEntries.Count : i;
                    
                    Widgets.DrawAltRect(boxRect);
                    Widgets.DrawHighlightIfMouseover(boxRect);

                    if (Widgets.ButtonInvisible(boxRect))
                    {
                        if (selectedList == worldEntries)
                        {
                            loadWorld(selectedList[i].Path);
                        }
                        else if(selectedList == saveGameEntries)
                        {
                            convertWorld(selectedList[i].Path);
                        }
                    }
                    
                    GUI.DrawTexture(boxRect, OpenFolder);
                }, worldEntries.Count + saveGameEntries.Count, null, 3);
        }

        public static void Reset()
        {
            scrollPosition = new Vector2();
        }

        internal abstract class UIEntry
        {
            private string path;

            public string Path => path;
            
            protected UIEntry(string path)
            {
                this.path = path;
            }
        }

        internal class PersistentWorldUIEntry : UIEntry
        {
            private string name;

            public PersistentWorldUIEntry(string path, string name) : base(path)
            {
                this.name = name;
            }

            public PersistentWorldUIEntry(DirectoryInfo directoryInfo) : this(directoryInfo.FullName, directoryInfo.Name)
            {
            }

            public PersistentWorldUIEntry(string directory) : this(new DirectoryInfo(directory))
            {
            }
        }

        internal class SaveGameUIEntry : UIEntry
        {
            public SaveGameUIEntry(string path) : base(path)
            {
            }
        }
    }
}