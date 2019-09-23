using System;
using System.IO;

namespace LikeAVersion
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