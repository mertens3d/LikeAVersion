using mertensd.LikeAVersion.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mertensd.LikeAVersion.FileTools
{
    public static class FileIO
    {
        #region Methods

        public static string ReadFileAsChuck(FileInfo fileInfo)
        {
            var toReturn = string.Empty;

            if (fileInfo.Exists)
            {
                try
                {
                    toReturn = File.ReadAllText(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(fileInfo.FullName);
                    Console.WriteLine(ex.Message);
                }
            }
            return toReturn;
        }

        public static List<string> ReadFileSync(FileInfo fileInfo, ReturnState returnState)
        {
            var toReturn = new List<string>();
            var dirtyList = new List<string>();
            if (fileInfo.Exists)
            {
                dirtyList = File.ReadAllLines(fileInfo.FullName).ToList();
            }

            foreach (var oneLine in dirtyList)
            {
                var newLine = oneLine;
                if (returnState == ReturnState.Clean)
                {
                    newLine = newLine.Trim();
                }
                toReturn.Add(newLine);
            }

            return toReturn;
        }

        internal static void WriteChunckToFile(FileInfo assemblyInfoFile, string fileContents)
        {
            if (assemblyInfoFile.Exists && fileContents.Length > 10)
            {
                try
                {
                    File.WriteAllText(assemblyInfoFile.FullName, fileContents);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(assemblyInfoFile.FullName);
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #endregion Methods
    }
}