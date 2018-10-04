using PersistentWorlds.Logic;

namespace PersistentWorlds
{
    public class PersistentWorldManager
    {
        public static PersistentWorld PersistentWorld;
        public static PersistentWorldLoader WorldLoader;
        public static int LoadColonyIndex = -1;

        public static PersistentWorldLoadStatus LoadStatus;

        public static ScribeMultiLoader MultiLoader = new ScribeMultiLoader();
        
        public enum PersistentWorldLoadStatus
        {
            Uninitialized,
            Patching,
            Loading,
            Finalizing,
            Ingame
        }
    }
}