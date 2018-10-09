using System;
using System.IO;

namespace PersistentWorlds
{
    public class FileMemoryLoader
    {
        public static string FindInFile(string file, string queryString)
        {
            string fileText = File.ReadAllText(file);

            if (fileText.ToLower().Contains(queryString.ToLower()))
            {
                var index = fileText.ToLower().IndexOf(queryString.ToLower(), StringComparison.Ordinal);
                
            }
        }
    }
}