using System;
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
        private static PersistentWorldManager _instance;
        
        private PersistentWorld persistentWorld;
        #endregion
        
        #region Properties
        public bool HasPersistentWorld => persistentWorld != null;
        
        public PersistentWorld PersistentWorld
        {
            get
            {
                if (this.persistentWorld == null)
                {
                    throw new NullReferenceException("Persistent World Field in PersistentWorldManager is null!");
                }
                
                return this.persistentWorld;
            }
            set => this.persistentWorld = value;
        }
        #endregion

        public static PersistentWorldManager GetInstance()
        {
            return _instance ??= new PersistentWorldManager();
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