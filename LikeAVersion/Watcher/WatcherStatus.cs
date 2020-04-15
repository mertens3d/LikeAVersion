using mertensd.LikeAVersion.Build;
using System.Threading;

namespace mertensd.LikeAVersion.Watcher
{
    public class WatcherStatus : CommonBase_a
    {
        #region Constructors

        public WatcherStatus(HeartBeatHub hub) : base(hub)
        {
        }

        #endregion Constructors

        #region Properties

        private bool ActiveWatchIsOn { get; set; } = true;

        #endregion Properties

        #region Methods

        public void PauseFileWatch()
        {
            ActiveWatchIsOn = false;
            Hub.Reporter.ToHuman("Watching paused");
        }

        public void ResumeFileWatch()
        {
            Hub.Reporter.ToHuman("About to resume watch (after sleep <SleepBeforeResumeWatchMSec>  )");
            Thread.Sleep(Hub.XmlData.SleepBeforeResumeWatchMSec); //this doesn't work as intended

            ActiveWatchIsOn = true;
            Hub.Reporter.ToHuman("Watching resumed");
        }

        public bool WatchIsEnabled()
        {
            return ActiveWatchIsOn;
        }

        #endregion Methods
    }
}