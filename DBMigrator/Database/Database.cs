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
            var connectionString = $"Data Source={servername};Initial Catalog={database};Persist Security Info=True;User ID={username};Password={password};MultipleActiveResultSets=True";
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
            //var data = ExecuteCommand($"INSERT INTO DBVersion (Version, Date, Log) OUTPUT Inserted.ID VALUES ('{versionStr}', GETUTCDATE(), '<xml>' + CHAR(13) + '{_logger.log.ToString()}</xml>')");
            
            //using (data)
            //{
            //    data.Read();
            //    version.ID = data.GetInt32(0);
            //}
        }

        public void UpdateLog(DBVersion version)
        {
            //ExecuteCommand($"UPDATE DBVersion SET Log = '<xml>' + CHAR(13) + '{_logger.log.ToString()}</xml>' WHERE ID = {version.ID}");
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

        
        //http://www.bidn.com/blogs/TomLannen/bidn-blog/2265/using-hashbytes-to-compare-columns
        public string GetTablesViewsAndColumnsChecksum()
        {
            var query = @"SELECT HASHBYTES('SHA1', TABLE_SCHEMA + '|' 
						+ DATA_TYPE + '|' 
						+ TABLE_NAME + '|' 
						+ COLUMN_NAME + '|' 
						+ CAST(ISNULL(NUMERIC_PRECISION, 0) as varchar(max)) + '|' 
						+ CAST(ISNULL(DATETIME_PRECISION, 0) as varchar(max)) + '|' 
						+ CAST(ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) as varchar(max)) + '|' 
                        ) FROM INFORMATION_SCHEMA.COLUMNS";
            
            return CheckSumHelper2(query);
        }

        public int GetStoredProceduresChecksum()
        {
            var query = @"SELECT
                        CHECKSUM_AGG(CHECKSUM
                        ([SPECIFIC_CATALOG]
                              , [SPECIFIC_SCHEMA]
                              , [SPECIFIC_NAME]
                              , [ROUTINE_CATALOG]
                              , [ROUTINE_SCHEMA]
                              , [ROUTINE_NAME]
                              , [ROUTINE_TYPE]
                              , [DATA_TYPE]
                              , [CHARACTER_MAXIMUM_LENGTH]
                              , [CHARACTER_OCTET_LENGTH]
                              , [NUMERIC_PRECISION]
                              , [DATETIME_PRECISION]
                              , [ROUTINE_BODY]
                              , [ROUTINE_DEFINITION]
                              , [IS_DETERMINISTIC]
                              , [SQL_DATA_ACCESS]
                              , [IS_NULL_CALL])) as StoredProceduresChecksum
                         FROM [INFORMATION_SCHEMA].[ROUTINES]
                        WHERE ROUTINE_TYPE = 'PROCEDURE'";

            return CheckSumHelper(query);
        }

        public int GetFunctionsChecksum()
        {
            var query = @"SELECT
                        CHECKSUM_AGG(CHECKSUM
                        ([SPECIFIC_CATALOG]
                              , [SPECIFIC_SCHEMA]
                              , [SPECIFIC_NAME]
                              , [ROUTINE_CATALOG]
                              , [ROUTINE_SCHEMA]
                              , [ROUTINE_NAME]
                              , [ROUTINE_TYPE]
                              , [DATA_TYPE]
                              , [CHARACTER_MAXIMUM_LENGTH]
                              , [CHARACTER_OCTET_LENGTH]
                              , [NUMERIC_PRECISION]
                              , [DATETIME_PRECISION]
                              , [ROUTINE_BODY]
                              , [ROUTINE_DEFINITION]
                              , [IS_DETERMINISTIC]
                              , [SQL_DATA_ACCESS]
                              , [IS_NULL_CALL])) as FunctionsChecksum
                         FROM [INFORMATION_SCHEMA].[ROUTINES]
                        WHERE ROUTINE_TYPE = 'FUNCTION'";

            return CheckSumHelper(query);
        }

        public int GetTriggersChecksum()
        {
            var query = @"SELECT
                        CHECKSUM_AGG(CHECKSUM
                        ([name]
                              , [sys].[all_objects].[object_id]
                              , [principal_id]
                              , [schema_id]
                              , [parent_object_id]
                              , [type]
                              , [type_desc]
                              , [is_ms_shipped]
                              , [is_published]
                              , [is_schema_published]
                              , [definition])) as TriggersChecksum
                        FROM[sys].[all_objects]
                        INNER JOIN[sys].[sql_modules]
                        ON[sys].[sql_modules].[object_id] = [sys].[all_objects].[object_id]
                        WHERE type = 'TR'";

            return CheckSumHelper(query);
        }

        public int GetIndexesChecksum()
        {
            var query = @"SELECT
                        CHECKSUM_AGG(CHECKSUM
                        ([CONSTRAINT_CATALOG]
                          ,[CONSTRAINT_SCHEMA]
                          ,[CONSTRAINT_NAME]
                          ,[TABLE_CATALOG]
                          ,[TABLE_SCHEMA]
                          ,[TABLE_NAME]
                          ,[CONSTRAINT_TYPE]
                          ,[IS_DEFERRABLE]
                          ,[INITIALLY_DEFERRED])) as IndexesChecksum
                        FROM [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS]";

            return CheckSumHelper(query);
        }

        private int CheckSumHelper(string query)
        {
            var result = 0;
            //sqlconn.Open();
            var data = ExecuteCommand(query);
            
            using (data)
            {
                data.Read();
                if(!data.IsDBNull(0))
                    result = data. GetInt32(0);
            }
            //sqlconn.Close();
            return result;
        }

        private string CheckSumHelper2(string query)
        {
            var result = 0;
            //sqlconn.Open();
            var data = ExecuteCommand(query);

            var sha = SHA256.Create();
            var memStream = new MemoryStream();

            using (data)
            {
                while(data.Read())
                {
                    using (var dbStream = data.GetStream(0))
                    {
                        dbStream.CopyTo(memStream);
                    }
                }
            }

            var hash = sha.ComputeHash(memStream.ToArray());

            //sqlconn.Close();
            return System.Text.Encoding.UTF8.GetString(hash).Replace("'", "");
        }



        public void Dispose()
        {
            if(Sqlconn.State != System.Data.ConnectionState.Closed)
                Sqlconn.Close();
        }
    }
}
