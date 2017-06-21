using System;
using System.Data.SqlClient;
using DBMigrator.Model;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using DBMigrator.SQL;

namespace DBMigrator
{
    public class Database : IDisposable
    {
        public SqlConnection Sqlconn;
        private SqlTransaction trans;
        private Logger _logger;


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

        private void CreateDBVersionTable()
        {
            _logger.Log("Creating DBVersion table");
            ExecuteSingleCommand(MigratorModelScripts.CreateDBVersionScriptsTable);
        }

        public List<DBVersion> GetDBState()
        {
            Sqlconn.Open();
            SqlDataReader reader;
            var result = new List<DBVersion>();
            try
            {
                reader = ExecuteCommand(MigratorModelScripts.SelectDBVersionScriptsScript);
            }
            catch (Exception ex)
            {
                Sqlconn.Close();
                CreateDBVersionTable();
                return result;
            }

            while (reader.Read())
            {
                var version = reader.GetString(0);

                var dbversion = result.FirstOrDefault(v => v.Name == version);
                if (dbversion == null)
                {
                    dbversion = new DBVersion(version);
                    result.Add(dbversion);
                }

                string feature = null;
                if (!reader.IsDBNull(1))
                {
                    feature = reader.GetString(1);
                    var order = reader.GetInt32(2);
                    var scriptFileName = reader.GetString(3);
                    var type = reader.GetString(4);
                    var checksum = reader.GetString(5);
                    var executiontime = reader.GetInt32(6);

                    var script = dbversion.AddAndOrGetFeature(feature).AddUpgradeScript(scriptFileName, order);
                    script.Checksum = checksum;
                }
            }
            Sqlconn.Close();
            return result;
        }

        public (byte[] databaseTriggersChecksum, byte[] DatabaseTablesAndViewsChecksum, byte[] DatabaseFunctionsChecksum, byte[] DatabaseStoredProceduresChecksum, byte[] DatabaseIndexesChecksum) GetLatestMigrationChecksums()
        {
            Sqlconn.Open();
            var data = ExecuteCommand("SELECT TOP 1 DatabaseTriggersChecksum, " +
                "DatabaseTablesAndViewsChecksum, " +
                "DatabaseFunctionsChecksum, " +
                "DatabaseStoredProceduresChecksum, " +
                "DatabaseIndexesChecksum FROM [DBVersionScripts] order by [Date] desc");

            if (!data.Read())
                return (null, null, null, null, null);

            var databaseTriggersChecksum = (byte [])data.GetValue(0);
            var DatabaseTablesAndViewsChecksum = (byte[])data.GetValue(1);
            var DatabaseFunctionsChecksum = (byte[])data.GetValue(2);
            var DatabaseStoredProceduresChecksum = (byte[])data.GetValue(3);
            var DatabaseIndexesChecksum = (byte[])data.GetValue(4);

            Sqlconn.Close();
            return (databaseTriggersChecksum, DatabaseTablesAndViewsChecksum, DatabaseFunctionsChecksum, DatabaseStoredProceduresChecksum, DatabaseIndexesChecksum);
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
