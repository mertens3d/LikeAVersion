using mertensd.LikeAVersion.Models;
using System.Collections.Generic;

namespace mertens3d.LikeAVersion
{
    public class CsProjArray
    {
        #region Properties

        public SlnData SlnData { get; set; } = new SlnData();

        public List<string> targetProjectNames { get; set; }

        #endregion Properties
    }
}