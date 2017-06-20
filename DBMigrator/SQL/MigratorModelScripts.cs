using System;
using System.Collections.Generic;
using System.Text;

namespace DBMigrator.SQL
{
    class MigratorModelScripts
    {
        public string CreateDBVersionScripts
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
                            [DatabaseTriggersChecksum] varchar(max) NOT NULL, 
                            [DatabaseTablesAndViewsChecksum] varchar(max) NOT NULL, 
                            [DatabaseFunctionsChecksum] varchar(max) NOT NULL, 
                            [DatabaseStoredProceduresChecksum] varchar(max) NOT NULL, 
                            [DatabaseIndexesChecksum] varchar(max) NOT NULL, 
                            ExecutionTime int NOT NULL)";
        }

        public string SelectDBVersionScriptsScript
        {
            get => "SELECT [Version], [Feature], [Order], [Script], [Type], [ScriptFileChecksum], [ExecutionTime] FROM [DBVersionScripts]";
        }

        public string GetInsertDBVersionScript(string version, string order, string featurename, string filename, string checksum,
            string databaseTriggersChecksum, string databaseTablesAndViewsChecksum, string databaseFunctionsChecksum, string databaseStoredProceduresChecksum,
            string databaseIndexesChecksum, int scriptExecutionTime)
        {
            return $@"INSERT INTO DBVersionScripts (
                                                [Version], 
                                                [Date], 
                                                [Order], 
                                                Feature, 
                                                Script, 
                                                Type, 
                                                ScriptFileChecksum, 
                                                DatabaseTriggersChecksum, 
                                                DatabaseTablesAndViewsChecksum, 
                                                DatabaseFunctionsChecksum, 
                                                DatabaseStoredProceduresChecksum, 
                                                DatabaseIndexesChecksum, 
                                                ExecutionTime) VALUES (
                                                '{version}',
                                                GETUTCDATE(), 
                                                {order}, 
                                                '{featurename}', 
                                                '{filename}', 
                                                'Upgrade', 
                                                '{checksum}', 
                                                '{databaseTriggersChecksum}', 
                                                '{databaseTablesAndViewsChecksum}', 
                                                '{databaseFunctionsChecksum}', 
                                                '{databaseStoredProceduresChecksum}', 
                                                '{databaseIndexesChecksum}', 
                                                {scriptExecutionTime})";
        }

        public string GetDeleteDBVersionScript(string rollbackFileName)
        {
            return $"DELETE FROM DBVersionScripts WHERE Script = '{rollbackFileName}'";
        }
    }
}
