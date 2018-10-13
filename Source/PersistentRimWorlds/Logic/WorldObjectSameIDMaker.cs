using System;
using PersistentWorlds.World;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PersistentWorlds.Logic
{
    public static class WorldObjectSameIDMaker
    {
        #region Methods
        public static WorldObject MakeWorldObject(WorldObjectDef def, int id)
        {
            var instance = (WorldObject) Activator.CreateInstance(def.worldObjectClass);

            instance.def = def;
            instance.ID = id;
            instance.creationGameTicks = Find.TickManager.TicksGame;
            instance.PostMake();
            
            return instance;
        }
        #endregion
    }
}