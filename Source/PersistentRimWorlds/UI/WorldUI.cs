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
    internal static class WorldUI
    {
        #region Fields
        private static readonly Texture2D OpenFolder = ContentFinder<Texture2D>.Get("UI/OpenFolder");
        private static readonly Texture2D ConvertFile = ContentFinder<Texture2D>.Get("UI/ConvertFile");
        
        private static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");

        private static readonly Dictionary<UIEntry, Vector2> ScrollPositions = new Dictionary<UIEntry, Vector2>();
        
        private static Vector2 scrollPosition;
        #endregion
            
        #region Methods
        public static void DrawWorldList(ref Rect inRect, float margin, List<UIEntry> worldEntries, List<UIEntry> saveGameEntries, Action<string> loadWorld, Action<string> deleteWorld, Action<string> convertWorld)
        {
            const int perRow = 3;
            var gap = (int) margin;
            
            UITools.DrawBoxGridView(out _, out _, ref inRect, ref scrollPosition, perRow, gap,
                (i, boxRect) =>
                {
                    var selectedList = i >= worldEntries.Count ? saveGameEntries : worldEntries;
                    i = i >= worldEntries.Count ? i - worldEntries.Count : i;

                    var isWorldEntries = selectedList == worldEntries;
                    var isSaveGameEntries = selectedList == saveGameEntries;

                    var currentItem = selectedList[i];
                    
                    Widgets.DrawAltRect(boxRect);

                    if (isWorldEntries)
                    {
                        var deleteSize = boxRect.width / 8;

                        var deleteRect = new Rect(boxRect.x + boxRect.width - deleteSize, boxRect.y, deleteSize,
                            deleteSize);

                        if (Widgets.ButtonImage(deleteRect, DeleteX))
                        {
                            deleteWorld(currentItem.Path);
                        }

                        TooltipHandler.TipRegion(deleteRect,
                            "FilUnderscore.PersistentRimWorlds.DeleteWorld".Translate());

                        DrawTexture(boxRect, OpenFolder, out var textureRect, 0.3f, 0.2f);
                        
                        const float nameMargin = 4f;

                        var worldNameRect = new Rect(boxRect.x + nameMargin, boxRect.y + nameMargin,
                            boxRect.width - nameMargin - deleteSize, textureRect.y - boxRect.y);

                        DrawLabel(worldNameRect, ((WorldUIEntry) currentItem).Name, currentItem);
                    }
                    else if(isSaveGameEntries)
                    {
                        DrawTexture(boxRect, ConvertFile, out var textureRect, 0.48f, 0.42f);

                        const float nameMargin = 4f;

                        var saveNameRect = new Rect(boxRect.x + nameMargin, boxRect.y + nameMargin,
                            boxRect.width - nameMargin, textureRect.y - boxRect.y);

                        DrawLabel(saveNameRect, Path.GetFileNameWithoutExtension(((SaveGameUIEntry) currentItem).Path),
                            currentItem);
                    }

                    Widgets.DrawHighlightIfMouseover(boxRect);

                    if (!Widgets.ButtonInvisible(boxRect)) return true;

                    if (isWorldEntries)
                    {
                        loadWorld(selectedList[i].Path);
                    }
                    else if(isSaveGameEntries)
                    {
                        convertWorld(selectedList[i].Path);
                    }

                    return true;
                }, worldEntries.Count + saveGameEntries.Count);
        }

        public static void DrawWorldSaveList(ref Rect inRect, float margin, List<UIEntry> worldEntries,
            Action<string> overwriteWorld, Action newWorld, Action<string> deleteWorld)
        {
            
        }

        public static void Reset()
        {
            scrollPosition = new Vector2();
            ScrollPositions.Clear();
        }

        private static void DrawLabel(Rect rect, string label, UIEntry item)
        {
            if (!ScrollPositions.ContainsKey(item))
            {
                ScrollPositions.Add(item, new Vector2());
            }

            var itemScrollPosition = ScrollPositions[item];

            Text.Font = GameFont.Small;
            
            WidgetExtensions.LabelScrollable(rect, label, ref itemScrollPosition, false, true, false);

            Text.Font = GameFont.Small;

            ScrollPositions[item] = scrollPosition;
        }

        private static void DrawTexture(Rect rect, Texture texture, out Rect textureRect, float widthMultiplier, float heightMultiplier)
        {
            var sizeWidth = Mathf.Clamp(rect.width * widthMultiplier, 0, texture.width);
            var sizeHeight = Mathf.Clamp(rect.height * heightMultiplier, 0, texture.height);

            textureRect = new Rect(rect.x + rect.width / 2 - sizeWidth / 2,
                rect.y + rect.height / 2 - sizeHeight / 2, sizeWidth, sizeHeight);
                        
            GUI.DrawTexture(textureRect, texture);
        }
        #endregion

        #region Classes
        internal abstract class UIEntry
        {
            public string Path { get; }

            protected UIEntry(string path)
            {
                this.Path = path;
            }
        }

        internal class WorldUIEntry : UIEntry
        {
            public string Name { get; }

            private WorldUIEntry(string path, string name) : base(path)
            {
                this.Name = name;
            }

            public WorldUIEntry(DirectoryInfo directoryInfo) : this(directoryInfo.FullName, directoryInfo.Name)
            {
            }

            public WorldUIEntry(string directory) : this(new DirectoryInfo(directory))
            {
            }
        }

        internal class SaveGameUIEntry : UIEntry
        {
            public SaveGameUIEntry(string path) : base(path)
            {
            }
        }
        #endregion
    }
}