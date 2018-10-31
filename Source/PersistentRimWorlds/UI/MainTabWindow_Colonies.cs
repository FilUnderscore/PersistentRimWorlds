using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public sealed class MainTabWindow_Colonies : MainTabWindow
    {
        #region Fields
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");
        #endregion
        
        #region Properties
        public override Vector2 RequestedTabSize => new Vector2(Verse.UI.screenWidth * 0.5f, Verse.UI.screenHeight / 3.5f);
        #endregion
        
        #region Methods
        public override void DoWindowContents(Rect inRect)
        {
            ColonyUI.DrawColoniesTab(ref inRect, this.Margin,
                PersistentWorldManager.GetInstance().PersistentWorld.Colonies, Load);
        }

        public override void PreOpen()
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
            {
                this.Close();
                return;
            }
            
            base.PreOpen();
        }

        public override void PostClose()
        {
            ColonyUI.Reset();
        }

        private void Load(int index)
        {
            this.Close();
            
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            var colony = persistentWorld.Colonies[index];

            persistentWorld.Game.World.renderer.wantedMode = WorldRenderMode.None;

            var previousColony = persistentWorld.Colony;
            
            LongEventHandler.QueueLongEvent(delegate
            {
                persistentWorld.SaveColony();
            }, "FilUnderscore.PersistentRimWorlds.SavingColony", false, null);
            
            LongEventHandler.QueueLongEvent(delegate
            {
                persistentWorld.ConvertCurrentGameWorldObjects();
                
                persistentWorld.LoadSaver.SaveWorldData();
            }, "FilUnderscore.PersistentRimWorlds.SavingWorld", false, null);
            
            LongEventHandler.QueueLongEvent(delegate
            {
                persistentWorld.LoadSaver.LoadColony(ref colony);
                persistentWorld.Colonies[index] = colony;
                            
                persistentWorld.SetPlayerFactionVarsToColonyFaction();
            }, "FilUnderscore.PersistentRimWorlds.LoadingColony", true, null);
            
            LongEventHandler.QueueLongEvent(delegate
            {
                // TODO: Figure out how to load asynchronously to not lock up game.
                var maps = DynamicMapLoader.LoadColonyMaps(colony);
                Current.Game.CurrentMap = Current.Game.FindMap(maps.First().Tile);

                persistentWorld.UnloadColony(previousColony);

                persistentWorld.ConvertToCurrentGameWorldObjects();
                
                persistentWorld.CheckAndSetColonyData();
                            
                Find.CameraDriver.SetRootPosAndSize(colony.GameData.CamRootPos, colony.GameData.DesiredSize);   
            }, "FilUnderscore.PersistentRimWorlds.LoadingMaps", false, null);
        }
        #endregion
    }
}