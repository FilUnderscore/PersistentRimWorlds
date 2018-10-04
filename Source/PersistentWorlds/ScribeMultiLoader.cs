using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using RimWorld;
using Verse;

namespace PersistentWorlds
{
    public class ScribeMultiLoader
    {
        // Assembles all files into one large element structure.

        public Dictionary<string, XmlNode> xmlParents = new Dictionary<string, XmlNode>();
        public CrossRefHandler CrossRefHandler = new CrossRefHandler();
        public PostLoadIniter PostIniter = new PostLoadIniter();
        public XmlNode curXmlNode;

        public void InitLoading(string[] filePaths)
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
                    using (StreamReader streamReader = new StreamReader(filePath))
                    {
                        using (XmlTextReader xmlTextReader = new XmlTextReader(streamReader))
                        {
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.Load(xmlTextReader);
                            this.xmlParents.Add(filePath, xmlDocument.DocumentElement);
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

        public void SetScribeCurXmlParentByFilePath(string filePath)
        {
            Scribe.loader.curXmlParent = xmlParents[filePath];
            Scribe.loader.curPathRelToParent = null;
            this.curXmlNode = Scribe.loader.curXmlParent;
            
            Log.Message("Set Scribe.loader.curXmlParent to " + filePath);
        }
    }
}