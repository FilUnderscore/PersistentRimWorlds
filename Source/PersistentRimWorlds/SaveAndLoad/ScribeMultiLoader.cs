using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using RimWorld;
using Verse;

namespace PersistentWorlds
{
    // Allows the loading of different files by sharing Cross-References of objects/pawns/things in the game.
    public sealed class ScribeMultiLoader
    { 
        public static Dictionary<string, XmlNode> xmlParents = new Dictionary<string, XmlNode>();
        public static XmlNode curXmlNode;

        public static void InitLoading(string[] filePaths)
        {
            if (Scribe.mode != LoadSaveMode.Inactive)
            {
                Log.Error("Called InitLoading() but current mode is " + Scribe.mode.ToString(), false);
                Scribe.ForceStop();
            }
            
            try
            {
                foreach (var filePath in filePaths)
                {
                    if (xmlParents.ContainsKey(filePath)) continue;
                    
                    using (var streamReader = new StreamReader(filePath))
                    {
                        using (var xmlTextReader = new XmlTextReader(streamReader))
                        {
                            var xmlDocument = new XmlDocument();
                            xmlDocument.Load(xmlTextReader);
                            xmlParents.Add(filePath, xmlDocument.DocumentElement);
                        }
                    }
                }

                Scribe.mode = LoadSaveMode.LoadingVars;
            }
            catch (Exception e)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Exception while init loading files: ",
                    filePaths.ToCommaList(),
                    "\n",
                    e
                }), false);
                
                throw;
            }
        }

        public static void SetScribeCurXmlParentByFilePath(string filePath)
        {
            Scribe.loader.curXmlParent = xmlParents[filePath];
            Scribe.loader.curPathRelToParent = null;
            curXmlNode = Scribe.loader.curXmlParent;
            
            Log.Message("Set Scribe.loader.curXmlParent to " + filePath);
        }

        public static void Clear()
        {
            xmlParents.Clear();
            curXmlNode = null;
        }
    }
}