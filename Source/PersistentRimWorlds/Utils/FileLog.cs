using System.IO;
using System.Reflection;

namespace PersistentWorlds.Utils
{
    public static class FileLog
    {
        #region Fields
        #if DEBUG
        private static StreamWriter _logWriter;
        #endif    
        #endregion
        
        #region Methods
        public static void StartLoggingToFile(string filePath)
        {
            #if DEBUG
            _logWriter = new StreamWriter(filePath, false);
            Log($"Persistent RimWorlds Debug Log (AssemblyVersion={Assembly.GetExecutingAssembly().GetName().Version}");
            #endif
        }

        public static void Log(string text)
        {
            #if DEBUG
            _logWriter.Write(text + "\n");
            _logWriter.Flush();
            #endif
        }

        public static void Close()
        {
            #if DEBUG
            _logWriter.Close();
            #endif
        }
        #endregion
    }
}