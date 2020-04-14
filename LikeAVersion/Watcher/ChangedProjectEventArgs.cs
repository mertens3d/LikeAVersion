using mertensd.LikeAVersion.Models;
using System;

namespace mertensd.LikeAVersion.Watcher
{
    public class ChangedProjectEventArgs : EventArgs
    {
        public ProjectData changedProject { get; set; }

        public ChangedProjectEventArgs(ProjectData changedProject)
        {
            this.changedProject = changedProject;
        }
    }
}