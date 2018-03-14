using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IStoredNameProvider<T> where T : IFileInfo
    {
        /// <summary>
        ///     Returns generated anti name to store file in a storage.
        ///     Its responsibility is to make it unique, randomly and short
        /// </summary>
        /// <param name="info">Meta file info</param>
        /// <returns></returns>
        string GetName(T info);

        /// <summary>
        ///     Called if generated name was not unique and required to be regenerated.
        /// </summary>
        /// <param name="info">File info</param>
        /// <param name="duplicatedName">Previously generated result</param>
        /// <returns></returns>
        string Regenerate(T info, string duplicatedName);
    }
}