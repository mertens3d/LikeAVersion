using System;
using System.Collections.Generic;
using System.IO;

namespace LikeAVersion
{
    public class ProjectData
    {

        #region Properties

        public FileInfo AssemblyInfoFile { get; set; }
        public FileInfo CsProjFile { get; set; }
        public DateTime LastAssemblyWrite { get; set; } = DateTime.Now;
        public DateTime LastTriggerTime { get; set; } = DateTime.Now;
        public int MinSpan { get; internal set; }
        public string ProjectGuidAsString { get; set; }
        public string projName { get; set; }
        public List<OneRefData> RawRefData { get; set; } = new List<OneRefData>();
        public List<ProjectData> ReferencedFiles { get; set; } = new List<ProjectData>();
        public List<ProjectData> ReverseRef { get; set; } = new List<ProjectData>();
        public List<ProjectData> UpsteamProjects { get; internal set; }

        #endregion Properties

    }
}