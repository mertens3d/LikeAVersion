using mertens3d.LikeAVersion;
using System.Security.Permissions;

namespace mertense3d.LikeAVersion
{
    internal class Program
    {
        //private static readonly log4net.ILog Hub.Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Main(string[] args)
        {
            mertens3d.LikeAVersion.ShineyAndNew csVersion = new mertens3d.LikeAVersion.ShineyAndNew();
            csVersion.Stream();
        }
    }
}