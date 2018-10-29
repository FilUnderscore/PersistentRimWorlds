using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    internal static class PersistentWorldMenuUI
    {
        private static readonly Texture2D OpenFolder = ContentFinder<Texture2D>.Get("UI/OpenFolder");
        private static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");

        private static Vector2 scrollPosition;
        
        public static void DrawWorldList(ref Rect inRect, float margin, List<UIEntry> worldEntries, List<UIEntry> saveGameEntries, Action<string> loadWorld, Action<string> deleteWorld, Action<string> convertWorld)
        {
            const int perRow = 3;
            var gap = (int) margin;
            
            UITools.DrawBoxGridView(out var viewRect, out var outRect, ref inRect, ref scrollPosition, perRow, gap,
                (i, boxRect) =>
                {
                    var selectedList = i >= worldEntries.Count ? saveGameEntries : worldEntries;
                    i = i >= worldEntries.Count ? i - worldEntries.Count : i;
                    
                    Widgets.DrawAltRect(boxRect);

                    if (selectedList == worldEntries)
                    {
                        var deleteSize = boxRect.width / 8;

                        var deleteRect = new Rect(boxRect.x + boxRect.width - deleteSize, boxRect.y, deleteSize,
                            deleteSize);

                        if (Widgets.ButtonImage(deleteRect, DeleteX))
                        {
                            deleteWorld(selectedList[i].Path);
                        }
                        
                        TooltipHandler.TipRegion(deleteRect, "FilUnderscore.PersistentRimWorlds.DeleteWorld".Translate());
                    }

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

                    var size = boxRect.width * 0.65f;

                    if (selectedList == worldEntries)
                    {
                        if (size >= OpenFolder.width)
                            size = OpenFolder.width;

                        GUI.DrawTexture(boxRect, OpenFolder);
                    }

                    return true;
                }, worldEntries.Count + saveGameEntries.Count, null);
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

            public string Name => name;

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