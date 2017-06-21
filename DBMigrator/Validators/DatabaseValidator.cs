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
            _database.BeginTransaction();

            var reader = _database.ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.FunctionsChecksum));
            reader.Read();
            var currentFunctionsChecksum = ((byte [])reader.GetValue(0));
            reader.Close();

            reader = _database.ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.IndexesChecksum));
            reader.Read();
            var currentIndexesChecksum = ((byte[])reader.GetValue(0));
            reader.Close();

            reader = _database.ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.StoredProceduresChecksum));
            reader.Read();
            var currentStoredProceduresChecksum = ((byte[])reader.GetValue(0));
            reader.Close();
            
            reader = _database.ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.TablesViewsAndColumnsChecksumScript));
            reader.Read();
            var currentTablesViewsAndColumnsChecksum = ((byte[])reader.GetValue(0));
            reader.Close();
            
            reader = _database.ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.TriggersChecksum));
            reader.Read();
            var currentTriggersChecksum = ((byte[])reader.GetValue(0));
            reader.Close();

            _database.CommitTransaction();

            var latest =  _database.GetLatestMigrationChecksums();
            if (latest.DatabaseTablesAndViewsChecksum == null)
                return;
            
            if (latest.DatabaseFunctionsChecksum != currentFunctionsChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentFunctionsChecksum} does not match latest migration checksum {latest.DatabaseFunctionsChecksum}");
            if (latest.DatabaseIndexesChecksum != currentIndexesChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentIndexesChecksum} does not match latest migration checksum {latest.DatabaseIndexesChecksum}");
            if (latest.DatabaseStoredProceduresChecksum != currentStoredProceduresChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentStoredProceduresChecksum} does not match latest migration checksum {latest.DatabaseStoredProceduresChecksum}");
            if (latest.DatabaseTablesAndViewsChecksum != currentTablesViewsAndColumnsChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentTablesViewsAndColumnsChecksum} does not match latest migration checksum {latest.DatabaseTablesAndViewsChecksum}");
            if (latest.databaseTriggersChecksum != currentTriggersChecksum)
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {currentTriggersChecksum} does not match latest migration checksum {latest.databaseTriggersChecksum}");
        }
    }
}
