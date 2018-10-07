using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyGameData : IExposable
    {
        public sbyte currentMapIndex;
        
        public GameInfo info = new GameInfo();
        public GameRules rules = new GameRules();
        public Scenario scenario;
        public PlaySettings playSettings = new PlaySettings();
        public StoryWatcher storyWatcher = new StoryWatcher();
        public GameEnder gameEnder = new GameEnder();
        public LetterStack letterStack = new LetterStack();
        public ResearchManager researchManager = new ResearchManager();
        public Storyteller storyteller = new Storyteller();
        public History history = new History();
        public TaleManager taleManager = new TaleManager();
        public PlayLog playLog = new PlayLog();
        public BattleLog battleLog = new BattleLog();
        public OutfitDatabase outfitDatabase = new OutfitDatabase();
        public DrugPolicyDatabase drugPolicyDatabase = new DrugPolicyDatabase();
        public Tutor tutor = new Tutor();
        public DateNotifier dateNotifier = new DateNotifier();
        public List<GameComponent> gameComponents = new List<GameComponent>();

        /*
         * Camera Driver.
         */
        public Vector3 camRootPos;
        public float desiredSize;
        
        public void ExposeData()
        {
            if (PersistentWorldManager.PersistentWorld == null)
            {
                Log.Error("PersistentWorld is null.");
                
                GenScene.GoToMainMenu();
                
                return;
            }
            
            Scribe_Values.Look<sbyte>(ref currentMapIndex, "currentMapIndex", -1, false);
            
            Scribe_Deep.Look<GameInfo>(ref info, "info", new object[0]);
            
            Scribe_Deep.Look<GameRules>(ref rules, "rules", new object[0]);
            
            Scribe_Deep.Look<Scenario>(ref scenario, "scenario", new object[0]);
            
            Scribe_Deep.Look<PlaySettings>(ref this.playSettings, "playSettings", new object[0]);
            
            Scribe_Deep.Look<StoryWatcher>(ref this.storyWatcher, "storyWatcher", new object[0]);
            
            Scribe_Deep.Look<GameEnder>(ref this.gameEnder, "gameEnder", new object[0]);

            Scribe_Deep.Look<LetterStack>(ref this.letterStack, "letterStack", new object[0]);
            
            Scribe_Deep.Look<ResearchManager>(ref this.researchManager, "researchManager", new object[0]);
            
            Scribe_Deep.Look<Storyteller>(ref this.storyteller, "storyteller", new object[0]);
            
            Scribe_Deep.Look<History>(ref this.history, "history", new object[0]);
            
            Scribe_Deep.Look<TaleManager>(ref this.taleManager, "taleManager", new object[0]);
            
            Scribe_Deep.Look<PlayLog>(ref this.playLog, "playLog", new object[0]);
            
            Scribe_Deep.Look<BattleLog>(ref this.battleLog, "battleLog", new object[0]);
            
            Scribe_Deep.Look<OutfitDatabase>(ref this.outfitDatabase, "outfitDatabase", new object[0]);
            
            Scribe_Deep.Look<DrugPolicyDatabase>(ref this.drugPolicyDatabase, "drugPolicyDatabase", new object[0]);
            
            Scribe_Deep.Look<Tutor>(ref this.tutor, "tutor", new object[0]);
            
            Scribe_Deep.Look<DateNotifier>(ref this.dateNotifier, "dateNotifier", new object[0]);
            
            Scribe_Collections.Look<GameComponent>(ref this.gameComponents, "components", LookMode.Deep, new object[] { PersistentWorldManager.PersistentWorld.Game });
            
            Scribe_Values.Look<Vector3>(ref this.camRootPos, "camRootPos", new Vector3(), false);
            Scribe_Values.Look<float>(ref this.desiredSize, "desiredSize", 0.0f, false);
        }

        public void SetGame()
        {
            PersistentWorldManager.PersistentWorld.Game.currentMapIndex = this.currentMapIndex;
            
            AccessTools.Field(typeof(Game), "info").SetValue(PersistentWorldManager.PersistentWorld.Game, this.info);
            AccessTools.Field(typeof(Game), "rules").SetValue(PersistentWorldManager.PersistentWorld.Game, this.rules);
            
            PersistentWorldManager.PersistentWorld.Game.Scenario = this.scenario;
            PersistentWorldManager.PersistentWorld.Game.playSettings = this.playSettings;
            PersistentWorldManager.PersistentWorld.Game.storyWatcher = this.storyWatcher;
            PersistentWorldManager.PersistentWorld.Game.gameEnder = this.gameEnder;
            PersistentWorldManager.PersistentWorld.Game.letterStack = this.letterStack;
            PersistentWorldManager.PersistentWorld.Game.researchManager = this.researchManager;
            PersistentWorldManager.PersistentWorld.Game.storyteller = this.storyteller;
            PersistentWorldManager.PersistentWorld.Game.history = this.history;
            PersistentWorldManager.PersistentWorld.Game.taleManager = this.taleManager;
            PersistentWorldManager.PersistentWorld.Game.playLog = this.playLog;
            PersistentWorldManager.PersistentWorld.Game.battleLog = this.battleLog;
            PersistentWorldManager.PersistentWorld.Game.outfitDatabase = this.outfitDatabase;
            PersistentWorldManager.PersistentWorld.Game.drugPolicyDatabase = this.drugPolicyDatabase;
            PersistentWorldManager.PersistentWorld.Game.tutor = this.tutor;
            PersistentWorldManager.PersistentWorld.Game.dateNotifier = this.dateNotifier;
            PersistentWorldManager.PersistentWorld.Game.components = this.gameComponents;
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
                tutor = game.tutor,
                dateNotifier = game.dateNotifier,
                gameComponents = game.components,
                camRootPos = cameraDriverRootPos,
                desiredSize = cameraDriverDesiredSize
            };

            return persistentColonyGameData;
        }
    }
}