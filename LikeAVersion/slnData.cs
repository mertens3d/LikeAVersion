using System.Collections.Generic;
using System.IO;

namespace LikeAVersion
{
    public class SlnData
    {
        public DirectoryInfo SolutionFolder { get; set; }
        public List<ProjectData> WatchedProjects { get; set; } = new List<ProjectData>();
        public List<ProjectData> IgnoredProjects { get; set; } = new List<ProjectData>();
    }
}