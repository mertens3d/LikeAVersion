using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LikeAVersion
{
    public class CsVersion
    {
        #region Fields

        private AssemblyInfoFileHandler _assemblyInfoFileHandler;
        private ILog _log;

        #endregion Fields

        #region Properties

        public string __dirname { get; private set; }

        public AssemblyInfoFileHandler AssemblyInfoFileHandler
        {
            get
            {
                return _assemblyInfoFileHandler ?? (_assemblyInfoFileHandler = new AssemblyInfoFileHandler(_log));
            }
        }

        public CsProjArray SlnProjObj { get; set; }

        public List<OneTargetedDirectoryOfTypeMonitor> Watchers { get; set; } = new List<OneTargetedDirectoryOfTypeMonitor>();
        public bool ActiveWatchIsOn { get; set; } = true;

        #endregion Properties

        #region Methods

        public void buildFileWatchForAllInSln()
        {
            _log.Debug("s) buildFileWatchForAllInSln");
            var toReturn = new List<string>();

            _log.Debug("\t SlnProjObj.SlnData.Projects.count: " + SlnProjObj.SlnData.WatchedProjects.Count);

            foreach (var oneProject in SlnProjObj.SlnData.WatchedProjects)
            {
                foreach (var oneTargetType in SlnProjObj.AllTargets.TargetFileTypes)
                {
                    var watcher = new OneTargetedDirectoryOfTypeMonitor(SlnProjObj.AllTargets.ExcludedFiles);
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
            if (e != null && ActiveWatchIsOn)
            {
                ActiveWatchIsOn = false;
                _log.Debug("Child file changed: " + e.changedProject.projName);

                var changedProj = e.changedProject;

                if (changedProj != null)
                {
                    if (changedProj.UpsteamProjects == null)
                    {
                        changedProj.UpsteamProjects = GetUpstreamProjectsB(changedProj);
                    }

                    if (changedProj.UpsteamProjects != null)
                    {
                        var now = DateTime.Now;
                        foreach (var oneProj in changedProj.UpsteamProjects)
                        {
                            var diff = now.Subtract(oneProj.LastAssemblyWrite).TotalMilliseconds;

                            if (diff > oneProj.MinSpan)
                            {
                                AssemblyInfoFileHandler.changeHandleOneProject(oneProj);
                            }
                        }
                    }
                }
            }
            ActiveWatchIsOn = true;
            _log.Debug("e) File changed: ");
            HumanFeedback.ToHuman("Completed File Change");
        }

        public List<ProjectData> GetUpstreamProjectsB(ProjectData rootProj)
        {
            var toReturn = new List<ProjectData>() {
                rootProj
            };

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

        public void StartProjWatch()
        {
            _log.Debug("s) Init Project Watch. Count = " + SlnProjObj.AllTargets.TargetProjectNames.Count);
            for (var projIdx = 0; projIdx < SlnProjObj.AllTargets.TargetProjectNames.Count; projIdx++)
            {
                var oneProject = SlnProjObj.AllTargets.TargetProjectNames[projIdx];
                _log.Debug("Init Project: " + projIdx + ":" + (SlnProjObj.AllTargets.TargetProjectNames.Count - 1) + "  " + oneProject);
            }
        }

        public void Stream(ILog log)
        {
            _log = log;
            SlnProjObj = new CsProjArray();
            SlnProjObj.Init(_log);
            buildFileWatchForAllInSln();
            ListWatched();
            var input = string.Empty;

            while (!input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                HumanFeedback.WriteMenu();
                input = Console.ReadLine();
                if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    TouchAllAssemblyFiles();
                }
                else if (input.Equals("W", StringComparison.OrdinalIgnoreCase))
                {
                    ListWatched();
                }
                else if (input.Equals("i", StringComparison.OrdinalIgnoreCase))
                {
                    ListIgnored();
                }
            }
        }

        private void ListIgnored()
        {
            if (SlnProjObj?.SlnData?.IgnoredProjects != null && SlnProjObj.SlnData.IgnoredProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.IgnoredProjects)
                {
                    HumanFeedback.ToHuman("Ignoring Project: " + oneSlnProject.CsProjFile.Directory.FullName);// + " filter: " + filter);
                }
            }
        }

        private void ListWatched()
        {
            if (SlnProjObj?.SlnData?.WatchedProjects != null && SlnProjObj.SlnData.WatchedProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.WatchedProjects)
                {
                    HumanFeedback.ToHuman("Watching Project: " + oneSlnProject.CsProjFile.Directory.FullName);// + " filter: " + filter);
                }
            }
        }

        public void TouchAllAssemblyFiles()
        {
            _log.Debug("Touch All");

            if (SlnProjObj?.SlnData?.WatchedProjects != null && SlnProjObj.SlnData.WatchedProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.WatchedProjects)
                {
                    if (oneSlnProject != null)
                    {
                        if (oneSlnProject.UpsteamProjects == null)
                        {
                            oneSlnProject.UpsteamProjects = GetUpstreamProjectsB(oneSlnProject);
                        }

                        if (oneSlnProject.UpsteamProjects != null)
                        {
                            var now = DateTime.Now;
                            foreach (var oneProj in oneSlnProject.UpsteamProjects)
                            {
                                var diff = now.Subtract(oneProj.LastAssemblyWrite).TotalMilliseconds;

                                if (diff > oneProj.MinSpan)
                                {
                                    AssemblyInfoFileHandler.changeHandleOneProject(oneProj);
                                }
                            }
                        }
                    }
                }
            }
            HumanFeedback.ToHuman("Completed Touch All");
            HumanFeedback.WriteMenu();
        }

        #endregion Methods
    }
}