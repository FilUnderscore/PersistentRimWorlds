using System.Xml;
using Verse;

namespace PersistentWorlds.SaveAndLoad
{
    /// <summary>
    /// Handles storing backup information about scribe variables so references don't get lost due to multiple file
    /// manipulation for better efficiency in loading times.
    /// </summary>
    public static class ScribeVars
    {
        #region Fields
        public static LoadSaveMode mode;
        public static IExposable curParent;
        public static XmlNode curXmlParent;
        public static string curPathRelToParent;
        #endregion
        
        #region Methods
        /// <summary>
        /// Set vars in ScribeVars to loader vars.
        /// </summary>
        public static void Set()
        {
            mode = Scribe.mode;
            curParent = Scribe.loader.curParent;
            curXmlParent = Scribe.loader.curXmlParent;
            curPathRelToParent = Scribe.loader.curPathRelToParent;
        }
        
        /// <summary>
        /// Re-set loader vars to ScribeVars vars.
        /// </summary>
        public static void Reset()
        {
            Scribe.mode = mode;
            Scribe.loader.curParent = curParent;
            Scribe.loader.curXmlParent = curXmlParent;
            Scribe.loader.curPathRelToParent = curPathRelToParent;
        }

        /// <summary>
        /// Clear vars stored in ScribeVars.
        /// </summary>
        public static void Clear()
        {
            mode = LoadSaveMode.Inactive;
            curParent = null;
            curXmlParent = null;
            curPathRelToParent = null;
        }

        /// <summary>
        /// Clear scribe: mode, loader current xml parent, loader current parent, and loader current path relative to
        /// parent, without affecting loader cross references or post load initializer.
        /// </summary>
        public static void TrickScribe()
        {
            Scribe.mode = LoadSaveMode.Inactive;
            Scribe.loader.curXmlParent = null;
            Scribe.loader.curParent = null;
            Scribe.loader.curPathRelToParent = null;
        }
        #endregion
    }
}