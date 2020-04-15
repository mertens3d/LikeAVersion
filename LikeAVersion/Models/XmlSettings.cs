using System.Collections.Generic;

namespace mertense3d.LikeAVersion.Models
{
    public class XmlSettings
    {
        #region Properties

        public string TargetSolutionFolder { get; set; }
        public List<string> TargetFileTypes { get; set; }
        public List<string> TargetProjectNames { get; set; }
        public List<string> IgnoredProjects { get; set; }
        public List<string> ExcludedFiles { get; set; }
        public int MinTimeSpanBetweenProjUpdateMSec { get; set; }
        public int SleepBeforeResumeWatchMSec { get; set; }

        #endregion Properties
    }
}