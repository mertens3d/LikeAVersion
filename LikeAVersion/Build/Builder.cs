using mertens3d.LikeAVersion;
using mertensd.LikeAVersion.Enums;
using mertensd.LikeAVersion.FileTools;
using mertensd.LikeAVersion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace mertensd.LikeAVersion.Build
{
    public class DataBuilder : CommonBase_a
    {
        #region Constructors

        public DataBuilder(HeartBeatHub hub) : base(hub)
        {
        }

        #endregion Constructors

        #region Properties

        private CsProjArray ProjAr { get; set; }

        #endregion Properties

        #region Methods

        public CsProjArray BuildCurrentState()
        {
            this.ProjAr = new CsProjArray();

            ReadSettingsXml();

            if (Hub.XmlData != null)
            {
                PopulateSolutionObj();
                ReadSolutionFile();
                PopulateProjectsInSolutionDataFromXDoc();
                PopulateCleanRefData();
                CrossReferenceRefFiles();
                CalculateUpstreamProj();
            }

            return this.ProjAr;
        }

        public void CrossReferenceRefFiles()
        {
            foreach (var oneProj in ProjAr.SlnData.WatchedProjects)
            {
                CrossRefOneProject(oneProj);
            }
        }

        public void CrossRefOneProject(ProjectData project)
        {
            foreach (var oneRef in project.ReferencedFiles)
            {
                oneRef.ProjThatRefThisProj.Add(project);
            }
        }

        public FileInfo getFirstFileByExt(string targetExt, DirectoryInfo targetDir)
        {
            FileInfo toReturn = null;
            Hub.Logger.Debug("s) getFirstFileByExt: " + targetExt + "  " + targetDir.FullName);

            var isValid = targetDir.Exists;

            Hub.Logger.Debug("Is Valid Path: " + isValid);

            if (isValid)
            {
                var dirCont = targetDir.GetFiles("*." + targetExt).ToList();

                if (dirCont != null && dirCont.Any())
                {
                    toReturn = dirCont[0];
                }
                else
                {
                    Hub.Logger.Error("First not found");
                }
            }

            Hub.Logger.Debug("e) returning: " + toReturn);
            return toReturn;
        }

        public List<OneUpStreamProj> GetUpstreamProjectsRecursive(ProjectData rootProj, List<OneUpStreamProj> knownUpsteam, int depth)
        {
            var toReturnList = new List<OneUpStreamProj>();

            var rootInKnown = knownUpsteam.FirstOrDefault(x => x.ProjData.Equals(rootProj));
            if (rootInKnown == null)
            {
                //toReturnList.Add(new OneUpStreamProj(rootProj, depth));
            }

            var foundOnThisLevel = new List<OneUpStreamProj>();
            foreach (var oneUpStreamProj in rootProj.ProjThatRefThisProj)
            {
                //var isInReturnList = toReturnList.FirstOrDefault(x => x.ProjData.Equals(oneUpStreamProj));
                var isInKnownList = knownUpsteam.FirstOrDefault(x => x.ProjData.Equals(oneUpStreamProj));
                if (isInKnownList == null) //isInReturnList == null &&
                {
                    foundOnThisLevel.Add(new OneUpStreamProj(oneUpStreamProj, depth));
                }
            }

            foreach (var oneUpstream in foundOnThisLevel)
            {
                toReturnList.Add(oneUpstream);
            }

            foreach (var oneUpsteam in foundOnThisLevel)
            {
                var ancestors = GetUpstreamProjectsRecursive(oneUpsteam.ProjData, Enumerable.Concat(knownUpsteam, toReturnList).ToList(), depth + 1);
                foreach (var oneAncestor in ancestors)
                {
                    var ancestorInReturnList = toReturnList.FirstOrDefault(x => x.ProjData.Equals(oneAncestor));
                    if (ancestorInReturnList == null)
                    {
                        toReturnList.Add(oneAncestor);
                    }
                }
            }

            return toReturnList;
        }

        public void ReadSolutionFile()
        {
            Hub.Logger.Debug("s) ReadSolutionFile: " + ProjAr.SlnData.SolutionFolder.FullName);
            var slnFile = getFirstFileByExt("sln", ProjAr.SlnData.SolutionFolder);

            if (slnFile != null)
            {
                var fileContent = FileIO.ReadFileSync(slnFile, ReturnState.Clean);
                ProcessOneFileContent(fileContent);
            }
        }

        private void CalculateUpstreamProj()
        {
            foreach (var oneSlnProject in ProjAr.SlnData.WatchedProjects)
            {
                if (oneSlnProject != null)
                {
                    oneSlnProject.UpsteamProjects = GetUpstreamProjectsRecursive(oneSlnProject,
                        new List<OneUpStreamProj>(),

                        0);
                }
            }
        }

        private bool CheckForInclusionExclusion(ProjectData projData, string foundProjectName, bool found)
        {
            if (!IsItExcluded(foundProjectName))
            {
                projData.ProjName = foundProjectName;

                ProjAr.SlnData.WatchedProjects.Add(projData);
                found = true;
            }
            else
            {
                ProjAr.SlnData.IgnoredProjects.Add(projData);
            }

            return found;
        }

        private bool IsItExcluded(string foundProjectName)
        {
            var toReturn = false;
            foreach (var oneProj in Hub.XmlData.IgnoredProjects)
            {
                if (oneProj.Equals(foundProjectName, StringComparison.OrdinalIgnoreCase))
                {
                    toReturn = true;
                    Hub.Reporter.ToHuman("Ignoring (in <IgnoredProjects>): " + foundProjectName);
                    break;
                }
            }

            return toReturn;
        }

        private void PopulateCleanRefData()
        {
            foreach (var oneProj in ProjAr.SlnData.WatchedProjects)
            {
                Hub.Reporter.ToHuman("Studying Refs for: " + oneProj.ProjName);
                foreach (var oneRef in oneProj.RawRefData)
                {
                    var realProj = ProjAr.SlnData.WatchedProjects
                        .FirstOrDefault(x => x.ProjectGuidAsString.Equals(oneRef.ProjectGuidAsString, StringComparison.OrdinalIgnoreCase));
                    if (realProj != null)
                    {
                        oneProj.ReferencedFiles.Add(realProj);
                        Hub.Reporter.ToHuman("\tAdding Ref: " + realProj.ProjName);
                    }
                }
            }
        }

        private void PopulateProjectsInSolutionDataFromXDoc()
        {
            foreach (var oneProj in ProjAr.SlnData.WatchedProjects)
            {
                XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

                try
                {
                    XDocument projDefinition = XDocument.Load(oneProj.CsProjFile.FullName);

                    var references = projDefinition
                         .Element(msbuild + "Project")
                         .Elements(msbuild + "ItemGroup")
                         .Elements(msbuild + "ProjectReference");

                    foreach (var oneProjRef in references)
                    {
                        var oneRef = new OneRefData();

                        oneRef.Include = oneProjRef
                            .Attributes("Include")
                            .FirstOrDefault()
                            .Value;

                        oneRef.ProjectGuidAsString = oneProjRef
                            .Elements(msbuild + "Project")
                            .Select(x => x.Value)
                            .FirstOrDefault();

                        oneRef.ProjectName = oneProjRef
                          .Elements(msbuild + "Name")
                          .Select(x => x.Value)
                          .FirstOrDefault();

                        oneProj.RawRefData.Add(oneRef);
                    }

                    var allCompile = projDefinition
                         .Element(msbuild + "Project")
                         .Elements(msbuild + "ItemGroup")
                         .Elements(msbuild + "Compile");

                    var foundAssemblyInfoNode =
                   allCompile
                   .FirstOrDefault(x => x.Attribute("Include").Value.Contains("AssemblyInfo.cs"));

                    if (foundAssemblyInfoNode != null)
                    {
                        var AssemblyInfoRelPath = foundAssemblyInfoNode.Attribute("Include").Value;
                        oneProj.AssemblyInfoFile = new FileInfo(Path.Combine(oneProj.CsProjFile.Directory.FullName, AssemblyInfoRelPath));
                    }
                    //---------- project guid

                    var projectGuid = projDefinition
                        .Element(msbuild + "Project")
                        .Elements(msbuild + "PropertyGroup")
                        .Elements(msbuild + "ProjectGuid");

                    if (projectGuid != null)
                    {
                        oneProj.ProjectGuidAsString = projectGuid.First().Value;
                    }

                    //ProjectGuidAsString
                }
                catch (Exception ex)
                {
                    Hub.Reporter.Error(oneProj.ProjName + ex.Message);
                }
            }
        }

        private void PopulateSolutionObj()
        {
            ProjAr.SlnData = new SlnData
            {
                SolutionFolder = new DirectoryInfo(Hub.XmlData.TargetSolutionFolder)
            };
        }

        private void ProcessOneFileContent(List<string> fileContent)
        {
            foreach (var candidateLine in fileContent)
            {
                if (candidateLine.StartsWith("Project"))
                {
                    var data = candidateLine.Split(',');
                    if (data.Length > 1)
                    {
                        var projRelPath = data[1];
                        Hub.Logger.Debug("> " + projRelPath);
                        if (projRelPath.IndexOf("csproj") > 0)
                        {
                            ProcessOneFoundCsProjLin(projRelPath, data);
                        }
                    }
                }
            }
        }

        private void ProcessOneFoundCsProjLin(string projRelPath, string[] data)
        {
            projRelPath = projRelPath.Replace("\"", "").Trim();

            var projFullPath = Path.Combine(ProjAr.SlnData.SolutionFolder.FullName, projRelPath);
            var projData = new ProjectData()
            {
                CsProjFile = new FileInfo(projFullPath)
            };

            var foundProjectName = data[0];

            var regEx = new Regex("Project.*=");
            foundProjectName = regEx.Replace(foundProjectName, string.Empty);
            foundProjectName = foundProjectName.Replace("\"", string.Empty);
            foundProjectName = foundProjectName.Trim();

            bool found = false;
            found = CheckForInclusionExclusion(projData, foundProjectName, found);
        }

        private void ReadSettingsXml()
        {
            FileInfo targetXml = new FileInfo(Path.Combine(
                Hub.RunTimeValues.AssemblyDirectory,
                Constants.TargetDataXml));

            Hub.Reporter.ToHuman("Reading settings file: " + targetXml);

            var settingsReader = new SettingsFile(Hub);
            Hub.XmlData = settingsReader.ReadFromXml(targetXml);
        }

        #endregion Methods
    }
}