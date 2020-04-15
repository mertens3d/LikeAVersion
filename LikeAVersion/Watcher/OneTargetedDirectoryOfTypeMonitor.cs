using mertensd.LikeAVersion.Build;
using mertensd.LikeAVersion.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace mertensd.LikeAVersion.Watcher
{
    public class OneTargetedDirectoryOfTypeMonitor : CommonBase_a
    {
        #region Fields

        private List<string> excludedFilesLc = new List<string>();

        #endregion Fields

        #region Constructors

        public OneTargetedDirectoryOfTypeMonitor(List<string> excludedFiles, HeartBeatHub hub) : base(hub)
        {
            excludedFiles.ForEach(x => excludedFilesLc.Add(x.ToLower()));
            //this.excludedFilesLc = excludedFiles;
        }

        #endregion Constructors

        #region Events

        internal event EventHandler<ChangedProjectEventArgs> OnFileChanged;

        #endregion Events

        #region Properties

        public ProjectData ChangedProject { get; set; }
        public List<string> FilesToIgnore { get; set; }
        private FileSystemWatcher filesystemwatcher { get; set; }

        #endregion Properties

        #region Methods

        public void MonitorDirectory(ProjectData projectData, string filter)
        {
            filesystemwatcher = new FileSystemWatcher
            {
                Path = projectData.CsProjFile.Directory.FullName,
                Filter = filter,
                NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };

            filesystemwatcher.Changed += FileSystemWatcher_Changed;
            filesystemwatcher.Renamed += FileSystemWatcher_Changed;

            filesystemwatcher.EnableRaisingEvents = true;
            ChangedProject = projectData;
        }

        private static string GetFileNameFromArgs(FileSystemEventArgs args)
        {
            string toReturn = string.Empty;
            string triggerFullFileName;

            if (args.ChangeType.Equals(WatcherChangeTypes.Renamed))
            {
                var renameArgs = args as RenamedEventArgs;
                if (args != null)
                {
                    triggerFullFileName = renameArgs.OldFullPath;
                    toReturn = new FileInfo(triggerFullFileName).Name;
                }
            }
            else
            {
                var modifiedFile = args.Name;
                triggerFullFileName = string.Empty;// args.FullPath;
                //{
                //    okToProceed = false;
                //}
            }

            return toReturn;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs args)
        {
            bool okToProceed = true;
            string triggerFileName = GetFileNameFromArgs(args);

            okToProceed = okToProceed && !string.IsNullOrEmpty(triggerFileName);
            okToProceed = okToProceed && !excludedFilesLc.Contains(triggerFileName.ToLower());

            if (okToProceed && Hub.WatcherStatus.WatchIsEnabled())
            {
                Hub.Reporter.ToHuman("");
                Hub.Reporter.ToHuman("-------");
                Hub.Reporter.ToHuman("");

                if (OnFileChanged != null)
                {
                    var now = DateTime.Now;
                    var diff = now.Subtract(ChangedProject.LastUpdateTime).TotalMilliseconds;

                    Hub.Reporter.ToHuman("Trigger: " + triggerFileName);
                    Hub.Reporter.ToHuman("Parent Project: " + ChangedProject.ProjName);
                    Hub.Reporter.ToHuman("Elapsed since last update (msec): " + diff);

                    if (diff > Hub.XmlData.MinTimeSpanBetweenProjUpdateMSec)
                    {
                        ChangedProject.LastUpdateTime = now;
                        OnFileChanged(null, new ChangedProjectEventArgs(ChangedProject));
                    }
                    else
                    {
                        Hub.Reporter.ToHuman("Ignoring - not enough time difference <MinTimeSpanBetweenProjUpdateMSec> : " + Hub.XmlData.MinTimeSpanBetweenProjUpdateMSec);
                    }
                }
                Hub.Reporter.ToHuman("Update Complete");
                Hub.Reporter.WriteMenu();
            }
        }

        #endregion Methods
    }
}