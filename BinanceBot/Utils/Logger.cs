using BinanceBot.Abstraction;

namespace BinanceBot.Utils
{
    public class Logger : ILogger
    {
        private static readonly string _logFilePath = "./logfile.log";

        public void WriteLog(string message)
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                using StreamWriter writer = new StreamWriter(_logFilePath, true);

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
