using Cactus.Fileserver.Model;

namespace Cactus.Fileserver.Security
{
    public class NothingCheckSecurityManager : ISecurityManager
    {
        public bool MayCreate(IFileInfo file)
        {
            return true;
        }

        public bool MayDelete(IFileInfo file)
        {
            return true;
        }

        public bool MayRead(IFileInfo info)
        {
            return true;
        }
    }
}