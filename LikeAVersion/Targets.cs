using System.Collections.Generic;

namespace LikeAVersion
{
    public class Targets
    {
        #region Properties

        public string SolutionFolder { get; set; }
        public List<string> TargetFileTypes { get; set; }
        public List<string> TargetProjectNames { get;  set; }
        public List<string> ExcludedFiles { get;  set; }
        public int MinMSecSpan { get;  set; }

        #endregion Properties
    }
}
