using log4net;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace LikeAVersion
{
    public class AssemblyInfoFileHandler
    {
        private ILog _log;

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

        public AssemblyInfoFileHandler(ILog log)
        {
            _log = log;
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

        public void changeHandleOneProject(ProjectData oneProj)
        {
            var assemblyInfo = oneProj.AssemblyInfoFile;
            if (assemblyInfo != null)
            {
                var newText = Constants.assemblyPrefix + "(\"" + GetSuffix() + "\")] //this file auto changed by: " + System.Reflection.Assembly.GetExecutingAssembly().Location;
                var regexPat = Constants.assemblyPrefix + ".*";

                _log.Debug("s) UpdateAssemblyInfo (" + assemblyInfo.ToString() + ")");

                if (assemblyInfo != null)
                {
                    ModifyAssemblyInfoFile(assemblyInfo, regexPat, newText, oneProj);
                }
                else
                {
                    _log.Debug("Candidate AssemblyInfo NOT found");
                }

                oneProj.LastAssemblyWrite = DateTime.Now;
            }
            else
            {
                HumanFeedback.ToHuman("Skipping (not enough time diff) :" + oneProj.projName);
            }
        }

        public void ModifyAssemblyInfoFile(FileInfo assemblyInfoFile, string regexPat, string newText, ProjectData oneProj)
        {
            _log.Debug("s) ModifyAssemblyInfoFile (" + assemblyInfoFile + ")");

            var fileContents = FileTools.ReadFileAsChuck(assemblyInfoFile);

            var origLength = fileContents.Length;

            fileContents = Regex.Replace(fileContents, regexPat, newText);

            var replaceLength = fileContents.Length;

            var maxFileLengthChange = 50;
            var delta = Math.Abs(origLength - replaceLength);

            if (delta < maxFileLengthChange)
            {
                HumanFeedback.ToHuman("\tModifying AssemInfo for: " + oneProj.projName);// assemblyInfoFile.FullName);
                FileTools.WriteChunckToFile(assemblyInfoFile, fileContents);
            }
            else
            {
                HumanFeedback.ToHuman("\t\t***   AssemInfo not configured for: " + delta + " vs " + maxFileLengthChange + " " + oneProj.projName + "   ***");// assemblyInfoFile.FullName);
            }
        }
    }
}