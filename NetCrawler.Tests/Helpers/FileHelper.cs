using System;
using System.IO;

namespace NetCrawler.Tests.Examples
{
    public static class FileHelper
    {
        public static string GetSiteFromFile(string file)
        {
            if(!File.Exists(file))
            {
                throw new Exception("Could not create mock from file");
            }

            string readText = File.ReadAllText(file);
            return readText;
        }
    }
}
