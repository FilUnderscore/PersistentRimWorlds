using System;
using System.Collections.Generic;
using System.Linq;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.UI
{
    public abstract class ColonySortOption
    {
        public static readonly ColonySortOption Id = new IdSortOption();
        public static readonly ColonySortOption LastWriteTime = new LastWriteTimeSortOption();
        
        public static IEnumerable<ColonySortOption> VALUES
        {
            get
            {
                yield return Id;
                yield return LastWriteTime;
            }
        }
        
        public string Name;
        
        public ColonySortOption(string Name)
        {
            this.Name = Name;
        }

        public abstract void Sort(ref List<PersistentColony> colonies);

        private class IdSortOption : ColonySortOption
        {
            public IdSortOption() : base("ID")
            {
                
            }

            public override void Sort(ref List<PersistentColony> colonies)
            {
                colonies.Sort((x, y) => x.ColonyData.UniqueId.CompareTo(y.ColonyData.UniqueId));
            }
        }

        private class LastWriteTimeSortOption : ColonySortOption
        {
            public LastWriteTimeSortOption() : base("Last Write Time")
            {
                
            }

            public override void Sort(ref List<PersistentColony> colonies)
            {
                colonies.Sort((x, y) =>
                {
                    if(x.FileInfo != null && y.FileInfo != null)
                        return y.FileInfo.LastWriteTime.CompareTo(x.FileInfo.LastWriteTime);
                    
                    return 0;
                });
            }
        }
        
        public static ColonySortOption FindSortOptionByName(string name)
        {
            foreach (var colonySortOption in VALUES)
            {
                if (colonySortOption.Name == name)
                {
                    return colonySortOption;
                }
            }

            Log.Error($"Name {name} does not exist as a {nameof(ColonySortOption)}. Using default.");
            
            return VALUES.First();
        }
    }
}