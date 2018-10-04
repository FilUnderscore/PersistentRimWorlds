using System.Collections.Generic;
using Harmony;
using RimWorld;
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
        public UniqueIDsManager uniqueIDsManager = new UniqueIDsManager();
        public List<GameComponent> gameComponents = new List<GameComponent>();

        public void ExposeData()
        {
            if (PersistentWorldManager.PersistentWorld == null)
            {
                Log.Error("PersistentWorld is null.");
                
                GenScene.GoToMainMenu();
                
                return;
            }
            
            Scribe_Values.Look<sbyte>(ref currentMapIndex, "currentMapIndex", -1, false);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.currentMapIndex = this.currentMapIndex;
            
            Scribe_Deep.Look<GameInfo>(ref info, "info", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                AccessTools.Field(typeof(Game), "info").SetValue(PersistentWorldManager.PersistentWorld.Game, this.info);
            
            Scribe_Deep.Look<GameRules>(ref rules, "rules", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                AccessTools.Field(typeof(Game), "rules").SetValue(PersistentWorldManager.PersistentWorld.Game, this.rules);
            
            Scribe_Deep.Look<Scenario>(ref scenario, "scenario", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.Scenario = this.scenario;
            
            Scribe_Deep.Look<PlaySettings>(ref this.playSettings, "playSettings", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.playSettings = this.playSettings;
            
            Scribe_Deep.Look<StoryWatcher>(ref this.storyWatcher, "storyWatcher", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.storyWatcher = this.storyWatcher;
            
            Scribe_Deep.Look<GameEnder>(ref this.gameEnder, "gameEnder", new object[0]);

            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.gameEnder = this.gameEnder;
            
            Scribe_Deep.Look<LetterStack>(ref this.letterStack, "letterStack", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.letterStack = this.letterStack;
            
            Scribe_Deep.Look<ResearchManager>(ref this.researchManager, "researchManager", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.researchManager = this.researchManager;
            
            Scribe_Deep.Look<Storyteller>(ref this.storyteller, "storyteller", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.storyteller = this.storyteller;
            
            Scribe_Deep.Look<History>(ref this.history, "history", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.history = this.history;
            
            Scribe_Deep.Look<TaleManager>(ref this.taleManager, "taleManager", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.taleManager = this.taleManager;
            
            Scribe_Deep.Look<PlayLog>(ref this.playLog, "playLog", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.playLog = this.playLog;
            
            Scribe_Deep.Look<BattleLog>(ref this.battleLog, "battleLog", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.battleLog = this.battleLog;
            
            Scribe_Deep.Look<OutfitDatabase>(ref this.outfitDatabase, "outfitDatabase", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.outfitDatabase = this.outfitDatabase;
            
            Scribe_Deep.Look<DrugPolicyDatabase>(ref this.drugPolicyDatabase, "drugPolicyDatabase", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.drugPolicyDatabase = this.drugPolicyDatabase;
            
            Scribe_Deep.Look<Tutor>(ref this.tutor, "tutor", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.tutor = this.tutor;
            
            Scribe_Deep.Look<DateNotifier>(ref this.dateNotifier, "dateNotifier", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.dateNotifier = this.dateNotifier;
            
            Scribe_Deep.Look<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.uniqueIDsManager = this.uniqueIDsManager;
            
            Scribe_Collections.Look<GameComponent>(ref this.gameComponents, "components", LookMode.Deep, new object[] { PersistentWorldManager.PersistentWorld.Game });
            
            if(PersistentWorldManager.LoadStatus == PersistentWorldManager.PersistentWorldLoadStatus.Loading && PersistentWorldManager.PersistentWorld.colony.ColonyData.GameData == this)
                PersistentWorldManager.PersistentWorld.Game.components = this.gameComponents;
            
            if(Find.CameraDriver != null)
                Find.CameraDriver.Expose();
        }

        public static PersistentColonyGameData Convert(Game game)
        {
            PersistentColonyGameData persistentColonyGameData = new PersistentColonyGameData();

            persistentColonyGameData.currentMapIndex = game.currentMapIndex;
            persistentColonyGameData.info = game.Info;
            persistentColonyGameData.scenario = game.Scenario;
            persistentColonyGameData.playSettings = game.playSettings;
            persistentColonyGameData.storyWatcher = game.storyWatcher;
            persistentColonyGameData.gameEnder = game.gameEnder;
            persistentColonyGameData.letterStack = game.letterStack;
            persistentColonyGameData.researchManager = game.researchManager;
            persistentColonyGameData.storyteller = game.storyteller;
            persistentColonyGameData.history = game.history;
            persistentColonyGameData.taleManager = game.taleManager;
            persistentColonyGameData.playLog = game.playLog;
            persistentColonyGameData.battleLog = game.battleLog;
            persistentColonyGameData.outfitDatabase = game.outfitDatabase;
            persistentColonyGameData.drugPolicyDatabase = game.drugPolicyDatabase;
            persistentColonyGameData.tutor = game.tutor;
            persistentColonyGameData.dateNotifier = game.dateNotifier;
            persistentColonyGameData.uniqueIDsManager = game.uniqueIDsManager;
            persistentColonyGameData.gameComponents = game.components;

            return persistentColonyGameData;
        }
    }
}