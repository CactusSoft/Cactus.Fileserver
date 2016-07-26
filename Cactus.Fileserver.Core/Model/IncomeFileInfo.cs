namespace Cactus.Fileserver.Core.Model
{
    public class IncomeFileInfo : IFileInfo
    {
        public string MimeType { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public int Size { get; set; }
    }
}
