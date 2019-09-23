﻿using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LikeAVersion
{
    public class CsProjArray
    {
        #region Properties

        public Targets AllTargets { get; set; }
        public ILog logger { get; set; }
        public SlnData SlnData { get; set; } = new SlnData();
        public List<string> targetProjectNames { get; set; }

        #endregion Properties

        #region Methods

        //public void populateProjRefs()
        //{
        //    if (SlnData?.Projects != null)
        //    {
        //        for (var idx = 0; idx < SlnData.Projects.Count; idx++)
        //        {
        //            ReadProjectReferencesFromOneFile(SlnData.Projects[idx]);
        //        }
        //    }
        //    else
        //    {
        //        logger.Error("No projects listed");
        //    }
        //}
        public void CrossRefOneProject(ProjectData project)
        {
            foreach (var oneRef in project.ReferencedFiles)
            {

                oneRef.ReverseRef.Add(project);

            }
        }

        public void CrossRererenceRefFiles()
        {
            foreach (var oneProj in SlnData.Projects)
            {
                CrossRefOneProject(oneProj);
            }
        }

        public FileInfo GetAssemblyInfoForProj(string needleProj)
        {
            // logger.Debug("s) GetAssemblyInfoForProj: " + needleProj);

            FileInfo toReturn = null;

            for (var idx = 0; idx < SlnData.Projects.Count; idx++)
            {
                var candidate = SlnData.Projects[idx];
                //logger.Debug("Comparing to: " + candidate.projName);
                if (candidate.projName.Equals(needleProj))
                {
                    toReturn = candidate.AssemblyInfoFile;
                    break;
                }
            }
            // logger.Debug("e) GetAssemblyInfoForProj: " + toReturn);
            return toReturn;
        }

        public FileInfo getCsProjInFolder()
        {
            return getFirstFileByExt("csproj", SlnData.SolutionFolder);
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

        public List<string> GetUpStreamProjects(ProjectData projData)
        {
            //we need to loop through all of the projects and find any that have this csProj in it
            var needleProjName = projData.projName;// needleProjName.Replace(".csproj", "");

            logger.Debug("s) GetAllUpStreamProjects: " + needleProjName);

            var projToCheck = new List<string>();
            projToCheck.Add(needleProjName);
            var projChecked = new List<string>();
            var toReturn = new List<string> { needleProjName };
            var maxIteration = 100;
            var currentIteration = 0;

            while (projToCheck.Count > 0 && currentIteration < maxIteration)
            {
                currentIteration++;

                string currNeedle = projToCheck[0];
                projToCheck.RemoveAt(0);

                logger.Debug("\t looking projs that ref: " + currNeedle);

                for (var idx = 0; idx < SlnData.Projects.Count; idx++)
                {
                    var candidateProjData = SlnData.Projects[idx];
                    logger.Debug("candidate: " + candidateProjData.projName + " refs: " + candidateProjData.RawRefData.Count);

                    foreach (var oneProjRef in candidateProjData.RawRefData)
                    {
                        logger.Debug("\tref projName: " + oneProjRef);

                        if (oneProjRef.Equals(currNeedle))
                        {
                            if (!projChecked.Contains(candidateProjData.projName))
                            {
                                // logger.Debug("\tfound one: " + candidateProjData.projName);
                                projToCheck.Add(candidateProjData.projName);
                                projChecked.Add(candidateProjData.projName);
                                toReturn.Add(candidateProjData.projName);
                            }
                        }
                    }
                }
            }

            toReturn.Sort();

            for (var mdx = 0; mdx < toReturn.Count; mdx++)
            {
                logger.Debug(mdx + " : " + toReturn[mdx]);
            }

            logger.Debug("e) GetAllUpStreamProjects: " + toReturn.Count);
            return toReturn;
        }

        public void Init(ILog loggerIn)
        {
            logger = loggerIn;
            logger.Debug("s) Init");

            ReadSettingsFromXml();
            PopulateSolutionObj();
            ReadSolutionFile();
            PopulateProjDataFromXDoc();
            PopulateCleanRefData();
            CrossRererenceRefFiles();
            logger.Debug("e) Init");
        }

        //public void PopulateAssemblyFiles()
        //{
        //    foreach (var workingProj in SlnData.Projects)
        //    {
        //        var propFolder = workingProj.CsProjFile.Directory.GetDirectories("Properties");
        //        if (propFolder != null && propFolder.Any())
        //        {
        //            var candidates = propFolder[0].GetFiles("AssemblyInfo.cs");
        //            if (candidates != null && candidates.Any())
        //            {
        //                workingProj.AssemblyInfo = candidates[0];
        //            }
        //            else
        //            {
        //                workingProj.AssemblyInfo = null;
        //                logger.Error("No assembly file found for: " + propFolder[0].FullName);
        //            }
        //        }
        //        else
        //        {
        //            logger.Error("No Properties folder found for: " + workingProj.CsProjFile.Directory.FullName);
        //        }
        //    }
        //}


        public void ReadSolutionFile()
        {
            logger.Debug("s) ReadSolutionFile: " + SlnData.SolutionFolder.FullName);
            var slnFile = getFirstFileByExt("sln", SlnData.SolutionFolder);

            if (slnFile != null)
            {
                var fileContent = FileTools.ReadFileSync(slnFile, ReturnState.Clean);

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
                                //projRelPath = projRelPath.replace(/ "/gi, "");

                                projRelPath = projRelPath.Replace("\"", "").Trim();

                                var projData = new ProjectData
                                {
                                    MinSpan = AllTargets.MinMSecSpan
                                };

                                var projFullPath = Path.Combine(SlnData.SolutionFolder.FullName, projRelPath);

                                var foundProjectName = data[0];

                                var regEx = new Regex("Project.*=");
                                foundProjectName = regEx.Replace(foundProjectName, string.Empty);
                                foundProjectName = foundProjectName.Replace("\"", string.Empty);
                                foundProjectName = foundProjectName.Trim();

                                projData.CsProjFile = new FileInfo(projFullPath);
                                bool found = false;
                                foreach (var oneProj in AllTargets.TargetProjectNames)
                                {
                                    if (oneProj.Equals(foundProjectName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        projData.projName = foundProjectName;


                                        SlnData.Projects.Add(projData);
                                        found = true;
                                        break;
                                    }
                                }
                                if(!found)
                                {
                                    Console.WriteLine("Ignoring: " + foundProjectName);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PopulateCleanRefData()
        {
            foreach (var oneProj in SlnData.Projects)
            {
                Console.WriteLine("Studying Refs for: " + oneProj.projName);
                foreach (var oneRef in oneProj.RawRefData)
                {
                    var realProj = SlnData.Projects.FirstOrDefault(x => x.ProjectGuidAsString.Equals(oneRef.ProjectGuidAsString, StringComparison.OrdinalIgnoreCase));
                    if (realProj != null)
                    {
                        //if (SlnData.Projects.Contains(realProj))
                        //{
                        oneProj.ReferencedFiles.Add(realProj);
                        Console.WriteLine("\tAdding Ref: " + realProj.projName);
                        //}
                        //else
                        //{
                        //    Console.WriteLine("\tIgnoring Ref: " + realProj.)
                        //}
                    }
                }
            }
        }

        //public void ReadProjectReferencesFromOneFile(ProjectData oneProject)
        //{
        //    var toReturn = new List<string>();
        //    if (oneProject != null && oneProject.CsProjFile.Exists)
        //    {
        //        logger.Debug("s) ReadProjectReferences : " + oneProject.CsProjFile.FullName);
        //        oneProject.ProjReferences = new List<string>();

        //        var isValidPath = oneProject?.CsProjFile != null && oneProject.CsProjFile.Exists;
        //        logger.Debug("\t Is Valid Path: " + isValidPath);

        //        if (isValidPath)
        //        {
        //            var rawFileContent = ReadFileSync(oneProject.CsProjFile);
        //            var fileContent = rawFileContent.Where(x => x.StartsWith("<ProjectReference")).ToList();

        //            var firstIsSkipped = false;

        //            foreach (var oneLine in fileContent)
        //            {
        //                //   logger.Debug("oneLineIdx: " + oneLineIdx);
        //                if (firstIsSkipped)
        //                {
        //                    var oneLineSplit = oneLine
        //                        .Split(new String[] { "<Name>" }, StringSplitOptions.RemoveEmptyEntries)[0]
        //                        .Split(new String[] { "</Name>" }, StringSplitOptions.RemoveEmptyEntries)[0]
        //                        .Trim();

        //                    // logger.Debug("\t oneSplit: " + oneLineSplit);

        //                    oneProject.ProjReferences.Add(oneLineSplit);
        //                }
        //                firstIsSkipped = true;
        //            }
        //        }
        //    }
        //    logger.Debug("e) ReadProjectReferences");
        //}

        private void PopulateProjDataFromXDoc()
        {
            foreach (var oneProj in SlnData.Projects)
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
                        oneRef.Include = oneProjRef.Attributes("Include").FirstOrDefault().Value;
                        oneRef.ProjectGuidAsString = oneProjRef
                            .Elements(msbuild + "Project").Select(x => x.Value).FirstOrDefault();
                        oneRef.ProjectName = oneProjRef
                          .Elements(msbuild + "Name").Select(x => x.Value).FirstOrDefault();

                        oneProj.RawRefData.Add(oneRef);
                    }

                    var allCompile = projDefinition
                         .Element(msbuild + "Project")
                         .Elements(msbuild + "ItemGroup")
                         .Elements(msbuild + "Compile");

                    var foundAssemblyInfoNode =
                   allCompile.FirstOrDefault(x => x.Attribute("Include").Value.Contains("AssemblyInfo.cs"));

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
                    logger.Debug(oneProj.projName + " does no have project references");
                }
            }
        }

        private void PopulateSolutionObj()
        {
            SlnData = new SlnData
            {
                SolutionFolder = new DirectoryInfo(AllTargets.SolutionFolder)
            };
        }

        private void ReadSettingsFromXml()
        {
            string assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo assemblyFile = new FileInfo(assembly);
            FileInfo targetXml = new FileInfo(Path.Combine(assemblyFile.DirectoryName, "TargetData.xml"));
            Serializer ser = new Serializer();

            if (targetXml.Exists)
            {
                string xmlInputData = File.ReadAllText(targetXml.FullName);
                try
                {
                    AllTargets = ser.Deserialize<Targets>(xmlInputData);
                    //if(AllTargets != null)
                    //{

                    //}
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