using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
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
        #region Reflection Fields
        private static readonly FieldInfo RelationsField = AccessTools.Field(typeof(Faction), "relations");

        private static readonly MethodInfo FillComponentsMethod =
            AccessTools.Method(typeof(RimWorld.Planet.World), "FillComponents");
        
        private static readonly FieldInfo PredatorThreatsField = AccessTools.Field(typeof(Faction), "predatorThreats");
        
        private static readonly FieldInfo NaturalGoodwillTimerField = AccessTools.Field(typeof(Faction), "naturalGoodwillTimer");
        #endregion

        public static Delegate AbandonColonyButtonDelegate = new Action(PressAbandonColonyButton);
        
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
        
        #region Reflection Constructors
        static PersistentWorld()
        {
            if(RelationsField == null)
                throw new NullReferenceException($"{nameof(RelationsField)} is null.");
            
            if(FillComponentsMethod == null)
                throw new NullReferenceException($"{nameof(FillComponentsMethod)} is null.");
            
            if(PredatorThreatsField == null)
                throw new NullReferenceException($"{nameof(PredatorThreatsField)} is null.");
            
            if(NaturalGoodwillTimerField == null)
                throw new NullReferenceException($"{nameof(NaturalGoodwillTimerField)} is null.");
        }
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
            
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.Loading.World".Translate());
            
            this.Game.LoadGame();
            
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.Loading.Game.Finalizing".Translate());
            
            // At the end.. because Scribe doesn't run due to us not loading Game directly.
            this.Game.FinalizeInit();
            
            this.LoadCameraDriver();

            SchedulePause();
            
            GameComponentUtility.LoadedGame();
            
            this.CheckAndSetColonyData();
            
            #if DEBUG
            Log.Message($"Faction count: {Current.Game.World.factionManager.AllFactions.Count()}");
            
            Current.Game.World.factionManager.AllFactions.Do((faction) =>
            {
                Log.Message($"Faction Name: {faction.Name}");
                Log.Message($"Faction ID: {faction.loadID}");
                Log.Message($"Type: {faction.def.defName}");
                Log.Message($"Relations: {((List<FactionRelation>) RelationsField.GetValue(faction)).ToDebugString()}");
            });
            #endif
        }

        public void SchedulePause()
        {
            // Pause on load.
            if (Prefs.PauseOnLoad)
            {
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    Find.TickManager.DoSingleTick();
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                });
            }
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
                Log.Error($"{nameof(LoadGame)}: ${nameof(Colony)} is null.");
                GenScene.GoToMainMenu();
                return;
            }
            
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.Loading.Game".Translate());
            
            Colony.GameData.SetGame();

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
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
            LongEventHandler.SetCurrentEventText("FilUnderscore.PersistentRimWorlds.Loading.Maps".Translate());
            
            var maps = this.LoadSaver.LoadMaps(this.Colony.ColonyData.ActiveWorldTiles.ToArray());
            maps.Do(Current.Game.AddMap);
            
            this.ConvertToCurrentGameWorldObjects();
            
            if (this.Game.Maps.RemoveAll((Map x) => x == null) != 0)
            {
                Log.Warning($"{nameof(LoadMaps)}: Some maps were null after loading.");
            }

            int num = -1;

            num = Colony.GameData.CurrentMapIndex;
            if (num < 0 && this.Game.Maps.Any<Map>())
            {
                Log.Error($"{nameof(LoadMaps)}: PersistentWorlds - Current map is null after loading but there are maps available. Setting current map to [0].");
                num = 0;
            }

            if (num >= this.Game.Maps.Count)
            {
                Log.Error($"{nameof(LoadMaps)}: Current map index out of bounds after loading.");
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
                    Log.Error($"{nameof(LoadMaps)}: Error in Map.FinalizeLoading(): {e}");
                }

                try
                {
                    t.Parent.FinalizeLoading();
                }
                catch (Exception e)
                {
                    Log.Error($"{nameof(LoadMaps)}: Error in MapParent.FinalizeLoading(): {e}");
                }

                this.LoadedMaps.Add(t.Tile, new HashSet<PersistentColony>(){Colony});
            }
        }

        private void LoadCameraDriver()
        {
            if (Find.CameraDriver == null)
            {
                Log.Error($"{nameof(LoadCameraDriver)}: Current CameraDriver is null.");
                return;
            }
            
            Find.CameraDriver.SetRootPosAndSize(this.Colony.GameData.CamRootPos, this.Colony.GameData.DesiredSize);
        }

        private void ExposeGameWorldData()
        {
            this.Game.World.info = this.WorldData.Info;
            this.Game.World.grid = this.WorldData.Grid;

            if (this.Game.World.components == null)
            {
                Log.Error($"{nameof(ExposeGameWorldData)}: Game World Components is null! Please look into this.");
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

        private void ExposeAndFillGameWorldComponents()
        {
            this.Game.tickManager = this.WorldData.TickManager;
            this.Game.World.factionManager = this.WorldData.FactionManager;
            this.Game.World.ideoManager = this.WorldData.IdeoManager;
            this.Game.World.worldPawns = this.WorldData.WorldPawns;
            this.Game.World.worldObjects = this.WorldData.WorldObjectsHolder;
            this.Game.World.gameConditionManager = this.WorldData.GameConditionManager;
            this.Game.World.storyState = this.WorldData.StoryState;
            this.Game.World.features = this.WorldData.WorldFeatures;
            this.Game.uniqueIDsManager = this.WorldData.UniqueIDsManager;
            this.Game.World.components = this.WorldData.WorldComponents;
            
            FillComponentsMethod.Invoke(this.Game.World, new object[0]);

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

            var colony = PersistentColony.Convert(this, game);
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
                colony.Name = settlement.Name;
                colony.NamedByPlayer = settlement.namedByPlayer;

                colony.PersistentColonyData = this.LoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting ? this.Colonies[0].ColonyData : this.Colony.ColonyData;
                
                toAdd.Add(colony);
                toRemove.Add(settlement);
            }
            
            toAdd.Do(colony => this.Game.World.worldObjects.Add(colony));
            toAdd.Clear();
            
            toRemove.Do(settlement =>
            {
                // Try to fix errors in conversion?
                try
                {
                    this.Game.World.worldObjects.Remove(settlement);
                }
                catch (NullReferenceException)
                {
                    Log.Warning($"{nameof(ConvertCurrentGameSettlements)}: NRE thrown while removing WorldObject. ID: {settlement.ID}");
                }
            });
            
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
                        Log.Error($"{nameof(ConvertToCurrentGameSettlements)}: Null map for colony {colony.PersistentColonyData.UniqueId} at {colony.Tile}");
                        
                        continue;
                    }
                }
                
                var settlement =
                    (Settlement) WorldObjectSameIDMaker.MakeWorldObject(WorldObjectDefOf.Settlement, colony.ID);
                settlement.SetFaction(Faction.OfPlayer);
                colony.Map.info.parent = settlement;
                settlement.Tile = colony.Tile;
                
                settlement.Name = colony.Name;
                settlement.namedByPlayer = colony.NamedByPlayer; // Prevents non-stop renaming.
                
                toAdd.Add(settlement);
                toRemove.Add(colony);
            }
            
            toAdd.Do(settlement => this.WorldData.WorldObjectsHolder.Add(settlement));
            toAdd.Clear();
            
            toRemove.Do(colony => this.WorldData.WorldObjectsHolder.Remove(colony));
            toRemove.Clear();
        }

        
        private void ConvertColoniesToAbandonedColonies(PersistentColony persistentColony)
        {
            var toAdd = new List<AbandonedColony>();
            var toRemove = new List<Settlement>();

            foreach (var settlement in this.WorldData.WorldObjectsHolder.Settlements)
            {
                if (settlement.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (settlement.Map?.info == null)
                {
                    continue;
                }

                var abandonedColony =
                    (AbandonedColony) WorldObjectSameIDMaker.MakeWorldObject(PersistentWorldsDefOf.AbandonedColony,
                        settlement.ID);

                settlement.Map.info.parent = abandonedColony;
                abandonedColony.Tile = settlement.Tile;

                abandonedColony.Name = persistentColony.ColonyData.ColonyFaction.Name;
                    
                toAdd.Add(abandonedColony);
                toRemove.Add(settlement);
            }

            toAdd.Do(abandonedColony => this.WorldData.WorldObjectsHolder.Add(abandonedColony));
            toAdd.Clear();
            
            toRemove.Do(settlement => this.WorldData.WorldObjectsHolder.Remove(settlement));
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
            var generatedFaction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));
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
            
            PredatorThreatsField.SetValue(targetFaction, PredatorThreatsField.GetValue(sourceFaction));
            
            targetFaction.defeated = sourceFaction.defeated;
            targetFaction.lastTraderRequestTick = sourceFaction.lastTraderRequestTick;
            targetFaction.lastMilitaryAidRequestTick = sourceFaction.lastMilitaryAidRequestTick;

            targetFaction.ideos = sourceFaction.ideos;

            NaturalGoodwillTimerField.SetValue(targetFaction, NaturalGoodwillTimerField.GetValue(sourceFaction));

            if (lateExecute)
            {
                LongEventHandler.ExecuteWhenFinished(() => SetFactionRelationsVars(sourceFaction, targetFaction, mode));
            }
            else
            {
                SetFactionRelationsVars(sourceFaction, targetFaction, mode);
            }
        }

        public void SetFactionRelationsVars(Faction sourceFaction, Faction targetFaction, FactionMode mode)
        {
            var sourceFactionRelations = (List<FactionRelation>) RelationsField.GetValue(sourceFaction);
            var targetFactionRelations = (List<FactionRelation>) RelationsField.GetValue(targetFaction);

            var allFactions = this.WorldData.FactionManager.AllFactionsListForReading;
            
            switch (mode)
            {
                case FactionMode.Load:

                    targetFactionRelations.Clear();
                    
                    foreach (var faction in allFactions)
                    {
                        if (faction.IsPlayer) continue;
                        
                        var factionRelations = (List<FactionRelation>) RelationsField.GetValue(faction);

                        // Clear any previous relations that are player factions.
                        for (var i = 0; i < factionRelations.Count; i++)
                        {
                            var relation = factionRelations[i];
                            
                            if (relation.other == null || relation.other == targetFaction)
                            {
                                factionRelations.Remove(relation);
                            }
                        }
                        
                        // Set relations.
                        var sourceRelation = sourceFaction.RelationWith(faction, true);
                        
                        if (sourceRelation != null)
                        {
                            var goodwill = sourceRelation.baseGoodwill;
                            var kind = sourceRelation.kind;

                            targetFactionRelations.Add(new FactionRelation
                            {
                                other = faction,
                                baseGoodwill = goodwill,
                                kind = kind
                            });

                            factionRelations.Add(new FactionRelation
                            {
                                other = targetFaction,
                                baseGoodwill = goodwill,
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

                        var factionRelations = (List<FactionRelation>) RelationsField.GetValue(faction);

                        for(var i = 0; i < factionRelations.Count; i++)
                        {
                            var relation = factionRelations[i];
                            
                            if (relation.other != null && relation.other == targetFaction && relation.other != sourceFaction)
                            {
                                factionRelations.Remove(relation);
                            }
                        }
                        
                        var sourceRelation = sourceFaction.RelationWith(faction, true);
                        
                        if (sourceRelation != null)
                        {
                            var goodwill = sourceRelation.baseGoodwill;
                            var kind = sourceRelation.kind;

                            targetFactionRelations.Add(new FactionRelation
                            {
                                other = faction,
                                baseGoodwill = goodwill,
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
                    
                    RelationsField.SetValue(targetFaction, sourceFactionRelations);

                    foreach (var faction in allFactions)
                    {
                        if (faction.IsPlayer) continue;
                        
                        var factionRelations = (List<FactionRelation>) RelationsField.GetValue(faction);

                        for (var i = 0; i < factionRelations.Count; i++)
                        {
                            var relation = factionRelations[i];
                            
                            if (relation.other != null && relation.other == targetFaction)
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
                            baseGoodwill = factionRelation.baseGoodwill,
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

        public void SaveColony()
        {
            var index = this.Colonies.IndexOf(this.Colony);
            
            LoadSaver.SaveColonyAndColonyMapsData(ref this.Colony);

            Colonies[index] = this.Colony;
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

            this.SetColonyLeaderAutomatically();
        }

        private void SetColonyLeaderAutomatically()
        {
            // Mod Support for Psychology.
            if (ModSupportHelper.IsPsychologyLoaded())
            {
                this.Colony.ColonyData.Leader = new PersistentColonyLeader(ModSupportHelper.GetPsychologyMayor());
            }
            // Mod Support for Fluffy's Relations Tab
            else if (Colony.ColonyData.ColonyFaction?.leader != null)
            {
                this.Colony.ColonyData.Leader = new PersistentColonyLeader(this.Colony.ColonyData.ColonyFaction.leader);
            }
            else
            {   
                // TODO: Come up with a good algorithm for choosing colony leader.

                foreach (var pawn in Find.CurrentMap.mapPawns.AllPawns.Where(pawn => pawn.IsColonist))
                {
                    this.Colony.ColonyData.Leader = new PersistentColonyLeader(pawn);
                    break;
                }
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
            return Colonies.First(colony => colony.ColonyData?.UniqueId == uniqueId);
        }

        public void DeleteColony(PersistentColony colony)
        {
            this.ConvertColoniesToAbandonedColonies(colony);
            
            // TODO: Insert any processes before colony abandonment/deletion.

            this.SaveWorld(() =>
            {
                SaveFileUtils.DeleteFile(this.Colony.FileInfo.FullName);
                GenScene.GoToMainMenu();
            });
        }

        // Called when Game Over Abandon Colony Button is pressed.
        private static void PressAbandonColonyButton()
        {
            if (!PersistentWorldManager.GetInstance()
                .PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame))
                throw new InvalidOperationException($"{nameof(PressAbandonColonyButton)}: 'Game Over Colony Abandon' button cannot be pressed while not currently in a Persistent World.");

            // TODO: Enter world view without tabs and ask player to choose a different colony.

            // Delete current colony, "abandoning" it.
            var persistentWorld = PersistentWorldManager.GetInstance().PersistentWorld;
            persistentWorld.DeleteColony(persistentWorld.Colony);
        }

        public void SaveWorld(Action toExecuteWhenFinished = null)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                this.LoadSaver.SaveWorld();

                if (toExecuteWhenFinished != null)
                    LongEventHandler.ExecuteWhenFinished(toExecuteWhenFinished);
            }, "FilUnderscore.PersistentRimWorlds.Saving.World", false, null);
        }
        #endregion
        
        #region Enums
        public enum FactionMode
        {
            Save,
            Load,
            Reset
        }
        #endregion
    }
}