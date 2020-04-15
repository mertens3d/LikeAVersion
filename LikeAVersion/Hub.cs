using log4net;
using mertensd.LikeAVersion.Feedback;
using mertensd.LikeAVersion.Watcher;
using mertense3d.LikeAVersion.Models;

namespace mertensd.LikeAVersion.Build
{
    public class HeartBeatHub
    {
        public HeartBeatHub()
        {
            this.RunTimeValues = new RuntimeValues();
            this.Logger = RunTimeValues.Logger;
            this.Reporter = new Reporter(this);
            this.WatcherStatus = new WatcherStatus(this);
        }

        public RuntimeValues RunTimeValues { get; }
        public WatcherStatus WatcherStatus { get; }
        public ILog Logger { get; }
        public Reporter Reporter { get; }
        public XmlSettings XmlData { get; internal set; }
    }
}