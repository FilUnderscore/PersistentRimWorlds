using PersistentWorlds.Logic;

namespace PersistentWorlds
{
    public static class PersistentWorldManager
    {
        public static PersistentWorld PersistentWorld;
        public static PersistentWorldLoadSaver WorldLoadSaver;

        public static bool Active()
        {
            return PersistentWorld != null && WorldLoadSaver != null && PersistentWorld.Colony != null;
        }

        public static bool NotNull()
        {
            return PersistentWorld != null && WorldLoadSaver != null;
        }
    }
}