using BinanceBot.Abstraction;

namespace BinanceBot.Utils;

public class Helper
{
    private readonly IFileSystem _fileSystem;

    public Helper(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string GetSolutionPath()
    {
        var currentDirectory = _fileSystem.GetCurrentDirectory();

        var directoryInfo = new DirectoryInfo(currentDirectory);

        while (directoryInfo != null && !_fileSystem.GetFiles(directoryInfo.FullName, "*.sln").Any())
            directoryInfo = directoryInfo.Parent;

        return directoryInfo?.FullName;
    }
}
