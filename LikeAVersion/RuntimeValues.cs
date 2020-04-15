using log4net;
using System.IO;

namespace mertensd.LikeAVersion
{
    public class RuntimeValues
    {
        #region Constructors

        public RuntimeValues()
        {
            this.Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.AssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo assemblyFile = new FileInfo(AssemblyLocation);
            this.AssemblyDirectory = assemblyFile.DirectoryName;
        }

        #endregion Constructors

        #region Properties

        public string AssemblyLocation { get; }
        public ILog Logger { get; }
        public string AssemblyDirectory { get; }

        #endregion Properties
    }
}