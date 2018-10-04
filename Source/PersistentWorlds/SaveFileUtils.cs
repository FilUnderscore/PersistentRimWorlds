﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Harmony;
using PersistentWorlds.UI;
using UnityEngine;
using Verse;

namespace PersistentWorlds
{
    public static class SaveFileUtils
    {
        public static bool HasPossibleSameWorldName(ScrollableListItem[] items, string filePath)
        {
            var names = items.Select(item => item.Text).ToArray();

            return HasPossibleSameWorldName(names, filePath);
        }
        
        public static bool HasPossibleSameWorldName(string[] names, string filePath)
        {
            var worldName = "";
            
            Scribe.loader.InitLoading(filePath);
            
            if (Scribe.EnterNode("game"))
            {
                if (Scribe.EnterNode("world"))
                {
                    if (Scribe.EnterNode("info"))
                    {
                        Scribe_Values.Look<string>(ref worldName, "name");
                    }
                }
            }
            
            Scribe.loader.ForceStop();
            
            return names.Any(name => worldName.EqualsIgnoreCase(name));
        }
    }
}