using System.IO;
using System.Reflection;

namespace PersistentWorlds.Utils
{
    public static class FileLog
    {
        #region Fields
        private static StreamWriter LogWriter;
        #endregion
        
        #region Methods
        public static void StartLoggingToFile(string filePath)
        {
            LogWriter = new StreamWriter(filePath, false);
            Log("Persistent RimWorlds Debug Log (AssemblyVersion=" + Assembly.GetExecutingAssembly().GetName().Version + ")");
        }

        public static void Log(string text)
        {
            LogWriter.Write(text + "\n");
            LogWriter.Flush();
        }

        public static void Close()
        {
            LogWriter.Close();
        }
        #endregion
    }
}