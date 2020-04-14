namespace mertensd.LikeAVersion.Models
{
    public class OneUpStreamProj
    {
        #region Constructors

        public OneUpStreamProj(ProjectData projData, int depth)
        {
            this.ProjData = projData;
            this.Depth = depth;
        }

        #endregion Constructors

        #region Properties

        public int Depth { get; set; }
        public ProjectData ProjData { get; set; }

        #endregion Properties
    }
}