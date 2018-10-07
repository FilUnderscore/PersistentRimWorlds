using System.Collections.Generic;
using System.Linq;
using Harmony;
using PersistentWorlds.Logic;
using PersistentWorlds.World;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.UI
{
    public class MainTabWindow_Colonies : MainTabWindow
    {
        private static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Town");
        // TODO: Draw.

        private Vector2 scrollPosition = Vector2.zero;
        private List<ScrollableListItem> items;
        
        public MainTabWindow_Colonies()
        {
            this.forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            /*
            base.DoWindowContents(inRect);
            
            GUI.BeginGroup(inRect);

            var boxRect = new Rect(0, 0, 100, 100);
            Widgets.DrawBox(boxRect, 1);
            
            GUI.BeginGroup(boxRect);

            var townTexRect = new Rect(boxRect.x + 10, boxRect.y, 80, 80);
            GUI.DrawTexture(townTexRect, Town);
            
            GUI.EndGroup();
            
            GUI.EndGroup();
            */
            
            ScrollableListUI.DrawList(ref inRect, ref scrollPosition, this.items.ToArray());
        }

        public override void PreOpen()
        {
            if (PersistentWorldManager.PersistentWorld == null)
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

            foreach (var colony in PersistentWorldManager.PersistentWorld.Colonies)
            {
                var item = new ScrollableListItem();

                item.Text = colony.ColonyData.ColonyFaction.Name;

                if (colony != PersistentWorldManager.PersistentWorld.Colony)
                {
                    item.ActionButtonText = "Switch To";
                    item.ActionButtonAction = delegate
                    {
                        LongEventHandler.QueueLongEvent(delegate
                            {
                                PersistentWorldManager.PersistentWorld.Colony = colony;
                                PersistentWorldManager.PersistentWorld.ConvertCurrentGameSettlements(PersistentWorldManager.PersistentWorld.Game);
                                PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();

                                LoadMaps(colony);
                            }, "LoadingColony", false, null);
                    };
                }
                
                this.items.Add(item);
            }
        }

        /*
        public override void PostClose()
        {
            base.PostClose();
            
            LongEventHandler.QueueLongEvent(delegate
            {
                foreach (var wo in Find.World.worldObjects.AllWorldObjects)
                {
                    if (wo.def != DefDatabase<WorldObjectDef>.GetNamed("Colony")) continue;

                    var colony = (Colony) wo;

                    GenTest(colony);
                    PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Remove(Find.CurrentMap
                        .Tile);
                    Current.Game.DeinitAndRemoveMap(Find.CurrentMap);
                }
                
                PersistentWorldManager.PersistentWorld.ConvertToCurrentGameSettlements();
            }, "GeneratingMap", false, null);
        }
        */

        private void LoadMaps(PersistentColony colony)
        {
            foreach (var map in PersistentWorldManager.PersistentWorld.Maps[colony])
            {
                Current.Game.Maps.Add(map);
                map.mapDrawer.RegenerateEverythingNow();
                map.FinalizeLoading();
                map.Parent.FinalizeLoading();
            }

            foreach (var map in Current.Game.Maps)
            {
                if (PersistentWorldManager.PersistentWorld.Maps[colony].Contains(map)) continue;
                
                Current.Game.DeinitAndRemoveMap(map);
            }
        }

        /*
        private void GenTest(Colony colony)
        {
            Log.Message("Running gen test.");

            var map = FindMap(colony.Tile);
                //Current.Game.AddMap(map);
            Current.Game.Maps.Add(map);    
            //map.areaManager.AddStartingAreas();
                //map.weatherDecider.StartInitialWeather();
                //Find.Scenario.PostMapGenerate(map);
            map.mapDrawer.RegenerateEverythingNow();
                map.FinalizeLoading();
                map.Parent.FinalizeLoading();    
            //MapComponentUtility.MapGenerated(map);
                //colony.PostMapGenerate();
            
            PersistentWorldManager.PersistentWorld.Colony.ColonyData.ActiveWorldTiles.Add(map.Tile);
        }

        private Map FindMap(int tile)
        {
            foreach (var map in PersistentWorldManager.PersistentWorld.Maps.Values)
            {
                foreach (var m in map)
                {
                    if (m.Tile == tile)
                    {
                        return m;
                    }
                }
            }

            Log.Error("N");
            return null;
        }
        */
    }
}