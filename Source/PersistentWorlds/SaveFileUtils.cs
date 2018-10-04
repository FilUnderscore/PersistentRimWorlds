using Verse;

namespace PersistentWorlds
{
    public sealed class SaveFileUtils
    {
        public static bool HasSameWorldName(string name, string filePath)
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
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return worldName.EqualsIgnoreCase(name);
        }
    }
}