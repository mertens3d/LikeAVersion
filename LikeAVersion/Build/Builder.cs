using log4net;
using mertens3d.LikeAVersion;
using mertens3d.LikeAVersion.FileTools;
using mertensd.LikeAVersion.Enums;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.FileTools;
using mertensd.LikeAVersion.Models;
using mertense3d.LikeAVersion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace mertensd.LikeAVersion.Build
{
    public class DataBuilder
    {
        #region Fields

        private ILog logger;

        public Reporter Reporter { get; }

        #endregion Fields

        #region Constructors

        public DataBuilder(ILog logger, Reporter reporter)
        {
            this.logger = logger;
            this.Reporter = reporter;
        }

        #endregion Constructors

        #region Properties

        private CsProjArray ProjAr { get; set; }

        #endregion Properties

        #region Methods

        public CsProjArray BuildCurrentState()
        {
            this.ProjAr = new CsProjArray();

            ReadSettingsFromXml();
            PopulateSolutionObj();
            ReadSolutionFile();
            PopulateProjectsInSolutionDataFromXDoc();
            PopulateCleanRefData();
            CrossReferenceRefFiles();
            CalculateUpstreamProj();

            return this.ProjAr;
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

        public void CrossRefOneProject(ProjectData project)
        {
            foreach (var oneRef in project.ReferencedFiles)
            {
                oneRef.ProjThatRefThisProj.Add(project);
            }
        }

        public void CrossReferenceRefFiles()
        {
            foreach (var oneProj in ProjAr.SlnData.WatchedProjects)
            {
                CrossRefOneProject(oneProj);
            }
        }

        public FileInfo getFirstFileByExt(string targetExt, DirectoryInfo targetDir)
        {
            FileInfo toReturn = null;
            logger.Debug("s) getFirstFileByExt: " + targetExt + "  " + targetDir.FullName);

            var isValid = targetDir.Exists;

            logger.Debug("Is Valid Path: " + isValid);

            if (isValid)
            {
                var dirCont = targetDir.GetFiles("*." + targetExt).ToList();

                if (dirCont != null && dirCont.Any())
                {
                    toReturn = dirCont[0];
                }
                else
                {
                    logger.Error("First not found");
                }
            }

            logger.Debug("e) returning: " + toReturn);
            return toReturn;
        }

        public void ReadSolutionFile()
        {
            logger.Debug("s) ReadSolutionFile: " + ProjAr.SlnData.SolutionFolder.FullName);
            var slnFile = getFirstFileByExt("sln", ProjAr.SlnData.SolutionFolder);

            if (slnFile != null)
            {
                var fileContent = FileIO.ReadFileSync(slnFile, ReturnState.Clean);
                ProcessOneFileContent(fileContent);
            }
        }

        private bool CheckForInclusion(ProjectData projData, string foundProjectName, bool found)
        {
            var includeIt = true;
            foreach (var oneProj in ProjAr.AllTargets.IgnoredProjects)
            {
                if (oneProj.Equals(foundProjectName, StringComparison.OrdinalIgnoreCase))
                {
                    includeIt = false;
                    Reporter.ToHuman("Ignoring: " + foundProjectName);
                    break;
                }
            }

            if (includeIt)
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

        private void PopulateCleanRefData()
        {
            foreach (var oneProj in ProjAr.SlnData.WatchedProjects)
            {
                Reporter.ToHuman("Studying Refs for: " + oneProj.ProjName);
                foreach (var oneRef in oneProj.RawRefData)
                {
                    var realProj = ProjAr.SlnData.WatchedProjects.FirstOrDefault(x => x.ProjectGuidAsString.Equals(oneRef.ProjectGuidAsString, StringComparison.OrdinalIgnoreCase));
                    if (realProj != null)
                    {
                        oneProj.ReferencedFiles.Add(realProj);
                        Reporter.ToHuman("\tAdding Ref: " + realProj.ProjName);
                    }
                }
            }
        }

        private void PopulateProjectsInSolutionDataFromXDoc()
        {
            foreach (var oneProj in ProjAr.SlnData.WatchedProjects)
            {
                XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
                XDocument projDefinition = XDocument.Load(oneProj.CsProjFile.FullName);

                try
                {
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
                    logger.Debug(oneProj.ProjName + " does no have project references");
                }
            }
        }

        private void PopulateSolutionObj()
        {
            ProjAr.SlnData = new SlnData
            {
                SolutionFolder = new DirectoryInfo(ProjAr.AllTargets.SolutionFolder)
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
                        logger.Debug("> " + projRelPath);
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
            //projRelPath = projRelPath.replace(/ "/gi, "");

            projRelPath = projRelPath.Replace("\"", "").Trim();

            var projData = new ProjectData
            {
                MinSpan = ProjAr.AllTargets.MinMSecSpan
            };

            var projFullPath = Path.Combine(ProjAr.SlnData.SolutionFolder.FullName, projRelPath);

            var foundProjectName = data[0];

            var regEx = new Regex("Project.*=");
            foundProjectName = regEx.Replace(foundProjectName, string.Empty);
            foundProjectName = foundProjectName.Replace("\"", string.Empty);
            foundProjectName = foundProjectName.Trim();

            projData.CsProjFile = new FileInfo(projFullPath);
            bool found = false;
            found = CheckForInclusion(projData, foundProjectName, found);
        }

        private void ReadSettingsFromXml()
        {
            string assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo assemblyFile = new FileInfo(assembly);
            FileInfo targetXml = new FileInfo(Path.Combine(assemblyFile.DirectoryName, Constants.TargetDataXml));
            Serializer ser = new Serializer();

            if (targetXml.Exists)
            {
                string xmlInputData = File.ReadAllText(targetXml.FullName);
                try
                {
                    ProjAr.AllTargets = ser.Deserialize<Targets>(xmlInputData);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
            else
            {
                logger.Error("Did not find settings file: " + targetXml.FullName);
            }
        }

        #endregion Methods
    }
}