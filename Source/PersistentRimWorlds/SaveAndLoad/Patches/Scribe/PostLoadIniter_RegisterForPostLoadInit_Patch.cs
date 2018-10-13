using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Harmony;
using RimWorld;
using Verse;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(PostLoadIniter), "RegisterForPostLoadInit")]
    public class PostLoadIniter_RegisterForPostLoadInit_Patch
    {
        /// <summary>
        /// Keeps track of the current list index in the current list when loading. Used for ReferenceTable.
        /// </summary>
        private static readonly Dictionary<int, int> listIndexes = new Dictionary<int, int>();
        
        private static int currentThingIndex = 0;

        public static int GetIndexInList(string path, string nodeName)
        {
            var currentListIndex = Regex.Matches(path + "/", "\\/li\\[(\\d+)\\]\\/").Count;

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
        
        static void Postfix(PostLoadIniter __instance, IExposable s)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars || PersistentWorldManager.WorldLoadSaver == null || PersistentWorldManager.WorldLoadSaver.Status == PersistentWorldLoadSaver.PersistentWorldLoadStatus.Converting) return;

            if (s != null && s is ILoadReferenceable referenceable)
            {
                // Decreases load times.
                if (referenceable is Thing && !(referenceable is Pawn) && Scribe.loader.curXmlParent.HasChildNodes && Scribe.loader.curXmlParent.ChildNodes[0].Name == "thing")
                {
                    return;
                }
                
                var path = FindParent(Scribe.loader.curXmlParent);
                Debug.FileLog.Log("Path: " + path);

                var pathToLoad = "";
                var label = "";
                
                if (Scribe.loader.curXmlParent.HasChildNodes)
                {
                    label = Scribe.loader.curXmlParent.ChildNodes[0].Name;
                    
                    var currentListIndex = Regex.Matches(path + "/", "\\/li\\[(\\d+)\\]\\/").Count;

                    if (Scribe.loader.curXmlParent.ChildNodes[0].Name == "li")
                    {
                        ++currentListIndex;
                    }
                    
                    if (Scribe.loader.curXmlParent.ChildNodes[0].Name == "li")
                    {
                        if (listIndexes.ContainsKey(currentListIndex))
                        {
                            listIndexes[currentListIndex] += 1;
                        }
                        else
                        {
                            listIndexes.Add(currentListIndex, 0);
                        }
                        
                        pathToLoad = path + "/" +
                                   Scribe.loader.curXmlParent.ChildNodes[0].Name + "[" + listIndexes[currentListIndex] + "]";
                        
                        Debug.FileLog.Log("Path LI: " + pathToLoad);
                    }
                    else if (Scribe.loader.curXmlParent.ChildNodes[0].Name == "thing")
                    {
                        pathToLoad = path + "/" +
                                   Scribe.loader.curXmlParent.ChildNodes[0].Name + "[" + ++currentThingIndex + "]";
                        
                        Debug.FileLog.Log("Path THING: " + pathToLoad);
                    }
                    else
                    {
                        Debug.FileLog.Log("CurPathRelToParent Child Nodes: " + pathToLoad + "/" + Scribe.loader.curXmlParent.ChildNodes[0].Name);
                    }
                }
                else
                {
                    Debug.FileLog.Log("CurPathRelToParent: " + path);
                }
                
                Debug.FileLog.Log("Adding reference: " + pathToLoad);
                Debug.FileLog.Log("Ref ID: " + referenceable.GetUniqueLoadID());

                if (!PersistentWorldManager.ReferenceTable.ContainsReferenceWithLoadID(referenceable.GetUniqueLoadID()))
                {
                    PersistentWorldManager.ReferenceTable.LoadReferenceIntoMemory(referenceable, pathToLoad);
                }
            }
        }

        public static string FindParent(XmlNode loaderCurXmlParent)
        {
            var path = loaderCurXmlParent.Name;

            while (loaderCurXmlParent.ParentNode != null && loaderCurXmlParent.ParentNode.Name != "#document")
            {
                path = loaderCurXmlParent.ParentNode.Name + "/" + path;
                loaderCurXmlParent = loaderCurXmlParent.ParentNode;
            }

            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            return path;
        }
    }
}