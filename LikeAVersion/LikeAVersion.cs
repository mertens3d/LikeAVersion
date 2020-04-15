using log4net;
using mertensd.LikeAVersion;
using mertensd.LikeAVersion.Build;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.Toucher;
using mertensd.LikeAVersion.Watcher;
using System;

namespace mertens3d.LikeAVersion
{
    public class ShineyAndNew
    {
        #region Properties

        public HeartBeatHub Hub { get; private set; }
        public CsProjArray SlnProjObj { get; set; }
        public AssemToucher Toucher { get; private set; }
        public Watcher Watcher { get; private set; }

        #endregion Properties

        #region Methods

        public void Init()
        {
            this.Hub = new HeartBeatHub();

            var builder = new DataBuilder(Hub);
            SlnProjObj = builder.BuildCurrentState();
            Toucher = new AssemToucher(Hub);
            Watcher = new Watcher(Toucher, Hub);
            Watcher.buildFileWatchForAllInSln(SlnProjObj);
            Hub.Reporter.ListWatched(SlnProjObj);
        }

        public void Stream()
        {
            Init();

            var input = string.Empty;

            while (!input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Hub.Reporter.WriteMenu();

                input = Console.ReadLine();
                if (input.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    Toucher.TouchAllAssemblyFiles(SlnProjObj);
                }
                else if (input.Equals("W", StringComparison.OrdinalIgnoreCase))
                {
                    Hub.Reporter.ListWatched(SlnProjObj);
                }
                else if (input.Equals("i", StringComparison.OrdinalIgnoreCase))
                {
                    Hub.Reporter.ListIgnored(SlnProjObj);
                }
                else if (input.Equals("u", StringComparison.OrdinalIgnoreCase))
                {
                    Hub.Reporter.ListUpStream(SlnProjObj);
                }
            }
        }

        #endregion Methods
    }
}