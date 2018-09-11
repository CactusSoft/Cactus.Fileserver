using Cactus.Fileserver.Model;

namespace Cactus.Fileserver.Security
{
    public interface ISecurityManager
    {
        bool MayCreate(IFileInfo file);

        bool MayDelete(IFileInfo file);

        bool MayRead(IFileInfo info);
    }
}