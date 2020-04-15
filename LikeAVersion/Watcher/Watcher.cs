using mertens3d.LikeAVersion;
using mertensd.LikeAVersion.Build;
using mertensd.LikeAVersion.Toucher;
using System;
using System.Collections.Generic;

namespace mertensd.LikeAVersion.Watcher
{
    public class Watcher : CommonBase_a
    {
        #region Constructors

        public Watcher(AssemToucher toucher, HeartBeatHub hub) : base(hub)
        {
            Toucher = toucher;
        }

        #endregion Constructors

        #region Properties

        public List<OneTargetedDirectoryOfTypeMonitor> Watchers { get; set; } = new List<OneTargetedDirectoryOfTypeMonitor>();
        private AssemToucher Toucher { get; }

        #endregion Properties

        #region Methods

        public void buildFileWatchForAllInSln(CsProjArray SlnProjObj)
        {
            Hub.Logger.Debug("s) buildFileWatchForAllInSln");
            var toReturn = new List<string>();

            Hub.Logger.Debug("\t SlnProjObj.SlnData.Projects.count: " + SlnProjObj.SlnData.WatchedProjects.Count);

            foreach (var oneProject in SlnProjObj.SlnData.WatchedProjects)
            {
                foreach (var oneTargetType in Hub.XmlData.TargetFileTypes)
                {
                    var dirWatcher = new OneTargetedDirectoryOfTypeMonitor(Hub.XmlData.ExcludedFiles, Hub);
                    dirWatcher.OnFileChanged += OneFileChangedHandler;
                    dirWatcher.MonitorDirectory(oneProject, oneTargetType);
                    Watchers.Add(dirWatcher);
                }
            }
        }

        public void OneFileChangedHandler(object sender, ChangedProjectEventArgs args)
        {
            if (args != null && Hub.WatcherStatus.WatchIsEnabled())
            {
                Hub.WatcherStatus.PauseFileWatch();

                //Hub.Reporter.ToHuman("Child file changed in : " + args.changedProject.ProjName);

                var changedProj = args.changedProject;

                if (changedProj != null)
                {
                    Toucher.AssemblyInfoFileHandler.UpdateOneProject(changedProj, 0);
                }

                if (changedProj?.UpsteamProjects != null)
                {
                    var now = DateTime.Now;
                    foreach (var oneProj in changedProj.UpsteamProjects)
                    {
                        var diff = now.Subtract(oneProj.ProjData.LastAssemblyWrite).TotalMilliseconds;

                        if (diff > Hub.XmlData.MinTimeSpanBetweenProjUpdateMSec)
                        {
                            Toucher.AssemblyInfoFileHandler.UpdateOneProject(oneProj.ProjData, oneProj.Depth + 1);
                        }
                    }
                }
            }
            Hub.WatcherStatus.ResumeFileWatch();

            Hub.Logger.Debug("e) File changed: ");
            //Hub.Reporter.ToHuman("Completed assemblyInfo.cs modifications");
        }

        #endregion Methods
    }
}