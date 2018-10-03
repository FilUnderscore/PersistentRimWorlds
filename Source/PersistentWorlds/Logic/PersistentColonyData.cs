using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData : IExposable
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
        public UniqueIDsManager uniqueIDsManager = new UniqueIDsManager();
        public List<GameComponent> gameComponents = new List<GameComponent>();

        public void ExposeData()
        {
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
            Scribe_Deep.Look<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
            Scribe_Collections.Look<GameComponent>(ref this.gameComponents, "components", LookMode.Deep, new object[] { PersistentWorldManager.PersistentWorld.Game });
            
            if(Find.CameraDriver != null)
                Find.CameraDriver.Expose();
        }

        public static PersistentColonyData Convert(Game game)
        {
            PersistentColonyData persistentColonyData = new PersistentColonyData();

            persistentColonyData.currentMapIndex = game.currentMapIndex;
            persistentColonyData.info = game.Info;
            persistentColonyData.scenario = game.Scenario;
            persistentColonyData.playSettings = game.playSettings;
            persistentColonyData.storyWatcher = game.storyWatcher;
            persistentColonyData.gameEnder = game.gameEnder;
            persistentColonyData.letterStack = game.letterStack;
            persistentColonyData.researchManager = game.researchManager;
            persistentColonyData.storyteller = game.storyteller;
            persistentColonyData.history = game.history;
            persistentColonyData.taleManager = game.taleManager;
            persistentColonyData.playLog = game.playLog;
            persistentColonyData.battleLog = game.battleLog;
            persistentColonyData.outfitDatabase = game.outfitDatabase;
            persistentColonyData.drugPolicyDatabase = game.drugPolicyDatabase;
            persistentColonyData.tutor = game.tutor;
            persistentColonyData.dateNotifier = game.dateNotifier;
            persistentColonyData.uniqueIDsManager = game.uniqueIDsManager;
            persistentColonyData.gameComponents = game.components;

            return persistentColonyData;
        }
    }
}