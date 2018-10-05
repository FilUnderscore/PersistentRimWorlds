using PersistentWorlds.Logic;

namespace PersistentWorlds
{
    public static class PersistentWorldManager
    {
        public static PersistentWorld PersistentWorld;
        public static PersistentWorldLoadSaver WorldLoadSaver;

        public static bool Active()
        {
            return PersistentWorld != null || WorldLoadSaver != null &&
                   WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Ingame;
        }
    }
}