using mertensd.LikeAVersion.Build;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.FileTools;
using mertensd.LikeAVersion.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace mertensd.LikeAVersion.Toucher
{
    public class AssemblyInfoFileHandler : CommonBase_a
    {
        public AssemblyInfoFileHandler(HeartBeatHub hub) : base(hub)
        {
        }

        #region Methods

        public void UpdateOneProject(ProjectData oneProj, int depth)
        {
            var assemblyInfo = oneProj.AssemblyInfoFile;
            if (assemblyInfo != null)
            {
                var newText = Constants.assemblyPrefix + "(\"" + GetSuffix()
                    + Constants.AssemblyFileComment
                    + System.Reflection.Assembly.GetExecutingAssembly().Location;

                var regexPat = Constants.assemblyPrefix + ".*";

                Hub.Logger.Debug("s) UpdateAssemblyInfo (" + assemblyInfo.ToString() + ")");

                if (assemblyInfo != null)
                {
                    ModifyAssemblyInfoFile(assemblyInfo, regexPat, newText, oneProj, depth);
                }
                else
                {
                    Hub.Logger.Debug("Candidate AssemblyInfo NOT found");
                }

                oneProj.LastAssemblyWrite = DateTime.Now;
            }
            else
            {
                Hub.Reporter.Error("Null :" + oneProj.ProjName);
            }
        }

        public string GetSuffix()
        {
            var nowDate = DateTime.Now;
            var Year = nowDate.Year;
            var Month = PadWithZero(nowDate.Month + 0);
            var Day = PadWithZero(nowDate.Day + 0);
            var Hour = PadWithZero(nowDate.Hour + 0);
            var Min = PadWithZero(nowDate.Minute + 0);
            var Seconds = PadWithZero(nowDate.Second + 0);

            return Year + "." + Month + "." + Day + "." + (Hour + Min + Seconds).Substring(0, 5);
        }

        public void ModifyAssemblyInfoFile(FileInfo assemblyInfoFile, string regexPat, string newText, ProjectData oneProj, int depth)
        {
            Hub.Logger.Debug("s) ModifyAssemblyInfoFile (" + assemblyInfoFile + ")");

            var fileContents = FileIO.ReadFileAsChuck(assemblyInfoFile);

            var origLength = fileContents.Length;

            fileContents = Regex.Replace(fileContents, regexPat, newText);

            var replaceLength = fileContents.Length;

            var maxFileLengthChange = 50;
            var delta = Math.Abs(origLength - replaceLength);

            if (delta < maxFileLengthChange)
            {
                string indent = "\t\t";
                for (int idx = 0; idx < depth; idx++)
                {
                    indent += "\t";
                }

                var modifyMessage = indent;
                if (depth == 0)
                {
                    modifyMessage += "Modifying owner AssemblyInfo.cs for: ";
                }
                else
                {
                    modifyMessage += "Modifying upstream AssemblyInfo.cs for: ";
                }

                modifyMessage += oneProj.ProjName;
                Hub.Reporter.ToHuman(modifyMessage);

                FileIO.WriteChunckToFile(assemblyInfoFile, fileContents);
            }
            else
            {
                Hub.Reporter.ToHuman("\t\t***   AssemInfo not configured for: " + delta + " vs " + maxFileLengthChange + " " + oneProj.ProjName + "   ***");// assemblyInfoFile.FullName);
            }
        }

        public string PadWithZero(int input)
        {
            return PadWithZero(input.ToString());
        }

        public string PadWithZero(string input)
        {
            if (input.Length < 2)
            {
                input = "0" + input;
            }
            return input;
        }

        #endregion Methods
    }
}