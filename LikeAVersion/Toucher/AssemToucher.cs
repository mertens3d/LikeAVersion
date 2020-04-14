using log4net;
using mertens3d.LikeAVersion;
using mertensd.LikeAVersion.Feedback;
using mertense3d.LikeAVersion;
using System;
using System.Linq;

namespace mertensd.LikeAVersion.Toucher
{
    public class AssemToucher
    {
        #region Fields

        private AssemblyInfoFileHandler _assemblyInfoFileHandler;
        private ILog log;

        #endregion Fields

        #region Constructors

        public AssemToucher(ILog log, Reporter reporter)
        {
            this.log = log;
            this.Reporter = reporter;
        }

        #endregion Constructors

        #region Properties

        public AssemblyInfoFileHandler AssemblyInfoFileHandler
        {
            get
            {
                return _assemblyInfoFileHandler ?? (_assemblyInfoFileHandler = new AssemblyInfoFileHandler(log, Reporter));
            }
        }

        private Reporter Reporter { get; }

        #endregion Properties

        #region Methods

        public void TouchAllAssemblyFiles(CsProjArray SlnProjObj)
        {
            log.Debug("Touch All");

            if (SlnProjObj?.SlnData?.WatchedProjects != null && SlnProjObj.SlnData.WatchedProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.WatchedProjects)
                {
                    if (oneSlnProject != null)
                    {
                        AssemblyInfoFileHandler.changeHandleOneProject(oneSlnProject, 0);

                        //if (oneSlnProject.UpsteamProjects != null)
                        //{
                        //    var now = DateTime.Now;

                        //    foreach (var oneUpstream in oneSlnProject.UpsteamProjects)
                        //    {
                        //        var diff = now.Subtract(oneUpstream.ProjData.LastAssemblyWrite).TotalMilliseconds;

                        //        if (diff > oneUpstream.ProjData.MinSpan)
                        //        {
                        //            AssemblyInfoFileHandler.changeHandleOneProject(oneUpstream.ProjData, oneUpstream.Depth);
                        //        }
                        //    }
                        //}
                    }
                }
            }
            Reporter.ToHuman("Completed Touch All");
            Reporter.WriteMenu();
        }

        #endregion Methods
    }
}