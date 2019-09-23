using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeAVersion
{
   public static class FileTools
    {
        public static string ReadFileAsChuck(FileInfo fileInfo)
        {
            var toReturn = string.Empty;

            if (fileInfo.Exists)
            {
                toReturn = File.ReadAllText(fileInfo.FullName);
            }
            return toReturn;
        }
        public static List<string> ReadFileSync(FileInfo fileInfo, ReturnState returnState)
        {
            var toReturn = new List<string>();
            var dirtyList = new List<string>();
            if (fileInfo.Exists)
            {
                dirtyList = System.IO.File.ReadAllLines(fileInfo.FullName).ToList();
            }

            foreach (var oneLine in dirtyList)
            {
                var newLine = oneLine;
                if(returnState == ReturnState.Clean)
                {
                    newLine = newLine.Trim();

                }
                toReturn.Add(newLine);
            }

            return toReturn;
        }

        internal static void WriteChunckToFile(FileInfo assemblyInfoFile, string fileContents)
        {
           if(assemblyInfoFile.Exists && fileContents.Length > 10)
            {
                File.WriteAllText(assemblyInfoFile.FullName, fileContents);
            }
        }
    }
}
