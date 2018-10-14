using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(ScribeSaver), "EnterNode")]
    public class ScribeSaver_EnterNode_Patch
    {
        #region Fields
        private static readonly FieldInfo WriterField = AccessTools.Field(typeof(ScribeSaver), "writer");

        private static readonly FieldInfo CurPathField = AccessTools.Field(typeof(ScribeSaver), "curPath");

        /// <summary>
        /// Keeps track of the current list index in the current list when saving. Used for ReferenceTable.
        /// </summary>
        private static readonly Dictionary<int, int> listIndexes = new Dictionary<int, int>();

        private static int currentThingIndex;
        #endregion

        public static int GetIndexInList(string curPath, string nodeName)
        {
            var currentListIndex = Regex.Matches(curPath + "/", "\\/li\\[(\\d+)\\]\\/").Count;

            if (nodeName == "li")
            {
                ++currentListIndex;
            }
            
            return listIndexes[currentListIndex];
        }

        public static int GetThingIndex()
        {
            return currentThingIndex;
        }
        
        public static void ResetThingIndex()
        {
            currentThingIndex = 0;
        }
        
        #region Methods
        static bool Prefix(ScribeSaver __instance, string nodeName)
        {
            if (!PersistentWorldManager.GetInstance().PersistentWorldNotNull())
                return true;
            
            var writer = WriterField.GetValue(__instance);
            
            if (writer == null) return true;
            
            var curPath = (string) CurPathField.GetValue(__instance);

            curPath = curPath + "/" + nodeName;

            var currentListIndex = Regex.Matches(curPath + "/", "\\/li\\[(\\d+)\\]\\/").Count;

            if (nodeName == "li")
            {
                ++currentListIndex;
            }
            
            Debug.FileLog.Log("Current list index before: " + currentListIndex);
            Debug.FileLog.Log("Curpath before: " + curPath);
            
            if (nodeName == "li" || nodeName == "thing")
            {   
                if (nodeName == "li")
                {
                    if (listIndexes.ContainsKey(currentListIndex))
                        listIndexes[currentListIndex] += 1;
                    else
                    {
                        listIndexes.Add(currentListIndex, 0);
                    }
                    
                    Debug.FileLog.Log("CurPath b4 set: " + curPath);
                    curPath = curPath + "[" + listIndexes[currentListIndex] + "]";
                    Debug.FileLog.Log("CurPath after set: " + curPath);
                }
                else
                {
                    curPath = curPath + "[" + currentThingIndex++ + "]";
                }
            }
            else
            {
                if (currentListIndex < listIndexes.Count)
                {
                    listIndexes.RemoveAll(set => set.Key > currentListIndex);
                }
            }
            
            Debug.FileLog.Log("Current list index after: " + currentListIndex);
            Debug.FileLog.Log("Curpath after: " + curPath);
                
            CurPathField.SetValue(__instance, curPath);

            return true;
        }
        #endregion
    }
}