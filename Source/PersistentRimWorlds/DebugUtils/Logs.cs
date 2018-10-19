using JetBrains.Annotations;
using Verse;

namespace PersistentWorlds.Debug
{
    public class Logs
    {
        private const string Prefix = "[Persistent RimWorlds]";
        
        public static void Info(string msg)
        {
            Log.Message(Prefix + " " + msg);
        }

        public static void Warning(string msg)
        {
            Log.Warning(Prefix + " " + msg);
        }

        public static void Error(string msg)
        {
            Log.Error(Prefix + " " + msg);
        }
    }
}