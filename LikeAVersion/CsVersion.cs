using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LikeAVersion
{
    public class CsVersion
    {
        #region Fields

        private ILog _log;

        private Watcher _watcher;

        #endregion Fields

        #region Properties

        public string __dirname { get; private set; }

        public CsProjArray SlnProjObj { get; set; }

        //public Watcher Watcher
        //{
        //    get
        //    {
        //        if (_watcher == null)
        //        {
        //            _watcher = new Watcher();
        //            _watcher.OnFileChanged += ChangedHandler;
        //        }
        //        return _watcher;
        //    }
        //}

        //    }
        //}
        public List<Watcher> Watchers { get; set; } = new List<Watcher>();

        #endregion Properties

        #region Methods

        //public void gulpWatch(List<OneWatchedFile> filesToWatchAr, ProjectData projectData)
        //{
        //    foreach (var file in filesToWatchAr)
        //    {
        //        var watcher = new Watcher();
        //        watcher.OnFileChanged += ChangedHandler;
        //        watcher.MonitorDirectory(projectData, file.WatchRoot, file.Filter);
        //        Watchers.Add(watcher);
        public void buildFileWatchForAllInSln()
        {
            _log.Debug("s) buildFileWatchForAllInSln");
            var toReturn = new List<string>();

            _log.Debug("\t SlnProjObj.SlnData.Projects.count: " + SlnProjObj.SlnData.Projects.Count);

            foreach (var oneProject in SlnProjObj.SlnData.Projects)
            {
                //var filesToWatchAr = BuildFileArrayToWatch(oneProject);

                foreach (var oneTargetType in SlnProjObj.AllTargets.TargetFileTypes)
                {
                    var watcher = new Watcher(SlnProjObj.AllTargets.ExcludedFiles);
                    watcher.OnFileChanged += ChangedHandler;
                    watcher.MonitorDirectory(oneProject, oneTargetType);
                    Watchers.Add(watcher);
                }
            }
        }

        public DirectoryInfo CalculateProjectRoot(string relativePath)
        {
            _log.Debug("s) Calculate Project Root Folder");
            DirectoryInfo toReturn = null;
            var firstFolder = "";
            var asArray = relativePath.Split('\\').ToArray();
            for (var idx = 0; idx < asArray.Count(); idx++)
            {
                var candidateFolder = asArray[idx];
                if (candidateFolder.Count() > 0)
                {
                    firstFolder = candidateFolder;
                    break;
                }
            }

            _log.Debug("\tFirst Folder: " + firstFolder);

            if (SlnProjObj.AllTargets.TargetProjectNames.IndexOf(firstFolder) > -1)
            {
                toReturn = new DirectoryInfo(Path.Combine(__dirname, firstFolder));
            }
            else
            {
                _log.Error("found root folder not in targets");
            }

            _log.Debug("e) Calculate Project Root Folder. Returning: " + toReturn);
            return toReturn;
        }

        public void ChangedHandler(object sender, ChangedProjectEventArgs e)
        {
            if (e != null)
            {
                _log.Debug("Child file changed: " + e.changedProject.projName);

                var parentCsProj = e.changedProject.CsProjFile;
                var changedProj = e.changedProject;
                //Console.WriteLine("Changed Proj " + changedProj.projName);

                if (parentCsProj != null)
                {
                    var upstreamProjects = GetUpstreamProjectsB(changedProj);
                    if (!upstreamProjects.Contains(changedProj))
                    {
                        upstreamProjects.Add(changedProj);
                    }
                    // var upstreamProjects = SlnProjObj.GetUpStreamProjects(e.changedProject);

                    var assemblyPrefix = "assembly: AssemblyVersion";

                    var regexPat = assemblyPrefix + ".*";

                    var newText = assemblyPrefix + "(\"" + GetSuffix() + "\")] //this file auto changed by: " + System.Reflection.Assembly.GetExecutingAssembly().Location;

                    if (upstreamProjects != null)
                    {
                        foreach (var oneProj in upstreamProjects)
                        {
                            var assemblyInfo = oneProj.AssemblyInfoFile;

                            var now = DateTime.Now;
                            var diff = now.Subtract(oneProj.LastAssemblyWrite).TotalMilliseconds;

                            if (diff > oneProj.MinSpan)
                            {
                                if (assemblyInfo != null)
                                {
                                    UpdateAssemblyInfo(assemblyInfo, regexPat, newText);
                                    oneProj.LastAssemblyWrite = now;
                                }
                                else
                                {
                                    Console.WriteLine("Skipping (not enough time diff) :" + oneProj.projName);
                                }
                            }
                        }
                    }
                }
            }
            _log.Debug("e) File changed: ");
            Console.WriteLine("Done");
        }

        public string GetRelativeProjectPath(string csFilePath)
        {
            //_log.Debug("s) GetRelativeProjectPath for: " + csFilePath);

            //we need to walk up the tree to find the assemblyInfo.cs
            //but first we may be at the root of the project
            //so it may be down

            //_log.Debug("\tCurrent: " + csFilePath);
            var parent = Path.Combine(csFilePath, "../");
            //_log.Debug("\tParent: " + parent);
            //_log.Debug("\t__dir  " + __dirname);

            var relativeProjectPath = "todo";
            //todo   var relativeProjectPath = parent.Replace(SlnProjObj.__dirname, "");

            //we want subtract the root path

            //_log.Debug("e) GetRelativeProjectPath Returning: " + relativeProjectPath);
            return relativeProjectPath;
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

        public List<ProjectData> GetUpstreamProjectsB(ProjectData rootProj)
        {
            var toReturn = new List<ProjectData>();

            foreach (var oneRevRef in rootProj.ReverseRef)
            {
                if (!toReturn.Contains(oneRevRef))
                {
                    toReturn.Add(oneRevRef);

                    var ancestors = GetUpstreamProjectsB(oneRevRef);
                    foreach (var oneAncestor in ancestors)
                    {
                        if (!toReturn.Contains(oneAncestor))
                        {
                            toReturn.Add(oneAncestor);
                        }
                    }
                }
            }

            return toReturn;
        }

        public void ModifyAssemblyInfoFile(FileInfo assemblyInfoFile, string regexPat, string newText)
        {
            _log.Debug("s) ModifyAssemblyInfoFile (" + assemblyInfoFile + ")");

            var fileContents = FileTools.ReadFileAsChuck(assemblyInfoFile);

            var origLength = fileContents.Length;

            fileContents = Regex.Replace(fileContents, regexPat, newText);

            var replaceLength = fileContents.Length;

            if (Math.Abs(origLength - replaceLength) < 50)
            {
                Console.WriteLine("Modifying: " + assemblyInfoFile.FullName);
                FileTools.WriteChunckToFile(assemblyInfoFile, fileContents);
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

        public void startProjWatch()
        {
            _log.Debug("s) Init Project Watch. Count = " + SlnProjObj.AllTargets.TargetProjectNames.Count);
            for (var projIdx = 0; projIdx < SlnProjObj.AllTargets.TargetProjectNames.Count; projIdx++)
            {
                //_log.Debug(projIdx);
                var oneProject = SlnProjObj.AllTargets.TargetProjectNames[projIdx];
                _log.Debug("Init Project: " + projIdx + ":" + (SlnProjObj.AllTargets.TargetProjectNames.Count - 1) + "  " + oneProject);
                //public void watchProj" + projIdx, public void () {
                //    gulp.watch(solutionRoot + "/**/*.cs", ["csWatch"]);
                //});
            }
        }

        public void Stream(ILog log)
        {
            // Endless stream mode
            //return
            _log = log;
            SlnProjObj = new CsProjArray();
            SlnProjObj.Init(_log);
            buildFileWatchForAllInSln();

            var input = Console.ReadLine();
        }

        public void UpdateAssemblyInfo(FileInfo assemblyFile, string regexPat, string newText)
        {
            _log.Debug("s) UpdateAssemblyInfo (" + assemblyFile + ")");

            if (assemblyFile != null)
            {
                ModifyAssemblyInfoFile(assemblyFile, regexPat, newText);
            }
            else
            {
                _log.Debug("Candidate AssemblyInfo NOT found");
            }
        }

        public void UpdateBuildNumber(List<OneTargetFile> allTargetFiles)
        {
            var nowDate = DateTime.Now;
            var Year = nowDate.Year;
            var Month = nowDate.Month;// ("0" + (nowDate.getMonth() + 1)).slice(-2);
            var Day = nowDate.Day;// ("0" + (nowDate.getDate() + 0)).slice(-2);
            var Hour = nowDate.Hour;// ("0" + (nowDate.getHours() + 0)).slice(-2);
            var Min = nowDate.Minute;// ("0" + (nowDate.getMinutes() + 0)).slice(-2);
            var Seconds = nowDate.Second;// ("0" + (nowDate.getSeconds() + 0)).slice(-1);

            foreach (var oneTargetFile in allTargetFiles)
            {
                UpdateOneAssemblyFile(oneTargetFile);
            }
        }

        public void UpdateOneAssemblyFile(OneTargetFile oneTargetFile)
        {
            // .pipe(replace(/private const string ReleasePrefix.*/, "private const string ReleasePrefix = "" + Year + "." + Month + "." + Day + "." + Hour + Min + Seconds + "";"))
        }
        //public List<OneWatchedFile> BuildFileArrayToWatch(ProjectData projectData)
        //{
        //    List<OneWatchedFile> filesToWatchAr = new List<OneWatchedFile>();

        //    foreach (var oneTargetType in SlnProjObj.AllTargets.TargetFileTypes)
        //    {
        //        filesToWatchAr.Add(new OneWatchedFile(watchPrefix, oneTargetType));
        //    }

        //    return filesToWatchAr;
        //}

        public FileInfo walk(DirectoryInfo dir, string needleFileName)
        {
            //_log.Debug("s) walk (" + dir + ", " + needleFileName + ")");

            var dirExists = dir.Exists;
            //_log.Debug("\t\tDir Exists ? " + dirExists);
            FileInfo found = null;
            //https://stackoverflow.com/questions/5827612/node-js-fs-readdir-recursive-directory-search
            if (dirExists)
            {
                var dirList = dir.GetFiles();
                var subDirectories = dir.GetDirectories();

                if (dirList != null && dirList.Any())
                {
                    //msb_log.Debug.Show_log.DebugInfo("\t\tDir Count: " + dirList.length);

                    foreach (var oneDirectory in subDirectories)
                    {
                        var result = walk(oneDirectory, needleFileName);
                        if (result != null)
                        {
                            found = result;
                        }
                    }

                    foreach (var oneFile in dirList)
                    {
                        if (oneFile.Name.Equals(needleFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            found = oneFile;
                            _log.Debug("\tfound match: " + found + "   " + oneFile.FullName);
                            break;
                        }
                    }
                }
            }
            return found;
        }

        public string WalkUpToCsProj(string startingDir, bool isIteration, int iteration)
        {
            _log.Debug("s) WalkUpToCsProj (" + iteration + " isIteration: " + isIteration + "  " + startingDir);

            var toReturn = "null";

            if (startingDir != null && File.Exists(startingDir))
            {
                if (!isIteration)
                {
                    iteration = 20;
                    isIteration = true;
                }

                var isDirectory = Directory.Exists(startingDir);

                if (!isDirectory)
                {
                    _log.Debug("\t Is a directory: " + isDirectory);
                    startingDir = "todo";// Path.dirname(startingDir);
                    _log.Debug("\t startingDir now : " + startingDir);
                }

                var targetDirectory = new DirectoryInfo(startingDir);

                if (targetDirectory.Exists)
                {
                    var dirList = targetDirectory.GetFiles();

                    if (dirList != null)
                    {
                        var regEx = new Regex(".*csproj/g");

                        var filesB = targetDirectory.GetFiles("*.csproj");

                        if (filesB == null || !filesB.Any())
                        {
                            if (iteration > 0)
                            {
                                toReturn = WalkUpToCsProj(Path.Combine(startingDir, ".."), isIteration, iteration - 1);
                            }
                            else
                            {
                                _log.Debug("max iteration hit");
                            }
                        }
                        else
                        {
                            if (filesB != null && filesB.Any())
                            {
                                toReturn = filesB[0].FullName;
                                _log.Debug("Found: " + toReturn);
                            }
                        }
                    }
                }
            }
            _log.Debug("e) WalkUpToCsProj " + toReturn);
            return toReturn;
        }

        #endregion Methods
    }
}