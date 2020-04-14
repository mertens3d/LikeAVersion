namespace mertensd.LikeAVersion.Models
{
    public class Dead
    {
        //public void StartProjWatch()
        //{
        //    log.Debug("s) Init Project Watch. Count = " + SlnProjObj.AllTargets.TargetProjectNames.Count);
        //    for (var projIdx = 0; projIdx < SlnProjObj.AllTargets.TargetProjectNames.Count; projIdx++)
        //    {
        //        var oneProject = SlnProjObj.AllTargets.TargetProjectNames[projIdx];
        //        log.Debug("Init Project: " + projIdx + ":" + (SlnProjObj.AllTargets.TargetProjectNames.Count - 1) + "  " + oneProject);
        //    }
        //}

        //public DirectoryInfo CalculateProjectRoot(string relativePath)
        //{
        //    log.Debug("s) Calculate Project Root Folder");
        //    DirectoryInfo toReturn = null;
        //    var firstFolder = "";
        //    var asArray = relativePath.Split('\\').ToArray();
        //    for (var idx = 0; idx < asArray.Count(); idx++)
        //    {
        //        var candidateFolder = asArray[idx];
        //        if (candidateFolder.Count() > 0)
        //        {
        //            firstFolder = candidateFolder;
        //            break;
        //        }
        //    }

        //    log.Debug("\tFirst Folder: " + firstFolder);

        //    if (SlnProjObj.AllTargets.TargetProjectNames.IndexOf(firstFolder) > -1)
        //    {
        //        toReturn = new DirectoryInfo(Path.Combine(__dirname, firstFolder));
        //    }
        //    else
        //    {
        //        log.Error("found root folder not in targets");
        //    }

        //    log.Debug("e) Calculate Project Root Folder. Returning: " + toReturn);
        //    return toReturn;
        //}

        //public FileInfo GetAssemblyInfoForProj(string needleProj)
        //{
        //    // logger.Debug("s) GetAssemblyInfoForProj: " + needleProj);

        //    FileInfo toReturn = null;

        //    for (var idx = 0; idx < SlnData.WatchedProjects.Count; idx++)
        //    {
        //        var candidate = SlnData.WatchedProjects[idx];
        //        //logger.Debug("Comparing to: " + candidate.projName);
        //        if (candidate.projName.Equals(needleProj))
        //        {
        //            toReturn = candidate.AssemblyInfoFile;
        //            break;
        //        }
        //    }
        //    // logger.Debug("e) GetAssemblyInfoForProj: " + toReturn);
        //    return toReturn;
        //}

        //public FileInfo getCsProjInFolder()
        //{
        //    return getFirstFileByExt("csproj", SlnData.SolutionFolder);
        //}
    }
}