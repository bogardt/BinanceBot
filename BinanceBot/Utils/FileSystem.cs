using BinanceBot.Abstraction;

namespace BinanceBot.Utils
{
    public class FileSystem : IFileSystem
    {
        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public FileInfo[] GetFiles(string path, string searchPattern)
        {
            var directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFiles(searchPattern);
        }
    }
}
