using System.Collections.Generic;
using System.IO;

namespace LikeAVersion
{
    public class SlnData
    {
        public DirectoryInfo SolutionFolder { get; set; }
        public List<ProjectData> Projects { get; set; } = new List<ProjectData>();

    }
}
