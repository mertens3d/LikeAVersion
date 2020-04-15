using mertens3d.LikeAVersion;
using mertensd.LikeAVersion.Build;
using mertensd.LikeAVersion.Feedback;
using System.Linq;

namespace mertensd.LikeAVersion.Toucher
{
    public class AssemToucher : CommonBase_a
    {
        #region Fields

        private AssemblyInfoFileHandler _assemblyInfoFileHandler;

        public AssemToucher(HeartBeatHub hub) : base(hub)
        {
        }

        #endregion Fields

        #region Properties

        public AssemblyInfoFileHandler AssemblyInfoFileHandler
        {
            get
            {
                return _assemblyInfoFileHandler ?? (_assemblyInfoFileHandler = new AssemblyInfoFileHandler(Hub));
            }
        }

        private Reporter Reporter { get; }

        #endregion Properties

        #region Methods

        public void TouchAllAssemblyFiles(CsProjArray SlnProjObj)
        {
            Hub.Reporter.ToHuman("Touch All");

            if (SlnProjObj?.SlnData?.WatchedProjects != null && SlnProjObj.SlnData.WatchedProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.WatchedProjects)
                {
                    if (oneSlnProject != null)
                    {
                        AssemblyInfoFileHandler.UpdateOneProject(oneSlnProject, 0);
                    }
                }
            }
            Reporter.ToHuman("Completed Touch All");
            Reporter.WriteMenu();
        }

        #endregion Methods
    }
}