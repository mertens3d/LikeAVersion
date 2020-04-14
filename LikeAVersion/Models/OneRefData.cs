namespace mertensd.LikeAVersion.Models
{
    public class OneRefData
    {
        #region Constructors

        public OneRefData()
        {
        }

        #endregion Constructors

        #region Properties

        public string Include { get; set; } = string.Empty;
        public string ProjectGuidAsString { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;

        #endregion Properties
    }
}