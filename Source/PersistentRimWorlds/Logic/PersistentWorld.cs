using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using PersistentWorlds.SaveAndLoad;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using Verse;
using PersistentWorlds.Utils;

namespace PersistentWorlds.Logic
{
    public class PersistentWorld : IDisposable
    {
        #region Fields
        public PersistentWorldLoadSaver LoadSaver;
        
        // Game.World is accessed.
        public Game Game = new Game();

        public PersistentWorldData WorldData = new PersistentWorldData();
        public PersistentColony Colony;
        
        // Stores map tile ids.
        public readonly Dictionary<int, HashSet<PersistentColony>> LoadedMaps = new Dictionary<int, HashSet<PersistentColony>>();
        public readonly List<PersistentColony> Colonies = new List<PersistentColony>();
        #endregion
        
        #region Constructors
        public PersistentWorld()
        {
            Current.Game = this.Game;
            this.Game.World = new RimWorld.Planet.World();
        }
        #endregion

        #region Methods
        public void LoadWorld()
        {
            Current.Game = this.Game;
            
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.LoadingWorld".Translate());
            
            this.Game.LoadGame();
            
            // At the end.. because Scribe doesn't run due to us not loading Game directly.
            this.Game.FinalizeInit();
            
            this.LoadCameraDriver();
            
            GameComponentUtility.LoadedGame();
            
            this.CheckAndSetColonyData();
            
            Log.Message("Faction count: " + Current.Game.World.factionManager.AllFactions.Count());
            
            Current.Game.World.factionManager.AllFactions.Do((faction) =>
            {
                Log.Message("Faction Name: " + faction.Name);
                Log.Message("Faction ID: " + faction.loadID);
                Log.Message("Type: " + faction.def.defName);
                Log.Message("Relations: " + ((List<FactionRelation>) AccessTools.Field(typeof(Faction), "relations").GetValue(faction)).ToDebugString());
            });
        }
        
        // Called from Patched Game.LoadGame().
        public void LoadGame()
        {
            /*
             * Fill Game components.
             */
            
            if (Colony == null)
            {
                // Return to main menu.
                Log.Error("Colony is null. - Persistent Worlds");
                GenScene.GoToMainMenu();
                return;
            }
            
            Colony.GameData.SetGame();

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            AccessTools.Method(typeof(Game), "FillComponents", new Type[0]).Invoke(this.Game, new object[0]);
            BackCompatibility.GameLoadingVars(this.Game);

            /*
             * Load world and maps.
             */
            
            this.LoadGameWorldAndMaps();
        }

        private void LoadGameWorldAndMaps()
        {
            this.ExposeGameWorldData();
            
            this.Game.World.FinalizeInit();

            this.SetPlayerFactionVarsToColonyFaction();
            
            this.LoadMaps();
        }

        private void LoadMaps()
        {
            var maps = this.LoadSaver.LoadMaps(this.Colony.ColonyData.ActiveWorldTiles.ToArray());
            maps.Do(Current.Game.AddMap);
            
            this.ConvertToCurrentGameWorldObjects();
            
            if (this.Game.Maps.RemoveAll((Map x) => x == null) != 0)
            {
                Log.Warning("Some maps were null after loading.", false);
            }

            int num = -1;

            num = Colony.GameData.currentMapIndex;
            if (num < 0 && this.Game.Maps.Any<Map>())
            {
                Log.Error("PersistentWorlds - Current map is null after loading but there are maps available. Setting current map to [0].", false);
                num = 0;
            }

            if (num >= this.Game.Maps.Count)
            {
                Log.Error("Current map index out of bounds after loading.", false);
                if (this.Game.Maps.Any<Map>())
                {
                    num = 0;
                }
                else
                {
                    num = -1;
                }
            }
            
            Game.CurrentMap = ((num < 0) ? null : this.Game.Maps[num]);

            foreach (var t in this.Game.Maps)
            {
                try
                {
                    t.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in Map.FinalizeLoading(): " + e, false);
                }

                try
                {
                    t.Parent.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error("Error in MapParent.FinalizeLoading(): " + e, false);
                }

                this.LoadedMaps.Add(t.Tile, new HashSet<PersistentColony>(){Colony});
            }
        }

        private void LoadCameraDriver()
        {
            if (Find.CameraDriver == null)
            {
                Log.Error("Current CameraDriver is null.");
                return;
            }
            
            Find.CameraDriver.SetRootPosAndSize(this.Colony.GameData.camRootPos, this.Colony.GameData.desiredSize);
        }

        private void ExposeGameWorldData()
        {
            this.Game.World.info = this.WorldData.Info;
            this.Game.World.grid = this.WorldData.Grid;

            if (this.Game.World.components == null)
            {
                Log.Error("Game World Components is null! Please look into this.");
                this.Game.World.components = new List<WorldComponent>();
            }
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (this.Game.World.grid == null || !this.Game.World.grid.HasWorldData)
                {
                    WorldGenerator.GenerateWithoutWorldData(this.Game.World.info.seedString);
                }
                else
                {
                    WorldGenerator.GenerateFromScribe(this.Game.World.info.seedString);
                }
            }
            else
            {
                this.ExposeAndFillGameWorldComponents();
            }
        }

        public void ConstructGameWorldComponentsAndExposeComponents()
        {   
            this.Game.World.ConstructComponents();

            this.ExposeAndFillGameWorldComponents();
        }

        public void ExposeAndFillGameWorldComponents()
        {
            this.Game.tickManager = this.WorldData.TickManager;
            this.Game.World.factionManager = this.WorldData.FactionManager;
            this.Game.World.worldPawns = this.WorldData.WorldPawns;
            this.Game.World.worldObjects = this.WorldData.WorldObjectsHolder;
            this.Game.World.gameConditionManager = this.WorldData.GameConditionManager;
            this.Game.World.storyState = this.WorldData.StoryState;
            this.Game.World.features = this.WorldData.WorldFeatures;
            this.Game.uniqueIDsManager = this.WorldData.UniqueIDsManager;
            this.Game.World.components = this.WorldData.WorldComponents;
            
            AccessTools.Method(typeof(RimWorld.Planet.World), "FillComponents", new Type[0]).Invoke(this.Game.World, new object[0]);

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            if (this.WorldData.UniqueIDsManager != null)
            {
                this.Game.uniqueIDsManager = this.WorldData.UniqueIDsManager;
            }
        }

        public void Convert(Game game)
        {
            Current.Game = game;

            this.Game = game;
            
            this.WorldData = PersistentWorldData.Convert(game, this.WorldData);

            var colony = PersistentColony.Convert(game);
            this.Colony = colony;
            
            this.Colonies.Add(colony);
            
            this.ConvertCurrentGameWorldObjects();
        }

        // Convert world objects to colony owned world objects for saving
        public void ConvertCurrentGameWorldObjects()
        {
            this.ConvertCurrentGameSettlements();

            this.ConvertCurrentGameCaravans();
        }

        // Convert Settlements to Colony Bases (this.Colony) for saving
        private void ConvertCurrentGameSettlements()
        {
            // Concurrency errors :/
            var toAdd = new List<Colony>();
            var toRemove = new List<Settlement>();
            
            foreach (var settlement in this.Game.World.worldObjects.Settlements)
            {
                if (settlement.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (settlement.Map?.info == null)
                {
                    continue;
                }
                
                var colony = (Colony) WorldObjectSameIDMaker.MakeWorldObject(PersistentWorldsDefOf.Colony, settlement.ID);
                settlement.Map.info.parent = colony;
                colony.Tile = settlement.Tile;
                colony.Name = settlement.HasName ? settlement.Name : null;

                colony.PersistentColonyData = this.LoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting ? this.Colonies[0].ColonyData : this.Colony.ColonyData;

                toAdd.Add(colony);
                toRemove.Add(settlement);
            }
            
            toAdd.Do(colony => this.Game.World.worldObjects.Add(colony));
            toAdd.Clear();
            
            toRemove.Do(settlement => this.Game.World.worldObjects.Remove(settlement));
            toRemove.Clear();
        }

        private void ConvertCurrentGameCaravans()
        {
            var toRemove = new List<Caravan>();

            foreach (var caravan in this.Game.World.worldObjects.Caravans)
            {
                if (caravan.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (!this.WorldData.ColonyCaravans.ContainsKey(this.Colony.ColonyData.UniqueId))
                    this.WorldData.ColonyCaravans.Add(this.Colony.ColonyData.UniqueId, new ExposableList<Caravan>());
                
                this.WorldData.ColonyCaravans[this.Colony.ColonyData.UniqueId].GetList().Add(caravan);
                
                toRemove.Add(caravan);
            }
            
            toRemove.Do(caravan => this.Game.World.worldObjects.Remove(caravan));
            toRemove.Clear();
        }

        // Convert colony owned world objects to world objects for loading
        public void ConvertToCurrentGameWorldObjects()
        {
            this.ConvertToCurrentGameSettlements();
            
            this.ConvertToCurrentGameCaravans();
        }

        // Convert Colony Bases to Settlements (this.Colony) for loading
        private void ConvertToCurrentGameSettlements()
        {
            var toAdd = new List<Settlement>();
            var toRemove = new List<Colony>();
            
            foreach (var mapParent in this.WorldData.WorldObjectsHolder.MapParents)
            {
                if (!(mapParent is Colony)) continue;
                
                var colony = (Colony) mapParent;

                if (this.Colony == null || colony.PersistentColonyData == null || this.Colony.ColonyData == null || colony.PersistentColonyData.UniqueId != this.Colony.ColonyData.UniqueId) continue;
                
                if (colony.Map == null)
                {
                    var map = Current.Game.FindMap(colony.Tile);

                    if (map != null)
                    {
                        map.info.parent = colony;
                    }
                    else
                    {
                        Log.Error("Null map for colony " + colony.PersistentColonyData.UniqueId + " at " + colony.Tile);
                        
                        continue;
                    }
                }
                
                var settlement =
                    (Settlement) WorldObjectSameIDMaker.MakeWorldObject(WorldObjectDefOf.Settlement, colony.ID);
                settlement.SetFaction(Faction.OfPlayer);
                colony.Map.info.parent = settlement;
                settlement.Tile = colony.Tile;
                
                settlement.Name = colony.HasName ? colony.Name : null;
                settlement.namedByPlayer = colony.HasName; // Prevents non-stop renaming.
                
                toAdd.Add(settlement);
                toRemove.Add(colony);
            }
            
            toAdd.Do(settlement => this.WorldData.WorldObjectsHolder.Add(settlement));
            toAdd.Clear();
            
            toRemove.Do(colony => this.WorldData.WorldObjectsHolder.Remove(colony));
            toRemove.Clear();
        }

        private void ConvertToCurrentGameCaravans()
        {
            if (!this.WorldData.ColonyCaravans.ContainsKey(this.Colony.ColonyData.UniqueId))
            {
                return;
            }
            
            foreach (var caravan in this.WorldData.ColonyCaravans[this.Colony.ColonyData.UniqueId].GetList())
            {
                this.WorldData.WorldObjectsHolder.Add(caravan);
                caravan.pather.StartPath(caravan.pather.Destination, caravan.pather.ArrivalAction, true, false);
            }

            this.WorldData.ColonyCaravans.Remove(this.Colony.ColonyData.UniqueId);
        }

        public void UpdateWorld()
        {
            // Hooks in from Game UpdatePlay()
        }

        public void SetPlayerFactionVarsToColonyFaction()
        {
            SetFactionVars(this.Colony.ColonyData.ColonyFaction, this.WorldData.FactionManager.OfPlayer, FactionMode.Load, true);
        }

        public void SetPlayerFactionVarsToNewGeneratedFaction(FactionDef def)
        {
            var generatedFaction = FactionGenerator.NewGeneratedFaction(def);
            generatedFaction.loadID = this.WorldData.FactionManager.OfPlayer.loadID;
            
            SetFactionVars(generatedFaction, this.WorldData.FactionManager.OfPlayer, FactionMode.Reset, true);
        }

        public void SetColonyFactionVarsToPlayerFaction(PersistentColonyData data)
        {
            SetFactionVars(this.WorldData.FactionManager.OfPlayer, data.ColonyFaction, FactionMode.Save, false);
        }

        private void SetFactionVars(Faction sourceFaction, Faction targetFaction, FactionMode mode, bool lateExecute)
        {
            targetFaction.leader = sourceFaction.leader;
    
            targetFaction.def = sourceFaction.def;

            targetFaction.loadID = sourceFaction.loadID;
            
            targetFaction.Name = sourceFaction.HasName ? sourceFaction.Name : null;
            
            targetFaction.randomKey = sourceFaction.randomKey;
            targetFaction.colorFromSpectrum = sourceFaction.colorFromSpectrum;
            targetFaction.centralMelanin = sourceFaction.centralMelanin;

            targetFaction.kidnapped = sourceFaction.kidnapped;
            
            var predatorThreatsField = AccessTools.Field(typeof(Faction), "predatorThreats");
            predatorThreatsField.SetValue(targetFaction, predatorThreatsField.GetValue(sourceFaction));
            
            targetFaction.defeated = sourceFaction.defeated;
            targetFaction.lastTraderRequestTick = sourceFaction.lastTraderRequestTick;
            targetFaction.lastMilitaryAidRequestTick = sourceFaction.lastMilitaryAidRequestTick;

            var naturalGoodwillTimerField = AccessTools.Field(typeof(Faction), "naturalGoodwillTimer");
            naturalGoodwillTimerField.SetValue(targetFaction, naturalGoodwillTimerField.GetValue(sourceFaction));

            if (lateExecute)
            {
                LongEventHandler.ExecuteWhenFinished(() => SetFactionRelationsVars(sourceFaction, targetFaction, mode));
            }
            else
            {
                SetFactionRelationsVars(sourceFaction, targetFaction, mode);
            }
        }

        private void SetFactionRelationsVars(Faction sourceFaction, Faction targetFaction, FactionMode mode)
        {
            // TODO: ...
            var relationsField = AccessTools.Field(typeof(Faction), "relations");

            var sourceFactionRelations = (List<FactionRelation>) relationsField.GetValue(sourceFaction);
            var targetFactionRelations = (List<FactionRelation>) relationsField.GetValue(targetFaction);

            var allFactions = this.WorldData.FactionManager.AllFactionsListForReading;
            
            switch (mode)
            {
                case FactionMode.Load:

                    targetFactionRelations.Clear();
                    
                    foreach (var faction in allFactions)
                    {
                        if (faction.IsPlayer) continue;
                        
                        var factionRelations = (List<FactionRelation>) relationsField.GetValue(faction);

                        // Clear any previous relations that are player factions.
                        for (var i = 0; i < factionRelations.Count; i++)
                        {
                            var relation = factionRelations[i];
                            
                            if (relation.other == null || relation.other.IsPlayer)
                            {
                                factionRelations.Remove(relation);
                            }
                        }
                        
                        // Set relations.
                        var sourceRelation = sourceFaction.RelationWith(faction, true);
                        
                        if (sourceRelation != null)
                        {
                            var goodwill = sourceRelation.goodwill;
                            var kind = sourceRelation.kind;

                            targetFactionRelations.Add(new FactionRelation
                            {
                                other = faction,
                                goodwill = goodwill,
                                kind = kind
                            });

                            factionRelations.Add(new FactionRelation
                            {
                                other = targetFaction,
                                goodwill = goodwill,
                                kind = kind
                            });

                            sourceFactionRelations.Remove(sourceRelation);
                        }
                        else
                        {
                            targetFaction.TryMakeInitialRelationsWith(faction);
                        }
                    }
                    
                    break;
                case FactionMode.Save:

                    targetFactionRelations.Clear();

                    foreach (var faction in allFactions)
                    {   
                        if (faction.IsPlayer) continue;

                        var factionRelations = (List<FactionRelation>) relationsField.GetValue(faction);

                        for(var i = 0; i < factionRelations.Count; i++)
                        {
                            var relation = factionRelations[i];
                            
                            if (relation.other != null && relation.other.IsPlayer && relation.other != sourceFaction)
                            {
                                factionRelations.Remove(relation);
                            }
                        }
                        
                        var sourceRelation = sourceFaction.RelationWith(faction, true);
                        
                        if (sourceRelation != null)
                        {
                            var goodwill = sourceRelation.goodwill;
                            var kind = sourceRelation.kind;

                            targetFactionRelations.Add(new FactionRelation
                            {
                                other = faction,
                                goodwill = goodwill,
                                kind = kind
                            });
                        }
                        else
                        {
                            targetFaction.TryMakeInitialRelationsWith(faction);
                        }
                    }
                    
                    break;
                case FactionMode.Reset:
                    
                    relationsField.SetValue(targetFaction, sourceFactionRelations);

                    foreach (var faction in allFactions)
                    {
                        if (faction.IsPlayer) continue;
                        
                        var factionRelations = (List<FactionRelation>) relationsField.GetValue(faction);

                        for (var i = 0; i < factionRelations.Count; i++)
                        {
                            var relation = factionRelations[i];
                            
                            if (relation.other != null && relation.other.IsPlayer)
                            {
                                factionRelations.Remove(relation);
                            }
                        }

                        var factionRelation = sourceFaction.RelationWith(faction, true);

                        if (factionRelation == null)
                        {
                            faction.TryMakeInitialRelationsWith(targetFaction);
                            continue;
                        }
                        
                        factionRelations.Add(new FactionRelation
                        {
                            other = targetFaction,
                            goodwill = factionRelation.goodwill,
                            kind = factionRelation.kind
                        });
                    }
                    
                    break;
            }
        }

        public IEnumerable<Map> GetMapsForColony(PersistentColony colony)
        {
            foreach (var map in LoadedMaps.Keys)
            {
                if (colony.ColonyData.ActiveWorldTiles.Contains(map))
                {
                    yield return Current.Game.FindMap(map);
                }
            }
        }

        public void SaveColony(PersistentColony colony)
        {
            var index = this.Colonies.IndexOf(colony);
            
            LoadSaver.SaveColonyAndColonyMapsData(ref colony);

            Colonies[index] = colony;
        }

        public void UnloadColony(PersistentColony colony)
        {
            DynamicMapUnloader.UnloadColonyMaps(colony);
            Find.ColonistBar.MarkColonistsDirty();

            colony.GameData = null;
        }
        
        public void Dispose()
        {
            this.LoadSaver.ReferenceTable.ClearReferences();
        }

        public override string ToString()
        {
            return $"{nameof(PersistentWorld)} " +
                   $"({nameof(Game)}={Game}, " +
                   $"{nameof(Colony)}={Colony}, " +
                   $"{nameof(LoadedMaps)}={LoadedMaps.ToDebugString()}, " +
                   $"{nameof(Colonies)}={Colonies.ToDebugString()})";
        }
        
        public void CheckAndSetColonyData()
        {
            this.CheckAndSetColonyLeader();
        }

        private void CheckAndSetColonyLeader()
        {
            // TODO: Check for Fluffy's Relations Tab / Psychology.   
            // TODO: Come up with a good algorithm for choosing colony leader.

            if (this.Colony?.ColonyData == null)
            {
                throw new NullReferenceException($"{nameof(CheckAndSetColonyLeader)}: Something is null! {nameof(this.Colony)}: {this.Colony == null} and {nameof(this.Colony.ColonyData)}: {this.Colony?.ColonyData == null}");
            }

            if (this.Colony.ColonyData.Leader != null && this.Colony.ColonyData.Leader.Set)
            {
                if (this.Colony.ColonyData.Leader.Reference == null)
                {
                    this.Colony.ColonyData.Leader.Reference = FindPawn(this.Colony.ColonyData.Leader.UniqueId);
                }

                return;
            }

            foreach (var pawn in Find.CurrentMap.mapPawns.AllPawns)
            {
                if (!pawn.IsColonist) continue;

                this.Colony.ColonyData.Leader = new PersistentColonyLeader(pawn);
                break;
            }
        }

        public Pawn FindPawn(string uniqueId)
        {
            foreach (var map in Find.Maps)
            {
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    if (pawn.GetUniqueLoadID().EqualsIgnoreCase(uniqueId))
                    {
                        return pawn;
                    }
                }
            }

            return null;
        }

        public PersistentColony GetColonyById(int uniqueId)
        {
            foreach (var colony in Colonies)
            {
                if (colony.ColonyData?.UniqueId == uniqueId)
                {
                    return colony;
                }
            }

            return null;
        }
        #endregion
        
        #region Enums
        private enum FactionMode
        {
            Save,
            Load,
            Reset
        }
        #endregion
    }
}