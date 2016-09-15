using Microsoft.Extensions.Logging;
using System.Text;

namespace DBMigrator
{
    public class Logger
    {
        private static Logger _instance;

        private ILogger<Migrator> _logger;
        public StringBuilder log = new StringBuilder();

        public Logger(ILoggerFactory factory) {
            _logger = factory.CreateLogger<Migrator>();
            _instance = this;
        }

        public void Log(string text)
        {
            _logger.LogInformation(text);
            log.AppendLine(text);
        }
    }
}
