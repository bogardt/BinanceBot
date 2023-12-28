using BinanceBot.Abstraction;

namespace BinanceBot.Utils
{
    public class Logger : ILogger
    {
        public string LogFilePath { get; set; } = $"./logfile-bot-{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}.log";
        private readonly IFileSystem _fileSystem;
        public Logger(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public void WriteLog(string message)
        {
            try
            {
                var logDirectory = _fileSystem.GetDirectoryName(LogFilePath);
                if (!_fileSystem.DirectoryExists(logDirectory))
                {
                    _fileSystem.CreateDirectory(logDirectory);
                }

                using StreamWriter writer = new(LogFilePath, true);

                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log: {ex.Message}");
            }
        }
    }
}
