using DBMigrator.Model;
using DBMigrator.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class DatabaseValidator
    {
        private readonly Database _database;

        public DatabaseValidator(Database database)
        {
            _database = database;
        }

        public void Validate()
        {
            var latest = _database.GetLatestMigrationChecksums();
            if (latest.DatabaseTablesAndViewsChecksum == null)
                return;

            var current = _database.GetDatabaseCurrentChecksums();

            if (latest.DatabaseFunctionsChecksum != current.DatabaseFunctionsChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {current.DatabaseFunctionsChecksum} does not match latest migration checksum {latest.DatabaseFunctionsChecksum}");
            if (latest.DatabaseIndexesChecksum != current.DatabaseIndexesChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {current.DatabaseIndexesChecksum} does not match latest migration checksum {latest.DatabaseIndexesChecksum}");
            if (latest.DatabaseStoredProceduresChecksum != current.DatabaseStoredProceduresChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {current.DatabaseStoredProceduresChecksum} does not match latest migration checksum {latest.DatabaseStoredProceduresChecksum}");
            if (latest.DatabaseTablesAndViewsChecksum != current.DatabaseTablesAndViewsChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {current.DatabaseTablesAndViewsChecksum} does not match latest migration checksum {latest.DatabaseTablesAndViewsChecksum}");
            if (latest.databaseTriggersChecksum != current.databaseTriggersChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {current.databaseTriggersChecksum} does not match latest migration checksum {latest.databaseTriggersChecksum}");
        }
    }
}
