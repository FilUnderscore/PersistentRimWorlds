using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using PersistentWorlds.Logic;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using Verse;
using PersistentWorlds.Patches;
using PersistentWorlds.World;
using Verse.Profile;

namespace PersistentWorlds.UI
{
    public class Dialog_PersistentWorlds_LoadWorld_FileList : Dialog_SaveFileList_Load
    {
        private List<string> worlds = new List<string>();
        
        public Dialog_PersistentWorlds_LoadWorld_FileList()
        {
            
        }

        protected override void DoFileInteraction(string saveFileName)
        {
            if (!worlds.Contains(saveFileName))
            {
                Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(
                    "ConfirmConversion-PersistentWorlds".Translate(),
                    () =>
                    {
                        Log.Message(saveFileName);
                        var path = GenFilePaths.FilePathForSavedGame(saveFileName);

                        PersistentWorldConverter.PrepareConvert(path);
                        Log.Message(path);
                        GameDataSaveLoader.LoadGame(saveFileName);
                    }, true));
            }
            else
            {
                Find.WindowStack.Add(new Dialog_PersistentWorlds_LoadWorld_ColonySelection(saveFileName));
            }
        }

        protected override void ReloadFiles()
        {
            base.ReloadFiles();
            
            foreach (string worldDir in Directory.GetDirectories(SaveUtils.SaveDir))
            {
                var worldDirInfo = new DirectoryInfo(worldDir);   
                var worldFile = worldDir + "/" + worldDirInfo.Name + ".pwf";
                
                this.files.Add(new SaveFileInfo(new FileInfo(worldFile)));
                
                worlds.Add(worldDirInfo.Name);
            }
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            Vector2 vector2_1 = new Vector2(inRect.width - 16f, 36f);
            Vector2 vector2_2 = new Vector2(100f, vector2_1.y - 2f);
          
            inRect.height -= 45f;
            
            float height = (float) this.files.Count * (vector2_1.y + 3f);
            
            Rect viewRect = new Rect(0.0f, 0.0f, inRect.width - 16f, height);
            Rect outRect = new Rect(inRect.AtZero());
            outRect.height -= this.bottomAreaHeight;
            
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            
            float y = 0.0f;
            int num = 0;
            
            foreach (SaveFileInfo file in this.files)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(file.FileInfo.Name);
                
                if ((double) y + (double) vector2_1.y >= (double) this.scrollPosition.y && (double) y <= (double) this.scrollPosition.y + (double) outRect.height)
                {
                    Rect rect1 = new Rect(0.0f, y, vector2_1.x, vector2_1.y);
                    
                    if (num % 2 == 0)
                      Widgets.DrawAltRect(rect1);
                    
                    Rect position = rect1.ContractedBy(1f);
                    
                    GUI.BeginGroup(position);
                    
                    GUI.color = this.FileNameColor(file);
                    
                    Rect rect2 = new Rect(15f, 0.0f, (float) byte.MaxValue, position.height);
                    
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Text.Font = GameFont.Small;
                    
                    Widgets.Label(rect2, fileNameNoExt);
                    
                    GUI.color = Color.white;
                    
                    Rect rect3 = new Rect(270f, 0.0f, 200f, position.height);
                    
                    DrawDateAndVersion(file, rect3);
                    
                    GUI.color = Color.white;
                    
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    
                    float x = vector2_1.x - 2f - vector2_2.x - vector2_2.y;
                    
                    if (Widgets.ButtonText(new Rect(x, 0.0f, vector2_2.x, vector2_2.y), (worlds.Contains(fileNameNoExt) ? this.interactButLabel : "Convert".Translate()), true, false, true))
                      this.DoFileInteraction(fileNameNoExt);
                    
                    Rect rect4 = new Rect((float) ((double) x + (double) vector2_2.x + 5.0), 0.0f, vector2_2.y, vector2_2.y);
          
                    Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);          
                    if (Widgets.ButtonImage(rect4, DeleteX, Color.white, GenUI.SubtleMouseoverColor))
                    {
                        FileInfo localFile = file.FileInfo;
                        
                        Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate((object) localFile.Name), (Action) (() =>
                        {
                            localFile.Delete();
                            this.ReloadFiles();
                        }), true, (string) null));
                    }
                    
                    TooltipHandler.TipRegion(rect4, (TipSignal) "DeleteThisSavegame".Translate());
                    
                    GUI.EndGroup();
                }
                
                y += vector2_1.y + 3f;
                ++num;
            }
            
            Widgets.EndScrollView();
            
            if (!this.ShouldDoTypeInField)
              return;
            
            this.DoTypeInField(inRect.AtZero());
        }

        private new void DrawDateAndVersion(SaveFileInfo sfi, Rect rect)
        {
            GUI.BeginGroup(rect);
              
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
              
            Rect rect1 = new Rect(0.0f, 2f, rect.width, rect.height / 2f);
              
            GUI.color = SaveFileInfo.UnimportantTextColor;
              
            Widgets.Label(rect1, sfi.FileInfo.LastWriteTime.ToString("g"));
              
            Rect rect2 = new Rect(0.0f, rect1.yMax, rect.width, rect.height / 2f);
              
            GUI.color = sfi.VersionColor;

            var fileNameNoExt = Path.GetFileNameWithoutExtension(sfi.FileInfo.Name);
            
            var gameVersionLabel = sfi.GameVersion + (worlds.Contains(fileNameNoExt) ? " [PW]" : "");
            
            Widgets.Label(rect2, gameVersionLabel);
              
            TooltipHandler.TipRegion(rect2, sfi.CompatibilityTip);
              
            GUI.EndGroup();
        }
    }
}