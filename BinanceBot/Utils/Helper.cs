namespace BinanceBot.Utils
{
    public static class Helper
    {
        public static string GetSolutionPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var directoryInfo = new DirectoryInfo(currentDirectory);

            while (directoryInfo != null && !directoryInfo.GetFiles("*.sln").Any())
                directoryInfo = directoryInfo.Parent;

            var solutionPath = directoryInfo?.FullName;

            return solutionPath;
        }
    }
}
