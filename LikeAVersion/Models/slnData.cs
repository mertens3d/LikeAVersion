using System.Collections.Generic;
using System.IO;

namespace mertensd.LikeAVersion.Models
{
    public class SlnData
    {
        #region Properties

        public List<ProjectData> IgnoredProjects { get; set; } = new List<ProjectData>();
        public DirectoryInfo SolutionFolder { get; set; }
        public List<ProjectData> WatchedProjects { get; set; } = new List<ProjectData>();

        #endregion Properties
    }
}