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
        
        public MainTabWindow_Colonies()
        {
            this.forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            
            GUI.BeginGroup(inRect);

            var boxRect = new Rect(0, 0, 100, 100);
            Widgets.DrawBox(boxRect, 1);
            
            GUI.BeginGroup(boxRect);

            var townTexRect = new Rect(boxRect.x + 10, boxRect.y, 80, 80);
            GUI.DrawTexture(townTexRect, Town);
            
            GUI.EndGroup();
            
            GUI.EndGroup();
        }

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
            }, "GeneratingMap", false, null);
        }

        private void GenTest(Colony colony)
        {
            Log.Message("Running gen test.");

            var map = FindMap(colony.Tile);
                Current.Game.AddMap(map);
                map.areaManager.AddStartingAreas();
                map.weatherDecider.StartInitialWeather();
                Find.Scenario.PostMapGenerate(map);
                map.FinalizeLoading();
                MapComponentUtility.MapGenerated(map);
                colony.PostMapGenerate();
            
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
    }
}