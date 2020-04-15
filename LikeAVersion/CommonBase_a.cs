using mertensd.LikeAVersion.Build;

namespace mertensd.LikeAVersion
{
    public abstract class CommonBase_a
    {
        #region Constructors

        public CommonBase_a(HeartBeatHub hub)
        {
            Hub = hub;
        }

        #endregion Constructors

        #region Properties

        public HeartBeatHub Hub { get; }

        #endregion Properties
    }
}