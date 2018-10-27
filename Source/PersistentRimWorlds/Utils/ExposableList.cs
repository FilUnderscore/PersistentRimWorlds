using System.Collections.Generic;
using Verse;

namespace PersistentWorlds.Utils
{
    public class ExposableList<T> : IExposable
    {
        private List<T> list = new List<T>();
        
        public void ExposeData()
        {
            Scribe_Collections.Look<T>(ref list, "list", LookMode.Deep);
        }

        public List<T> GetList()
        {
            return this.list;
        }
    }
}