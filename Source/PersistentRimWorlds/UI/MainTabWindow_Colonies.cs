using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.World;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public sealed class MainTabWindow_Colonies : MainTabWindow
    {
        #region Fields
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");

        private Vector2 scrollPosition = Vector2.zero;
        private List<ScrollableListItem> items;
        #endregion
        
        #region Properties
        public override Vector2 RequestedTabSize => new Vector2(Verse.UI.screenWidth * 0.5f, Verse.UI.screenHeight / 3.5f);
        #endregion
        
        #region Methods
        public override void DoWindowContents(Rect inRect)
        {   
            ScrollableListUI.DrawList(ref inRect, ref scrollPosition, ref this.items);
        }

        public override void PreOpen()
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
            {
                this.Close();
                return;
            }
            
            base.PreOpen();
            
            this.ConvertColoniesToItems();
        }

        private void ConvertColoniesToItems()
        {
            this.items = new List<ScrollableListItem>();

            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            
            for (var i = 0; i < persistentWorld.Colonies.Count; i++)
            {
                var colony = persistentWorld.Colonies[i];

                var item = new ScrollableListItemColored {Text = colony.ColonyData.ColonyFaction.Name, 
                    canChangeColor = false, canSeeColor = true, Color = colony.ColonyData.color, texture = Town};

                if (!Equals(colony, persistentWorld.Colony))
                {
                    item.canChangeColor = true;
                    
                    var index = i;
                    
                    item.ActionButtonText = "FilUnderscore.PersistentRimWorlds.SwitchToColony".Translate();
                    item.ActionButtonAction = delegate
                    {                         
                        this.Close();

                        this.Load(index, colony, persistentWorld);
                    };
                }
                
                item.Info.Add(new ScrollableListItemInfo
                {
                    Text = "Colony ID: " + colony.ColonyData.uniqueID,
                    color = SaveFileInfo.UnimportantTextColor
                });
                
                item.Info.Add(new ScrollableListItemInfo
                {
                    Text = colony.FileInfo.LastWriteTime.ToString("g"),
                    color = SaveFileInfo.UnimportantTextColor
                });
                
                this.items.Add(item);
            }
        }

        private void Load(int index, PersistentColony colony, PersistentWorld persistentWorld)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                persistentWorld.ConvertCurrentGameSettlements();

                var previousColony = persistentWorld.Colony;
                persistentWorld.SaveColony(previousColony);
                            
                persistentWorld.LoadSaver.LoadColony(ref colony);
                persistentWorld.Colonies[index] = colony;
                            
                persistentWorld.PatchPlayerFaction();
                            
                // TODO: Figure out how to load asynchronously to not lock up game.
                var maps = DynamicMapLoader.LoadColonyMaps(colony);
                Current.Game.CurrentMap = Current.Game.FindMap(maps.First().Tile);

                persistentWorld.UnloadColony(previousColony);

                persistentWorld.ConvertToCurrentGameSettlements();
                            
                Find.CameraDriver.SetRootPosAndSize(colony.GameData.camRootPos, colony.GameData.desiredSize);   
            }, "FilUnderscore.PersistentRimWorlds.LoadingColonyAndMaps", false, null);
        }
        #endregion
    }
}