using System.Xml;
using Verse;

namespace PersistentWorlds
{
    public class ScribeVars
    {
        public static LoadSaveMode mode;
        public static IExposable curParent;
        public static XmlNode curXmlParent;
        public static string curPathRelToParent;

        public static void Reset()
        {
            Scribe.mode = mode;
            Scribe.loader.curParent = curParent;
            Scribe.loader.curXmlParent = curXmlParent;
            Scribe.loader.curPathRelToParent = curPathRelToParent;
        }
    }
}