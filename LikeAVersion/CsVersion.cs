using log4net;
using mertensd.LikeAVersion.Build;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.Toucher;
using mertensd.LikeAVersion.Watcher;
using System;

namespace mertens3d.LikeAVersion
{
    public class CsVersion
    {
        #region Fields

        private ILog log;

        #endregion Fields

        #region Properties

        public string __dirname { get; private set; }

        public Reporter Reporter { get; private set; }
        public CsProjArray SlnProjObj { get; set; }
        public AssemToucher Toucher { get; private set; }
        public Watcher Watcher { get; private set; }

        #endregion Properties

        #region Methods

        public void Init()
        {
            Reporter = new Reporter(log);
            var builder = new DataBuilder(log, Reporter);
            SlnProjObj = builder.BuildCurrentState();
            Toucher = new AssemToucher(log, Reporter);
            Watcher = new Watcher(Toucher, Reporter, log);
            Watcher.buildFileWatchForAllInSln(SlnProjObj);
            Reporter.ListWatched(SlnProjObj);
        }

        public void Stream(ILog log)
        {
            this.log = log;

            Init();

            var input = string.Empty;

            while (!input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Reporter.WriteMenu();

                input = Console.ReadLine();
                if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    Toucher.TouchAllAssemblyFiles(SlnProjObj);
                }
                else if (input.Equals("W", StringComparison.OrdinalIgnoreCase))
                {
                    Reporter.ListWatched(SlnProjObj);
                }
                else if (input.Equals("i", StringComparison.OrdinalIgnoreCase))
                {
                    Reporter.ListIgnored(SlnProjObj);
                }
                else if (input.Equals("u", StringComparison.OrdinalIgnoreCase))
                {
                    Reporter.ListUpStream(SlnProjObj);
                }
            }
        }

        #endregion Methods
    }
}