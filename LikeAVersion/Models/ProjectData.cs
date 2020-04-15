using System;
using System.Collections.Generic;
using System.IO;

namespace mertensd.LikeAVersion.Models
{
    public class ProjectData
    {
        #region Properties

        public FileInfo AssemblyInfoFile { get; set; }
        public FileInfo CsProjFile { get; set; }
        public DateTime LastAssemblyWrite { get; set; } = DateTime.Now;
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        public string ProjectGuidAsString { get; set; }
        public string ProjName { get; set; }
        public List<OneRefData> RawRefData { get; set; } = new List<OneRefData>();
        public List<ProjectData> ReferencedFiles { get; set; } = new List<ProjectData>();
        public List<ProjectData> ProjThatRefThisProj { get; set; } = new List<ProjectData>();
        public List<OneUpStreamProj> UpsteamProjects { get; internal set; }

        #endregion Properties
    }
}