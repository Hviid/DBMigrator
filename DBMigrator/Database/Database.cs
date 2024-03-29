﻿using System;
using System.Data.SqlClient;
using DBMigrator.Model;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using DBMigrator.SQL;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DBMigrator
{
    public class Database : IDatabase
    {
        public SqlConnection Sqlconn;
        private SqlTransaction trans;
        private readonly ILogger<Database> _logger;
        private Regex _goRegex = new Regex(@"[\n\r]GO\b");


        public Database(string servername, string database, string username, string password, string options)
        {
            var loggerFactory = Bootstrapper.GetConfiguredServiceProvider().GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<Database>();

            string connectionString;
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                connectionString = $"Application Name=DbMigrator;Data Source={servername};Initial Catalog={database};Integrated Security=true;MultipleActiveResultSets=True;" + options;
            }
            else
            {
                connectionString = $"Application Name=DbMigrator;Data Source={servername};Initial Catalog={database};Persist Security Info=True;User ID={username};Password={password};MultipleActiveResultSets=True;" + options;
            }
            SetupConnAndLogger(connectionString);
        }

        private void SetupConnAndLogger(string connectionString)
        {
            Sqlconn = new SqlConnection(connectionString);
            Sqlconn.InfoMessage += Sqlconn_InfoMessage;
        }

        private void Sqlconn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            _logger.LogInformation(e.Message);
        }

        private void CreateDBVersionTable()
        {
            _logger.LogInformation("Creating DBVersion table");
            ExecuteSingleCommand(MigratorModelScripts.CreateDBVersionScriptsTable);
        }

        public List<DBVersion> GetDBState()
        {
            Sqlconn.Open();
            ExecuteSingleCommand(ChecksumScripts.DropCustomHasbytesFunction);
            ExecuteSingleCommand(ChecksumScripts.CreateCustomHashbytesFunction);
            SqlDataReader reader;
            var result = new List<DBVersion>();
            try
            {
                ExecuteSingleCommand(MigratorModelScripts.TestModelAndUpgrade);
                reader = ExecuteCommand(MigratorModelScripts.SelectDBVersionScriptsScript);
            }
            catch (Exception)
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

                    var script = dbversion.AddAndOrGetFeature(feature, 0).AddUpgradeScript(scriptFileName, order);
                    script.Checksum = checksum;
                    script.ExecutionTime = executiontime;
                }
            }
            Sqlconn.Close();
            return result;
        }

        public (byte[] databaseTriggersChecksum, 
            byte[] DatabaseTablesAndViewsChecksum, 
            byte[] DatabaseFunctionsChecksum, 
            byte[] DatabaseStoredProceduresChecksum, 
            byte[] DatabaseIndexesChecksum) GetLatestMigrationChecksums()
        {
            Sqlconn.Open();
            var data = ExecuteCommand("SELECT TOP 1 DatabaseTriggersChecksum, " +
                "DatabaseTablesAndViewsChecksum, " +
                "DatabaseFunctionsChecksum, " +
                "DatabaseStoredProceduresChecksum, " +
                "DatabaseIndexesChecksum FROM [DBVersionScripts] order by [Date] desc");

            if (!data.Read())
            {
                Sqlconn.Close();
                return (null, null, null, null, null);
            }

            var databaseTriggersChecksum = data.GetValue(0).GetType() == typeof(DBNull) ? null: (byte[])data.GetValue(0);
            var DatabaseTablesAndViewsChecksum = data.GetValue(1).GetType() == typeof(DBNull) ? null : (byte[])data.GetValue(1);
            var DatabaseFunctionsChecksum = data.GetValue(2).GetType() == typeof(DBNull) ? null : (byte[])data.GetValue(2);
            var DatabaseStoredProceduresChecksum = data.GetValue(3).GetType() == typeof(DBNull) ? null : (byte[])data.GetValue(3);
            var DatabaseIndexesChecksum = data.GetValue(4).GetType() == typeof(DBNull) ? null : (byte[])data.GetValue(4);

            Sqlconn.Close();
            return (databaseTriggersChecksum, DatabaseTablesAndViewsChecksum, DatabaseFunctionsChecksum, DatabaseStoredProceduresChecksum, DatabaseIndexesChecksum);
        }

        public (byte[] databaseTriggersChecksum,
            byte[] DatabaseTablesAndViewsChecksum,
            byte[] DatabaseFunctionsChecksum,
            byte[] DatabaseStoredProceduresChecksum,
            byte[] DatabaseIndexesChecksum) GetDatabaseCurrentChecksums()
        {
            BeginTransaction();

            byte[] currentFunctionsChecksum = null;
            using (var reader = ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.FunctionsChecksum))){
                reader.Read();
                if (!reader.IsDBNull(0))
                    currentFunctionsChecksum = ((byte[])reader.GetValue(0));
            }

            byte[] currentIndexesChecksum = null;
            using (var reader = ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.IndexesChecksum))) {
                reader.Read();
                if (!reader.IsDBNull(0))
                    currentIndexesChecksum = ((byte[])reader.GetValue(0));
            }

            byte[] currentStoredProceduresChecksum = null;
            using (var reader = ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.StoredProceduresChecksum)))
            {
                reader.Read();
                if (!reader.IsDBNull(0))
                    currentStoredProceduresChecksum = ((byte[])reader.GetValue(0));
            }

            byte[] currentTablesViewsAndColumnsChecksum = null;
            using (var reader = ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.TablesViewsAndColumnsChecksumScript)))
            {
                reader.Read();
                if (!reader.IsDBNull(0))
                    currentTablesViewsAndColumnsChecksum = ((byte[])reader.GetValue(0));
            }

            byte[] currentTriggersChecksum = null;
            using (var reader = ExecuteCommand(ChecksumScripts.GetHashbytesFor(ChecksumScripts.TriggersChecksum)))
            {
                reader.Read();
                if (!reader.IsDBNull(0))
                    currentTriggersChecksum = ((byte[])reader.GetValue(0));
            }

            CommitTransaction();

            return (currentTriggersChecksum, currentTablesViewsAndColumnsChecksum, currentFunctionsChecksum, currentStoredProceduresChecksum, currentIndexesChecksum);
        }

        private void ExecuteMultipleCommands(IEnumerable<string> cmds)
        {
            var alreadyOpen = Sqlconn.State == System.Data.ConnectionState.Open;
            if (!alreadyOpen)
                Sqlconn.Open();
            try
            {
                foreach (var cmd in cmds)
                {
                    using (ExecuteCommand(cmd)) { }
                }
            }
            finally
            {
                if (!alreadyOpen)
                    Sqlconn.Close();
            }
        }

        public void ExecuteUpgradeCommand(string version, string feature, string scriptFile, string cmd)
        {
            ExecuteMultipleCommands(BatchByGoStatement(cmd));
        }

        public void ExecuteSingleCommand(string cmd)
        {
            ExecuteMultipleCommands(BatchByGoStatement(cmd));
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
        
        private IEnumerable<string> BatchByGoStatement(string sqltext)
        {
            return _goRegex.Split(sqltext).Where(cmd => !string.IsNullOrWhiteSpace(cmd));
        }

        public void FixChecksum(UpgradeScript sourceScript)
        {
            Sqlconn.Open();
            var script = MigratorModelScripts.GetChecksumUpdateScript(sourceScript.Feature.Version.Name, sourceScript.Order, sourceScript.Feature.Name, sourceScript.Checksum);
            ExecuteCommand(script);
            Sqlconn.Close();
        }
    }
}
