using mertensd.LikeAVersion;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.Models;
using mertensd.LikeAVersion.Watcher;
using System;
using System.Collections.Generic;
using System.IO;

namespace mertense3d.LikeAVersion
{
    public class OneTargetedDirectoryOfTypeMonitor
    {
        #region Fields

        private List<string> excludedFilesLc = new List<string>();

        #endregion Fields

        #region Constructors

        public OneTargetedDirectoryOfTypeMonitor(List<string> excludedFiles, Reporter reporter)
        {
            this.Reporter = reporter;
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
        private Reporter Reporter { get; }

        #endregion Properties

        #region Methods

        public void MonitorDirectory(ProjectData projectData, string filter)
        {
            filesystemwatcher = new FileSystemWatcher();
            filesystemwatcher.Path = projectData.CsProjFile.Directory.FullName;
            filesystemwatcher.Filter = filter;
            filesystemwatcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
            filesystemwatcher.IncludeSubdirectories = true;
            filesystemwatcher.Changed += FileSystemWatcher_Changed;
            filesystemwatcher.Renamed += FileSystemWatcher_Changed;

            filesystemwatcher.EnableRaisingEvents = true;
            ChangedProject = projectData;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs args)
        {
            bool okToProceed = true;
            string triggerFileName = GetFileNameFromArgs(args);

            okToProceed = okToProceed && !string.IsNullOrEmpty(triggerFileName);
            okToProceed = okToProceed && !excludedFilesLc.Contains(triggerFileName.ToLower());

            if (okToProceed)
            {
                Reporter.ToHuman("");
                Reporter.ToHuman("-------");
                Reporter.ToHuman("");

                if (OnFileChanged != null)
                {
                    var now = DateTime.Now;
                    var diff = now.Subtract(ChangedProject.LastTriggerTime).TotalMilliseconds;

                    if (diff > ChangedProject.MinSpan)
                    {
                        Reporter.ToHuman("Changed Trigger: " + triggerFileName);
                        Reporter.ToHuman("Changed Project: " + ChangedProject.ProjName);
                        ChangedProject.LastTriggerTime = now;
                        OnFileChanged(null, new ChangedProjectEventArgs(ChangedProject));
                    }
                    else
                    {
                        Reporter.ToHuman("Ignoring (not enough time diff) : " + ChangedProject.ProjName + " " + diff + " vs " + ChangedProject.MinSpan);
                    }
                }
                Reporter.ToHuman("Update Complete");
                Reporter.WriteMenu();
            }
        }

        private static string GetFileNameFromArgs(FileSystemEventArgs args)
        {
            string toReturn = string.Empty;
            var triggerFullFileName = string.Empty;
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

        #endregion Methods
    }
}