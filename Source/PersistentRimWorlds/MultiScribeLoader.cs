using System.IO;
using System.Xml;
using Verse;

namespace PersistentWorlds
{
    public class MultiScribeLoader
    {
        public void InitLoader(params string[] files)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><root></root>");
            
            foreach (var file in files)
            {
                using (var streamReader = new StreamReader(file))
                {
                    using (var xmlTextReader = new XmlTextReader((TextReader) streamReader))
                    {
                        var doc = new XmlDocument();
                        doc.Load((XmlReader) xmlTextReader);

                        xmlDocument.AppendChild(doc.DocumentElement);
                    }
                }
            }

            Scribe.loader.curXmlParent = (XmlNode) xmlDocument.DocumentElement;
            Scribe.mode = LoadSaveMode.LoadingVars;
        }
    }
}