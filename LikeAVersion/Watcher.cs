using System;
using System.Collections.Generic;
using System.IO;

namespace LikeAVersion
{
    public class Watcher
    {
        #region Fields

        private List<string> excludedFilesLc = new List<string>();

        #endregion Fields

        #region Constructors

        public Watcher(List<string> excludedFiles)
        {
            excludedFiles.ForEach(x => excludedFilesLc.Add(x.ToLower()));
            this.excludedFilesLc = excludedFiles;
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
            Console.WriteLine("Watching: " + projectData.CsProjFile.Directory.FullName + " filter: " + filter);
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
            var triggerFullFileName = string.Empty;
            if (args.ChangeType.Equals(WatcherChangeTypes.Renamed))
            {
                var renameArgs = args as RenamedEventArgs;
                if (args != null)
                {
                    triggerFullFileName = renameArgs.OldFullPath;
                }
            }
            else
            {
                var modifiedFile = args.Name;
                triggerFullFileName = args.FullPath;
                {
                    okToProceed = false;
                }
            }
            var triggerFileName = new FileInfo(triggerFullFileName).Name;

            if (excludedFilesLc.Contains(triggerFileName.ToLower()))
            {
                okToProceed = false;
            }

            if (okToProceed)
            {
                Console.WriteLine("");
                Console.WriteLine("-------");
                Console.WriteLine("");

                if (OnFileChanged != null)
                {
                    var now = DateTime.Now;
                    var diff = now.Subtract(ChangedProject.LastTriggerTime).TotalMilliseconds;

                    if (diff > ChangedProject.MinSpan)
                    {
                        Console.WriteLine("Changed Trigger: " + triggerFileName);
                        Console.WriteLine("Changed Project: " + ChangedProject.projName);
                        ChangedProject.LastTriggerTime = now;
                        OnFileChanged(null, new ChangedProjectEventArgs(ChangedProject));
                    }
                    else
                    {
                        Console.WriteLine("Ignoring (not enough time diff) : " + ChangedProject.projName);
                    }
                }
            }
        }

        #endregion Methods
    }
}