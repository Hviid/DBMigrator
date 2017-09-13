using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBMigrator
{
    public static class LoggerExtensions
    {
        public static void LogError(this ILogger logger, Exception ex, string message = null, params object[] args)
        {
            logger.LogError(default(EventId), ex, message, args);
        }

        public static void LogCritical(this ILogger logger, Exception ex, string message = null, params object[] args)
        {
            logger.LogCritical(default(EventId), ex, message, args);
        }
    }
}
