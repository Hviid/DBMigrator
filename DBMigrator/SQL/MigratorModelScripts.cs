using System;
using System.Collections.Generic;
using System.Text;

namespace DBMigrator.SQL
{
    static class MigratorModelScripts
    {
        public static string CreateDBVersionScriptsTable
        {
            get => @"CREATE TABLE DBVersionScripts (
                            [ID] [int] IDENTITY(1,1) NOT NULL, 
                            Date datetime2 NOT NULL, 
                            Version varchar(max) NOT NULL, 
                            Feature varchar(max) NOT NULL, 
                            [Order] int NOT NULL, 
                            Script varchar(max) NOT NULL, 
                            Type varchar(max) NOT NULL, 
                            [ScriptFileChecksum] varchar(max) NOT NULL, 
                            [DatabaseTriggersChecksum] varbinary(max) NOT NULL, 
                            [DatabaseTablesAndViewsChecksum] varbinary(max) NOT NULL, 
                            [DatabaseFunctionsChecksum] varbinary(max) NOT NULL, 
                            [DatabaseStoredProceduresChecksum] varbinary(max) NOT NULL, 
                            [DatabaseIndexesChecksum] varbinary(max) NOT NULL, 
                            ExecutionTime int NOT NULL)";
        }
        /// <summary>
        /// returns [Version], [Feature], [Order], [Script], [Type], [ScriptFileChecksum]
        /// [ExecutionTime], Date, [DatabaseTriggersChecksum], [DatabaseTablesAndViewsChecksum]
        /// [DatabaseFunctionsChecksum], [DatabaseStoredProceduresChecksum], [DatabaseIndexesChecksum]
        /// </summary>
        public static string SelectDBVersionScriptsScript
        {
            get => "SELECT [Version], [Feature], [Order], [Script], [Type], [ScriptFileChecksum], " +
                "[ExecutionTime], Date, [DatabaseTriggersChecksum], [DatabaseTablesAndViewsChecksum]," +
                "[DatabaseFunctionsChecksum], [DatabaseStoredProceduresChecksum], [DatabaseIndexesChecksum] FROM [DBVersionScripts]";
        }

        public static string GetInsertDBVersionScript(string version, int order, string featurename, string filename, string checksum,
            string databaseTriggersChecksum, string databaseTablesAndViewsChecksum, string databaseFunctionsChecksum, string databaseStoredProceduresChecksum,
            string databaseIndexesChecksum, int scriptExecutionTime)
        {
            return $"INSERT INTO DBVersionScripts (" +
                    $"[Version], " +
                    $"[Date], " +
                    $"[Order], " +
                    $"Feature, " +
                    $"Script, " +
                    $"Type, " +
                    $"ScriptFileChecksum, " +
                    $"DatabaseTriggersChecksum, " +
                    $"DatabaseTablesAndViewsChecksum, " +
                    $"DatabaseFunctionsChecksum, " +
                    $"DatabaseStoredProceduresChecksum, " +
                    $"DatabaseIndexesChecksum, " +
                    $"ExecutionTime) VALUES (" +
                    $"'{version}'," +
                    $"GETUTCDATE(), " +
                    $"{order}, " +
                    $"'{featurename}', " +
                    $"'{filename}', " +
                    $"'Upgrade', " +
                    $"'{checksum}', " +
                    $"{databaseTriggersChecksum}, " +
                    $"{databaseTablesAndViewsChecksum}, " +
                    $"{databaseFunctionsChecksum}, " +
                    $"{databaseStoredProceduresChecksum}, " +
                    $"{databaseIndexesChecksum}, " +
                    $"{scriptExecutionTime})";
        }

        public static string GetDeleteDBVersionScript(string rollbackFileName)
        {
            return $"DELETE FROM DBVersionScripts WHERE Script = '{rollbackFileName}'";
        }
    }
}
