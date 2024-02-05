namespace BinanceBot.Abstraction;

public interface IFileSystem
{
    string GetCurrentDirectory();
    string GetDirectoryName(string path);
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    FileInfo[] GetFiles(string path, string searchPattern);
}
