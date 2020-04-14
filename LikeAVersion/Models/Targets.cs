﻿using System.Collections.Generic;

namespace mertense3d.LikeAVersion.Models
{
    public class Targets
    {
        #region Properties

        public string SolutionFolder { get; set; }
        public List<string> TargetFileTypes { get; set; }
        public List<string> TargetProjectNames { get; set; }
        public List<string> IgnoredProjects { get; set; }
        public List<string> ExcludedFiles { get; set; }
        public int MinMSecSpan { get; set; }

        #endregion Properties
    }
}