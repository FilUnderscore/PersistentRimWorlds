using System.Linq;
using PersistentWorlds.UI;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    public static class SaveFileUtils
    {
        #region Methods
        public static bool HasPossibleSameWorldName(ScrollableListItem[] items, string filePath)
        {
            var names = items.Select(item => item.Text).ToArray();

            return HasPossibleSameWorldName(names, filePath);
        }
        
        public static bool HasPossibleSameWorldName(string[] names, string filePath)
        {
            var worldName = "";
            
            Scribe.loader.InitLoading(filePath);
            
            if (Scribe.EnterNode("game"))
            {
                if (Scribe.EnterNode("world"))
                {
                    if (Scribe.EnterNode("info"))
                    {
                        Scribe_Values.Look<string>(ref worldName, "name");
                    }
                }
            }
            
            Scribe.loader.ForceStop();
            
            return names.Any(name => worldName.EqualsIgnoreCase(name));
        }
        #endregion
    }
}