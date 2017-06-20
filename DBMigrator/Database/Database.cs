using System;
using System.Data.SqlClient;
using DBMigrator.Model;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace DBMigrator
{
    public class Database : IDisposable
    {
        public SqlConnection Sqlconn;
        private SqlTransaction trans;
        private Logger _logger;
        private DatabaseSchema _databaseSchema;
        private DatabaseFuncViewStoredProcedureTrigger _databaseFuncViewStoredProcedureTrigger;
        private DatabaseData _databaseData;


        public Database(string servername, string database, string username, string password)
        {
            string connectionString;
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                connectionString = $"Data Source={servername};Initial Catalog={database};Integrated Security=true;MultipleActiveResultSets=True";
            }
            else
            {
                connectionString = $"Data Source={servername};Initial Catalog={database};Persist Security Info=True;User ID={username};Password={password};MultipleActiveResultSets=True";
            }
            SetupConnAndLogger(connectionString);
            _databaseSchema = new DatabaseSchema(this);
            _databaseFuncViewStoredProcedureTrigger = new DatabaseFuncViewStoredProcedureTrigger(this);
            _databaseData = new DatabaseData(this);
        }

        public Database(string initialCatalog) // string mdfFilePath, 
        {
            var connectionString = $@"Data Source=(localdb)\v11.0;Integrated Security=True;User Instance=False;Initial Catalog={initialCatalog}";
            SetupConnAndLogger(connectionString);
        }
        private void SetupConnAndLogger(string connectionString)
        {
            _logger = Bootstrapper.GetConfiguredServiceProvider().GetRequiredService<Logger>();
            Sqlconn = new SqlConnection(connectionString);
        }

        public List<DBVersion> GetDBState()
        {
            var versions = _databaseSchema.GetDBState();
            _databaseFuncViewStoredProcedureTrigger.AppendDatabaseFuncViewStoredProcedureTriggerState(versions);
            return versions;
        }

        public (string databaseTriggersChecksum, string DatabaseTablesAndViewsChecksum, string DatabaseFunctionsChecksum, string DatabaseStoredProceduresChecksum, string DatabaseIndexesChecksum) GetLatestMigrationChecksums()
        {
            return _databaseSchema.GetLatestMigrationChecksums();
        }

        public void UpgradeSchema(UpgradeScript script)
        {
            _databaseSchema.UpdateDataWithFile(script);
        }

        public void DowngradeShema(DowngradeScript script)
        {
            _databaseSchema.DowngradeDataWithFile(script);
        }

        public void UpgradeFuncViewStoredProcedureTrigger(FuncViewStoredProcedureTriggerScript script)
        {
            _databaseFuncViewStoredProcedureTrigger.UpdateDataWithFile(script);
        }

        public void UpdateDatabaseVersion(DBVersion version)
        {
            var versionStr = version.Version.ToString();
            _logger.Log($"Updating DBVersion version to {versionStr}");
        }

        public void ExecuteSingleCommand(string cmd)
        {
            Sqlconn.Open();
            try
            {
                ExecuteCommand(cmd);
            }
            finally
            {
                Sqlconn.Close();
            }
        }

        public SqlDataReader ExecuteCommand(string cmd)
        {
            using (SqlCommand command = new SqlCommand(cmd, Sqlconn, trans))
            {
                command.CommandTimeout = 0;
                var result = command.ExecuteReader();
                return result;
            }
        }

        public void BeginTransaction()
        {
            Sqlconn.Open();
            trans = Sqlconn.BeginTransaction();
        }

        public void CommitTransaction()
        {
            trans.Commit();
            Sqlconn.Close();
        }

        public void RollbackTransaction()
        {
            trans.Rollback();
            Sqlconn.Close();
        }

        public void Close()
        {
            Sqlconn.Close();
        }

        public void Dispose()
        {
            if(Sqlconn.State != System.Data.ConnectionState.Closed)
                Sqlconn.Close();
        }
    }
}
