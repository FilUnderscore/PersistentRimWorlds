using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    internal static class PersistentWorldMenuUI
    {
        private static readonly Texture2D OpenFolder = ContentFinder<Texture2D>.Get("UI/OpenFolder");
        private static readonly Texture2D ConvertFile = ContentFinder<Texture2D>.Get("UI/ConvertFile");
        
        private static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");

        private static readonly Dictionary<UIEntry, Vector2> ScrollPositions = new Dictionary<UIEntry, Vector2>();
        
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

                        var sizeWidth = Mathf.Clamp(boxRect.width * 0.3f, 0, OpenFolder.width);
                        var sizeHeight = Mathf.Clamp(boxRect.height * 0.2f, 0, OpenFolder.height);

                        var textureRect = new Rect(boxRect.x + boxRect.width / 2 - sizeWidth / 2, boxRect.y + boxRect.height / 2 - sizeHeight / 2, sizeWidth,
                            sizeHeight);

                        GUI.DrawTexture(textureRect, OpenFolder);
                        
                        const float nameMargin = 4f;
                        var worldNameRect = new Rect(boxRect.x + nameMargin, boxRect.y + nameMargin, boxRect.width - nameMargin - deleteSize, textureRect.y - boxRect.y);

                        if (!ScrollPositions.ContainsKey(selectedList[i]))
                        {
                            ScrollPositions.Add(selectedList[i], new Vector2());
                        }

                        var worldScrollPosition = ScrollPositions[selectedList[i]];
                        
                        Text.Font = GameFont.Small;
                        
                        WidgetExtensions.LabelScrollable(worldNameRect, ((PersistentWorldUIEntry)selectedList[i]).Name, ref worldScrollPosition, false, true, false);

                        Text.Font = GameFont.Small;

                        ScrollPositions[selectedList[i]] = worldScrollPosition;
                    }
                    else
                    {
                        var sizeWidth = Mathf.Clamp(boxRect.width * 0.3f, 0, ConvertFile.width);
                        var sizeHeight = Mathf.Clamp(boxRect.height * 0.25f, 0, ConvertFile.height);

                        var textureRect = new Rect(boxRect.x + boxRect.width / 2 - sizeWidth / 2,
                            boxRect.y + boxRect.height / 2 - sizeHeight / 2, sizeWidth, sizeHeight);
                        
                        GUI.DrawTexture(textureRect, ConvertFile);
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

                    return true;
                }, worldEntries.Count + saveGameEntries.Count, null);
        }

        public static void Reset()
        {
            scrollPosition = new Vector2();
            ScrollPositions.Clear();
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