using System.IO;
using System.Reflection;

namespace PersistentWorlds.Utils
{
    public static class FileLog
    {
        #region Fields
        #if DEBUG
        private static StreamWriter LogWriter;
        #endif    
        #endregion
        
        #region Methods
        public static void StartLoggingToFile(string filePath)
        {
            #if DEBUG
            LogWriter = new StreamWriter(filePath, false);
            Log("Persistent RimWorlds Debug Log (AssemblyVersion=" + Assembly.GetExecutingAssembly().GetName().Version + ")");
            #endif
        }

        public static void Log(string text)
        {
            #if DEBUG
            LogWriter.Write(text + "\n");
            LogWriter.Flush();
            #endif
        }

        public static void Close()
        {
            #if DEBUG
            LogWriter.Close();
            #endif
        }
        #endregion
    }
}