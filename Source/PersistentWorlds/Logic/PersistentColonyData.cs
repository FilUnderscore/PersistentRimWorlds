using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public class PersistentColonyData
    {
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
    }
}