using Cactus.Fileserver.Model;

namespace Cactus.Fileserver.Storage
{
    public interface IStoredNameProvider
    { 
        /// <summary>
        ///     Returns generated anti name to store file in a storage.
        ///     Its responsibility is to make it unique, randomly and short
        /// </summary>
        /// <param name="info">Meta file info</param>
        /// <returns>Name</returns>
        string GetName(IMetaInfo info);

        /// <summary>
        ///     Called if generated name was not unique and required to be regenerated.
        /// </summary>
        /// <param name="info">File info</param>
        /// <param name="duplicatedName">Previously generated result</param>
        /// <returns>New name</returns>
        string Regenerate(IMetaInfo info, string duplicatedName);
    }
}