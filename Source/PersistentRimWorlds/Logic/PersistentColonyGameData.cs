using System;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using PersistentWorlds.Utils;
using RimWorld.Planet;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyGameData : IExposable
    {
        #region Fields
        public sbyte currentMapIndex;
        public int mapSize;
        
        private GameInfo info = new GameInfo();
        private GameRules rules = new GameRules();
        private Scenario scenario;
        private PlaySettings playSettings = new PlaySettings();
        private StoryWatcher storyWatcher = new StoryWatcher();
        private GameEnder gameEnder = new GameEnder();
        private LetterStack letterStack = new LetterStack();
        private ResearchManager researchManager = new ResearchManager();
        private Storyteller storyteller = new Storyteller();
        private History history = new History();
        private TaleManager taleManager = new TaleManager();
        private PlayLog playLog = new PlayLog();
        private BattleLog battleLog = new BattleLog();
        private OutfitDatabase outfitDatabase = new OutfitDatabase();
        private DrugPolicyDatabase drugPolicyDatabase = new DrugPolicyDatabase();
        private FoodRestrictionDatabase foodRestrictionDatabase = new FoodRestrictionDatabase();
        private Tutor tutor = new Tutor();
        private DateNotifier dateNotifier = new DateNotifier();
        private List<GameComponent> gameComponents = new List<GameComponent>();

        /*
         * Camera Driver.
         */
        public Vector3 camRootPos;
        public float desiredSize;
        #endregion
        
        #region Methods
        public void ExposeData()
        {
            Scribe_Values.Look<sbyte>(ref currentMapIndex, "currentMapIndex", -1);

            Scribe_Values.Look<int>(ref mapSize, "mapSize");
            
            Scribe_Deep.Look(ref info, "info");

            Scribe_Deep.Look(ref rules, "rules");

            Scribe_Deep.Look(ref scenario, "scenario");

            Scribe_Deep.Look(ref this.playSettings, "playSettings");

            Scribe_Deep.Look(ref this.storyWatcher, "storyWatcher");

            Scribe_Deep.Look(ref this.gameEnder, "gameEnder");

            Scribe_Deep.Look(ref this.letterStack, "letterStack");

            Scribe_Deep.Look(ref this.researchManager, "researchManager");

            Scribe_Deep.Look(ref this.storyteller, "storyteller");

            Scribe_Deep.Look(ref this.history, "history");

            Scribe_Deep.Look(ref this.taleManager, "taleManager");

            Scribe_Deep.Look(ref this.playLog, "playLog");

            Scribe_Deep.Look(ref this.battleLog, "battleLog");

            Scribe_Deep.Look(ref this.outfitDatabase, "outfitDatabase");

            Scribe_Deep.Look(ref this.drugPolicyDatabase, "drugPolicyDatabase");

            Scribe_Deep.Look(ref this.foodRestrictionDatabase, "foodRestrictionDatabase");

            Scribe_Deep.Look(ref this.tutor, "tutor");
            
            Scribe_Deep.Look(ref this.dateNotifier, "dateNotifier");
            
            Scribe_Collections.Look(ref this.gameComponents, "components", LookMode.Deep, new object[] { Current.Game });
            
            Scribe_Values.Look(ref this.camRootPos, "camRootPos");
            Scribe_Values.Look(ref this.desiredSize, "desiredSize");
        }

        public void SetGame()
        {
            Current.Game.currentMapIndex = this.currentMapIndex;
            Current.Game.World.info.initialMapSize = new IntVec3(mapSize, 1, mapSize);
            
            AccessTools.Field(typeof(Game), "info").SetValue(Current.Game, this.info);
            AccessTools.Field(typeof(Game), "rules").SetValue(Current.Game, this.rules);
            
            Current.Game.Scenario = this.scenario;
            Current.Game.playSettings = this.playSettings;
            Current.Game.storyWatcher = this.storyWatcher;
            Current.Game.gameEnder = this.gameEnder;
            Current.Game.letterStack = this.letterStack;
            Current.Game.researchManager = this.researchManager;
            Current.Game.storyteller = this.storyteller;
            Current.Game.history = this.history;
            Current.Game.taleManager = this.taleManager;
            Current.Game.playLog = this.playLog;
            Current.Game.battleLog = this.battleLog;
            Current.Game.outfitDatabase = this.outfitDatabase;
            Current.Game.drugPolicyDatabase = this.drugPolicyDatabase;
            Current.Game.foodRestrictionDatabase = this.foodRestrictionDatabase;
            Current.Game.tutor = this.tutor;
            Current.Game.dateNotifier = this.dateNotifier;
            Current.Game.components = this.gameComponents;
        }

        public static PersistentColonyGameData Convert(Game game)
        {
            /*
             * Camera Driver.
             */
            var cameraDriverRootPos = (Vector3) AccessTools.Field(typeof(CameraDriver), "rootPos").GetValue(Find.CameraDriver);
            var cameraDriverDesiredSize = (float) AccessTools.Field(typeof(CameraDriver), "desiredSize").GetValue(Find.CameraDriver);
            
            var persistentColonyGameData = new PersistentColonyGameData
            {
                currentMapIndex = game.currentMapIndex,
                mapSize = game.World.info.initialMapSize.x,
                info = game.Info,
                scenario = game.Scenario,
                playSettings = game.playSettings,
                storyWatcher = game.storyWatcher,
                gameEnder = game.gameEnder,
                letterStack = game.letterStack,
                researchManager = game.researchManager,
                storyteller = game.storyteller,
                history = game.history,
                taleManager = game.taleManager,
                playLog = game.playLog,
                battleLog = game.battleLog,
                outfitDatabase = game.outfitDatabase,
                drugPolicyDatabase = game.drugPolicyDatabase,
                foodRestrictionDatabase = game.foodRestrictionDatabase,
                tutor = game.tutor,
                dateNotifier = game.dateNotifier,
                gameComponents = game.components,
                camRootPos = cameraDriverRootPos,
                desiredSize = cameraDriverDesiredSize
            };

            return persistentColonyGameData;
        }

        public override string ToString()
        {
            return $"{nameof(PersistentColonyGameData)} " +
                   $"({nameof(currentMapIndex)}={currentMapIndex}, " +
                   $"{nameof(mapSize)}={mapSize}, " +
                   $"{nameof(info)}={info}, " +
                   $"{nameof(rules)}={rules}, " +
                   $"{nameof(scenario)}={scenario}, " +
                   $"{nameof(playSettings)}={playSettings}, " +
                   $"{nameof(storyWatcher)}={storyWatcher}, " +
                   $"{nameof(gameEnder)}={gameEnder}, " +
                   $"{nameof(letterStack)}={letterStack}, " +
                   $"{nameof(researchManager)}={researchManager}, " +
                   $"{nameof(storyteller)}={storyteller}, " +
                   $"{nameof(history)}={history}, " +
                   $"{nameof(taleManager)}={taleManager}, " +
                   $"{nameof(playLog)}={playLog}, " +
                   $"{nameof(battleLog)}={battleLog}, " +
                   $"{nameof(outfitDatabase)}={outfitDatabase}, " +
                   $"{nameof(drugPolicyDatabase)}={drugPolicyDatabase}, " +
                   $"{nameof(foodRestrictionDatabase)}={foodRestrictionDatabase}, " +
                   $"{nameof(tutor)}={tutor}, " +
                   $"{nameof(dateNotifier)}={dateNotifier}, " +
                   $"{nameof(gameComponents)}={gameComponents.ToDebugString()}, " +
                   $"{nameof(camRootPos)}={camRootPos}, " +
                   $"{nameof(desiredSize)}={desiredSize})";
        }
        #endregion
    }
}