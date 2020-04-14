using log4net;
using mertens3d.LikeAVersion;
using System;
using System.Linq;

namespace mertensd.LikeAVersion.Feedback
{
    public class Reporter
    {
        #region Fields

        private ILog log;

        #endregion Fields

        #region Constructors

        public Reporter(ILog log)
        {
            this.log = log;
        }

        #endregion Constructors

        #region Methods

        public void ListIgnored(CsProjArray SlnProjObj)
        {
            if (SlnProjObj?.SlnData?.IgnoredProjects != null && SlnProjObj.SlnData.IgnoredProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.IgnoredProjects)
                {
                    ToHuman("Ignoring Project: " + oneSlnProject.CsProjFile.Directory.FullName);// + " filter: " + filter);
                }
            }
        }

        public void ListWatched(CsProjArray SlnProjObj)
        {
            if (SlnProjObj?.SlnData?.WatchedProjects != null && SlnProjObj.SlnData.WatchedProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.WatchedProjects)
                {
                    ToHuman("Watching Project: " + oneSlnProject.CsProjFile.Directory.FullName);// + " filter: " + filter);
                }
            }
        }

        public static void ToHuman(string output)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " : " + output);
        }

        public void WriteMenu()
        {
            ToHuman("q = quit, a = touch all, w = list watched, i = list ignored");
        }

        internal void ListUpStream(CsProjArray SlnProjObj)
        {
            if (SlnProjObj?.SlnData?.WatchedProjects != null && SlnProjObj.SlnData.WatchedProjects.Any())
            {
                foreach (var oneSlnProject in SlnProjObj.SlnData.WatchedProjects)
                {
                    ToHuman("");
                    ToHuman("Watching Project: " + oneSlnProject.CsProjFile.Directory.Name);

                    foreach (var oneUpStream in oneSlnProject.UpsteamProjects)
                    {
                        var indent = "\t\t";
                        for (int idx = 0; idx < oneUpStream.Depth; idx++)
                        {
                            indent += "\t\t";
                        }

                        ToHuman(indent + oneUpStream.Depth + " : " + oneUpStream.ProjData.CsProjFile.Directory.Name);
                    }
                }
            }
        }

        #endregion Methods
    }
}