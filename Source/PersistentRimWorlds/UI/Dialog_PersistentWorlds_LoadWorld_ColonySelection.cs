using System.Collections.Generic;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public sealed class Dialog_PersistentWorlds_LoadWorld_ColonySelection : Window
    {        
        #region Fields
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");

        private readonly PersistentWorld persistentWorld;
        
        private List<ScrollableListItem> items = new List<ScrollableListItem>();

        private Vector2 scrollPosition = Vector2.zero;
        #endregion
        
        #region Properties
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        #endregion
        
        #region Constructors
        public Dialog_PersistentWorlds_LoadWorld_ColonySelection(PersistentWorld persistentWorld)
        {
            this.persistentWorld = persistentWorld;
            
            this.LoadColoniesAsItems();

            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }
        #endregion
        
        #region Methods
        public override void PreOpen()
        {
            this.SetInitialSizeAndPosition();
        }

        public override void PostClose()
        {
            
        }

        private void LoadColoniesAsItems()
        {
            this.persistentWorld.LoadSaver.LoadColonies();
            
            for (var i = 0; i < this.persistentWorld.Colonies.Count; i++)
            {
                var colony = this.persistentWorld.Colonies[i];

                var i1 = i;
                var scrollableListItem = new ScrollableListItemColored
                {
                    Text = colony.ColonyData.ColonyFaction.Name,
                    ActionButtonText = "Load".Translate(),
                    ActionButtonAction = delegate
                    {
                        PersistentWorldManager.GetInstance().PersistentWorld = this.persistentWorld;
                        
                        this.persistentWorld.LoadSaver.LoadColony(ref colony);
                        this.persistentWorld.Colonies[i1] = colony;

                        // This line cause UIRoot_Play to throw one error due to null world/maps, can be patched to check if null before running.
                        MemoryUtility.ClearAllMapsAndWorld();

                        this.persistentWorld.PatchPlayerFaction();
                        this.persistentWorld.LoadSaver.TransferToPlayScene();
                    },
                    DeleteButtonAction = delegate
                    {
                        // TODO: Allow colonies to be deleted.   
                    },
                    DeleteButtonTooltip = "DeleteColony-PersistentWorlds".Translate(),
                    canSeeColor = true,
                    canChangeColor = true,
                    Color = colony.ColonyData.color,
                    texture = Town
                };

                scrollableListItem.Info.Add(new ScrollableListItemInfo
                {
                    Text = colony.FileInfo.LastWriteTime.ToString("g"),
                    color = SaveFileInfo.UnimportantTextColor
                });
                
                items.Add(scrollableListItem);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            var rect1 = new Rect((inRect.width - 170f) / 2, 0.0f, 170f, inRect.height);

            var optList = new List<ListableOption>
            {
                new ListableOption("NewColony".Translate(),
                    delegate
                    {
                        PersistentWorldManager.GetInstance().PersistentWorld = this.persistentWorld;

                        Find.WindowStack.Add((Window) new Page_SelectScenario());
                    })
            };

            var num1 = (double) OptionListingUtility.DrawOptionListing(rect1, optList);
            
            var rect2 = new Rect(0, (float) num1, inRect.width, inRect.height);
            
            GUI.BeginGroup(rect2);
            ScrollableListUI.DrawList(ref rect2, ref this.scrollPosition, ref this.items);
            GUI.EndGroup();
            
            GUI.EndGroup();
        }
        #endregion
    }
}