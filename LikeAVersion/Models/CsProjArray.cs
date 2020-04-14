using mertensd.LikeAVersion.Models;
using mertense3d.LikeAVersion.Models;
using System.Collections.Generic;

namespace mertens3d.LikeAVersion
{
    public class CsProjArray
    {
        #region Properties

        public Targets AllTargets { get; set; }

        public SlnData SlnData { get; set; } = new SlnData();

        public List<string> targetProjectNames { get; set; }

        #endregion Properties
    }
}