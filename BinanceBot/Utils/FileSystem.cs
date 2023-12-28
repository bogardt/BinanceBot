using BinanceBot.Abstraction;

namespace BinanceBot.Utils
{
    public class FileSystem : IFileSystem
    {
        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public FileInfo[] GetFiles(string path, string searchPattern)
        {
            var directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFiles(searchPattern);
        }
    }
}
