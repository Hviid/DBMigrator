using DBMigrator.Model;
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
            _database.BeginTransaction();
            var currentFunctionsChecksum = _database.GetFunctionsChecksum();
            var currentIndexesChecksum = _database.GetIndexesChecksum();
            var currentStoredProceduresChecksum = _database.GetStoredProceduresChecksum();
            var currentTablesViewsAndColumnsChecksum = _database.GetTablesViewsAndColumnsChecksum();
            var currentTriggersChecksum = _database.GetTriggersChecksum();

            _database.CommitTransaction();

            var latest =  _database.GetLatestMigrationChecksums();

            if (latest.DatabaseFunctionsChecksum != currentFunctionsChecksum.ToString())
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentFunctionsChecksum} does not match latest migration checksum {latest.DatabaseFunctionsChecksum}");
            if (latest.DatabaseIndexesChecksum != currentIndexesChecksum.ToString())
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentIndexesChecksum} does not match latest migration checksum {latest.DatabaseIndexesChecksum}");
            if (latest.DatabaseStoredProceduresChecksum != currentStoredProceduresChecksum.ToString())
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentStoredProceduresChecksum} does not match latest migration checksum {latest.DatabaseStoredProceduresChecksum}");
            if (latest.DatabaseTablesAndViewsChecksum != currentTablesViewsAndColumnsChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentTablesViewsAndColumnsChecksum} does not match latest migration checksum {latest.DatabaseTablesAndViewsChecksum}");
            if (latest.databaseTriggersChecksum != currentTriggersChecksum.ToString())
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentTriggersChecksum} does not match latest migration checksum {latest.databaseTriggersChecksum}");
        }
    }
}
