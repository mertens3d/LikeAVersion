using log4net;
using mertens3d.LikeAVersion;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.Toucher;
using mertense3d.LikeAVersion;
using System;
using System.Collections.Generic;

namespace mertensd.LikeAVersion.Watcher
{
    public class Watcher
    {
        #region Fields

        private ILog _log;

        #endregion Fields

        #region Constructors

        public Watcher(AssemToucher toucher, Reporter reporter, ILog log)
        {
            Toucher = toucher;
            _log = log;
            Reporter = reporter;
        }

        #endregion Constructors

        #region Properties

        public bool ActiveWatchIsOn { get; set; } = true;

        public List<OneTargetedDirectoryOfTypeMonitor> Watchers { get; set; } = new List<OneTargetedDirectoryOfTypeMonitor>();
        private Reporter Reporter { get; }
        private AssemToucher Toucher { get; }

        #endregion Properties

        #region Methods

        public void buildFileWatchForAllInSln(CsProjArray SlnProjObj)
        {
            _log.Debug("s) buildFileWatchForAllInSln");
            var toReturn = new List<string>();

            _log.Debug("\t SlnProjObj.SlnData.Projects.count: " + SlnProjObj.SlnData.WatchedProjects.Count);

            foreach (var oneProject in SlnProjObj.SlnData.WatchedProjects)
            {
                foreach (var oneTargetType in SlnProjObj.AllTargets.TargetFileTypes)
                {
                    var watcher = new OneTargetedDirectoryOfTypeMonitor(SlnProjObj.AllTargets.ExcludedFiles, Reporter);
                    watcher.OnFileChanged += ChangedFileHandler;
                    watcher.MonitorDirectory(oneProject, oneTargetType);
                    Watchers.Add(watcher);
                }
            }
        }

        public void ChangedFileHandler(object sender, ChangedProjectEventArgs args)
        {
            if (args != null && ActiveWatchIsOn)
            {
                ActiveWatchIsOn = false;
                _log.Debug("Child file changed: " + args.changedProject.ProjName);

                var changedProj = args.changedProject;

                if (changedProj != null)
                {
                    Toucher.AssemblyInfoFileHandler.changeHandleOneProject(changedProj, 0);
                }

                if (changedProj?.UpsteamProjects != null)
                {
                    var now = DateTime.Now;
                    foreach (var oneProj in changedProj.UpsteamProjects)
                    {
                        var diff = now.Subtract(oneProj.ProjData.LastAssemblyWrite).TotalMilliseconds;

                        if (diff > oneProj.ProjData.MinSpan)
                        {
                            Toucher.AssemblyInfoFileHandler.changeHandleOneProject(oneProj.ProjData, oneProj.Depth + 1);
                        }
                    }
                }
            }
            ActiveWatchIsOn = true;
            _log.Debug("e) File changed: ");
            Reporter.ToHuman("Completed File Change");
        }

        #endregion Methods
    }
}