using System;
using System.Collections.Generic;
using System.Linq;
using PersistentWorlds.Logic;
using Verse;

namespace PersistentWorlds.UI
{
    public abstract class ColonySortOption
    {
        public static readonly ColonySortOption ID = new IDColonySortOption();
        public static readonly ColonySortOption LastWriteTime = new LastWriteTimeColonySortOption();
        
        public static IEnumerable<ColonySortOption> Values
        {
            get
            {
                yield return ID;
                yield return LastWriteTime;
            }
        }

        public readonly string Id;
        public readonly string Name;

        private ColonySortOption(string id)
        {
            this.Id = id;
            this.Name = $"FilUnderscore.PersistentRimWorlds.Misc.Sort.{id}".Translate();
        }

        public abstract void Sort(ref List<PersistentColony> colonies);

        private class IDColonySortOption : ColonySortOption
        {
            public IDColonySortOption() : base("ID")
            {
                
            }

            public override void Sort(ref List<PersistentColony> colonies)
            {
                colonies.Sort((x, y) => x.ColonyData.UniqueId.CompareTo(y.ColonyData.UniqueId));
            }
        }

        private class LastWriteTimeColonySortOption : ColonySortOption
        {
            public LastWriteTimeColonySortOption() : base("LastWriteTime")
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
        
        public static ColonySortOption FindSortOptionById(string id)
        {
            foreach (var colonySortOption in Values)
            {
                if (colonySortOption.Id == id)
                {
                    return colonySortOption;
                }
            }

            Log.Error($"ID {id} does not exist as a {nameof(ColonySortOption)}. Using default.");
            
            return Values.First();
        }
    }
}