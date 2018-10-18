using System.Collections.Generic;
using PersistentWorlds.Logic;
using PersistentWorlds.SaveAndLoad;
using RimWorld;
using Verse;

namespace PersistentWorlds
{
    /// <summary>
    /// A singleton class for keeping track of persistent worlds.
    /// </summary>
    public sealed class PersistentWorldManager
    {
        #region Fields
        private static PersistentWorldManager instance;
        
        private PersistentWorld persistentWorld;
        #endregion
        
        #region Properties
        public PersistentWorld PersistentWorld
        {
            get => this.persistentWorld;
            set => this.persistentWorld = value;
        }
        #endregion

        public static PersistentWorldManager GetInstance()
        {
            return instance ?? (instance = new PersistentWorldManager());
        }
        
        #region Checking Methods
        public bool PersistentWorldNotNull()
        {
            return persistentWorld != null;
        }

        public bool PersistentWorldNotNullAndLoadStatusIs(PersistentWorldLoadSaver.PersistentWorldLoadStatus status)
        {
            return persistentWorld?.LoadSaver != null && persistentWorld.LoadSaver.Status == status;
        }
        
        public bool PersistentWorldNotNullAndLoadStatusIsNot(PersistentWorldLoadSaver.PersistentWorldLoadStatus status)
        {
            return !PersistentWorldNotNullAndLoadStatusIs(status);
        }
        #endregion
        
        #region Other Methods
        public void Clear()
        {
            if (persistentWorld != null)
            {
                persistentWorld.Dispose();
                persistentWorld = null;
            }

            ScribeVars.Clear();
            ScribeMultiLoader.Clear();
            
            Scribe.ForceStop();
        }
        #endregion
    }
}