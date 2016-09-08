using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using DBMigrator.Model;
using System.IO;
using System.Diagnostics;

namespace DBMigrator
{
    public class Database : IDisposable
    {
        private string executingPath;
        private SqlConnection sqlconn;
        private SqlTransaction trans;
        public List<DBVersion> allVersions;

        public Database(string servername, string database, string username, string password)
        {
            executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var connectionString = $"Data Source={servername};Initial Catalog={database};Persist Security Info=True;User ID={username};Password={password};MultipleActiveResultSets=True";
            sqlconn = new SqlConnection(connectionString);
            //allVersions = GetDBState();
        }

        public string CheckDatabaseVersion()
        {
            sqlconn.Open();
            var version = "0.0.0.0";
            try
            {
                var data = ExecuteCommand("SELECT TOP 1 Version FROM DBVersion order by Date desc");
                data.Read();
                version = data.GetString(0);
                Logger.GetInstance().Log($"Found existing database version {version}");
            }
            catch (Exception ex)
            {
                CreateDBVersionTable();
            }
            sqlconn.Close();
            return version;
        }

        private void CreateDBVersionTable()
        {
            Logger.GetInstance().Log("Creating DBVersion table");
            ExecuteCommand("CREATE TABLE DBVersion ([ID] [int] IDENTITY(1,1) NOT NULL, Version varchar(max) NOT NULL, Date datetime2 NOT NULL, Log xml NOT NULL, CONSTRAINT [PK_dbo.DBVersion] PRIMARY KEY CLUSTERED ([ID] ASC))");
            ExecuteCommand("CREATE TABLE DBVersionScripts (DBVersionID int NOT NULL, Feature varchar(max) NOT NULL, [Order] int NOT NULL, Script varchar(max) NOT NULL, Type varchar(max) NOT NULL, [Checksum] varchar(max) NOT NULL, ExecutionTime int NOT NULL)");
            ExecuteCommand("ALTER TABLE [DBVersionScripts] WITH CHECK ADD CONSTRAINT [FK.DBVersion.DBVersionScripts_DBVersionID] FOREIGN KEY([DBVersionID]) REFERENCES [DBVersion]([ID])");
        }

        public void UpdateDatabaseVersion(DBVersion version)
        {
            var versionStr = version.Version.ToString();
            Logger.GetInstance().Log($"Updating DBVersion version to {versionStr}");
            var data = ExecuteCommand($"INSERT INTO DBVersion (Version, Date, Log) OUTPUT Inserted.ID VALUES ('{versionStr}', GETUTCDATE(), '<xml>' + CHAR(13) + '{Logger.GetInstance().log.ToString()}</xml>')");
            using (data)
            {
                data.Read();
                version.ID = data.GetInt32(0);
            }
        }

        public void UpdateLog(DBVersion version)
        {
            ExecuteCommand($"UPDATE DBVersion SET Log = '<xml>' + CHAR(13) + '{Logger.GetInstance().log.ToString()}</xml>' WHERE ID = {version.ID}");
        }

        public void UpdateDataWithFile(Script script)
        {
            var sw = new Stopwatch();
            sw.Start();
            ExecuteCommand(script.SQL);
            sw.Stop();
            script.ExecutionTime = Convert.ToInt32(sw.ElapsedMilliseconds);
            ExecuteCommand($"INSERT INTO DBVersionScripts (DBVersionID, [Order], Feature, Script, Type, Checksum, ExecutionTime) VALUES ('{script.Feature.Version.ID}', {script.Order}, '{script.Feature.Name}', '{script.FileName}', '{script.Type.ToString()}', '{script.Checksum}', {script.ExecutionTime})");
        }

        public SqlDataReader ExecuteSingleCommand(string cmd)
        {
            sqlconn.Open();
            var result = ExecuteCommand(cmd);
            sqlconn.Close();
            return result;
        }

        private SqlDataReader ExecuteCommand(string cmd)
        {
            using (SqlCommand command = new SqlCommand(cmd, sqlconn, trans))
            {
                var result = command.ExecuteReader();
                return result;
            }
        }

        public void BeginTransaction()
        {
            sqlconn.Open();
            trans = sqlconn.BeginTransaction();
        }

        public void CommitTransaction()
        {
            trans.Commit();
            sqlconn.Close();
        }

        public void RollbackTransaction()
        {
            trans.Rollback();
            sqlconn.Close();
        }

        public void Close()
        {
            sqlconn.Close();
        }

        public List<DBVersion> GetDBState() {
            CheckDatabaseVersion();
            sqlconn.Open();
            var result = new List<DBVersion>();
            var data = ExecuteCommand("SELECT [Version], [Feature], [Order], [Script], [Type], [Checksum], [ExecutionTime] FROM [DBVersion] INNER JOIN [DbversionScripts] ON [DBVersion].ID = [DbversionScripts].DBVersionID");
            while (data.Read())
            {
                var version = data.GetString(0);
                var feature = data.GetString(1);
                var order = data.GetInt32(2);
                var script = data.GetString(3);
                var type = data.GetString(4);
                var checksum = data.GetString(5);
                var executiontime = data.GetInt32(6);

                var dbversion = result.FirstOrDefault(v => v.Name == version);
                if (dbversion == null)
                {
                    dbversion = new DBVersion(version);
                    result.Add(dbversion);
                }
                dbversion.AddOrUpdateFeature(feature, new Script(script, order, (Script.SQLTYPE)Enum.Parse(typeof(Script.SQLTYPE), type), null));
            }
            sqlconn.Close();
            return result;
        }

        public int GetTablesViewsAndColumnsChecksum()
        {
            var query = @"SELECT
                        CHECKSUM_AGG(CHECKSUM
                        (TABLE_SCHEMA
                        , DATA_TYPE
                        , TABLE_NAME
                        , COLUMN_NAME
                        , NUMERIC_PRECISION
                        , DATETIME_PRECISION
                        , CHARACTER_MAXIMUM_LENGTH)) as TablesViewsAndColumnsChecksum
                         FROM INFORMATION_SCHEMA.COLUMNS";
            
            return CheckSumHelper(query);
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
                         FROM[amphi].[INFORMATION_SCHEMA].[ROUTINES]
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
                         FROM[amphi].[INFORMATION_SCHEMA].[ROUTINES]
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

        private int CheckSumHelper(string query)
        {
            var result = 0;
            sqlconn.Open();
            var data = ExecuteCommand(query);
            
            using (data)
            {
                data.Read();
                result = data.GetInt32(0);
            }
            sqlconn.Close();
            return result;
        }



        public void Dispose()
        {
            sqlconn.Close();
        }
    }
}
