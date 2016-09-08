using Microsoft.Extensions.Logging;
using System.Text;

namespace DBMigrator
{
    public class Logger
    {
        private static Logger _instance;

        private ILogger<Migrator> _logger;
        public StringBuilder log = new StringBuilder();

        public Logger(ILogger<Migrator> logger) {
            _logger = logger;
            _instance = this;
        }

        public static Logger GetInstance()
        {
            return _instance;
        }

        public void Log(string text)
        {
            _logger.LogInformation(text);
            log.AppendLine(text);
        }
    }
}
