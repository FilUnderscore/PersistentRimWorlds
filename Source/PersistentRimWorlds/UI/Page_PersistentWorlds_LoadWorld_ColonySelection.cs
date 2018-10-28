using System.Collections.Generic;
using System.Threading;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace PersistentWorlds.UI
{
    [StaticConstructorOnStartup]
    public sealed class Page_PersistentWorlds_LoadWorld_ColonySelection : Page
    {        
        #region Fields
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");

        private readonly PersistentWorld persistentWorld;

        private bool normalClose = true;
        #endregion
        
        #region Properties
        public override Vector2 InitialSize => new Vector2(600f, 700f);
        #endregion
        
        #region Constructors
        public Page_PersistentWorlds_LoadWorld_ColonySelection(PersistentWorld persistentWorld)
        {
            this.persistentWorld = persistentWorld;
            
            // Load colonies in a separate thread.
            new Thread(() => { persistentWorld.LoadSaver.LoadColonies(); }).Start();
            
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }
        #endregion
        
        #region Methods
        public override void PostClose()
        {
            base.PostClose();
            
            ColonyUI.Reset();
            
            if (!normalClose) return;

            this.DoBack();
            PersistentWorldManager.GetInstance().Clear();
        }

        public override void DoWindowContents(Rect inRect)
        {
            ColonyUI.DrawColoniesList(ref inRect, this.Margin, this.CloseButSize, this.persistentWorld.Colonies, this.Load, this.NewColony, this.Delete);
        }

        private void Load(int index)
        {
            normalClose = false;
            this.Close();
            
            var colony = this.persistentWorld.Colonies[index];
             
            PersistentWorldManager.GetInstance().PersistentWorld = this.persistentWorld;
                        
            this.persistentWorld.LoadSaver.LoadColony(ref colony);
            this.persistentWorld.Colonies[index] = colony;

            // This line cause UIRoot_Play to throw one error due to null world/maps, can be patched to check if null before running.
            MemoryUtility.ClearAllMapsAndWorld();

            this.persistentWorld.LoadSaver.TransferToPlayScene();
        }

        private void NewColony()
        {
            normalClose = false;

            PersistentWorldManager.GetInstance().PersistentWorld = this.persistentWorld;

            this.next = new Page_SelectScenario {prev = this};
            this.DoNext();
        }

        private void Delete(int index)
        {
            
        }
        #endregion
    }
}