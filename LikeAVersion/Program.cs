using System.Security.Permissions;

namespace LikeAVersion
{
    internal class Program
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Main(string[] args)
        {
            CsVersion csVersion = new CsVersion();
            csVersion.Stream(_log);
        }
    }
}