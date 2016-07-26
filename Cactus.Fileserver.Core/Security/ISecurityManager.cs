using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Security
{
    public interface ISecurityManager
    {
        bool MayCreate(IFileInfo file);

        bool MayDelete(IFileInfo file);

        bool MayRead(IFileInfo info);
    }
}
