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

            if (!ByteArraysEqual(latest.DatabaseFunctionsChecksum, current.DatabaseFunctionsChecksum ))
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {System.Text.Encoding.UTF8.GetString(current.DatabaseFunctionsChecksum)} does not match latest migration checksum {System.Text.Encoding.UTF8.GetString(latest.DatabaseFunctionsChecksum)}");
            if (!ByteArraysEqual(latest.DatabaseIndexesChecksum, current.DatabaseIndexesChecksum))
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {System.Text.Encoding.UTF8.GetString(current.DatabaseIndexesChecksum)} does not match latest migration checksum {System.Text.Encoding.UTF8.GetString(latest.DatabaseIndexesChecksum)}");
            if (!ByteArraysEqual(latest.DatabaseStoredProceduresChecksum, current.DatabaseStoredProceduresChecksum))
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {System.Text.Encoding.UTF8.GetString(current.DatabaseStoredProceduresChecksum)} does not match latest migration checksum {System.Text.Encoding.UTF8.GetString(latest.DatabaseStoredProceduresChecksum)}");
            if (!ByteArraysEqual(latest.DatabaseTablesAndViewsChecksum, current.DatabaseTablesAndViewsChecksum))
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {System.Text.Encoding.UTF8.GetString(current.DatabaseTablesAndViewsChecksum)} does not match latest migration checksum {System.Text.Encoding.UTF8.GetString(latest.DatabaseTablesAndViewsChecksum)}");
            if (!ByteArraysEqual(latest.databaseTriggersChecksum, current.databaseTriggersChecksum))
                throw new Exception($"DatabaseFunctionsChecksum exception current database checksum {System.Text.Encoding.UTF8.GetString(current.databaseTriggersChecksum)} does not match latest migration checksum {System.Text.Encoding.UTF8.GetString(latest.databaseTriggersChecksum)}");
        }
        private bool ByteArraysEqual(byte[] array1, byte[] array2)
        {
            return array1 == null && array2 == null || array1 != null && array2 != null && array1.SequenceEqual(array2);
            
        }
    }
}
