using DBMigrator.SQL;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBMigrator
{
    public class TextFileDB : IDatabase
    {
        StreamWriter migrationsStream;
        StreamWriter migrationsTestStream;

        public TextFileDB(DirectoryInfo migrationsDir)
        {
            var migrationsFile = File.Create(Path.Combine(migrationsDir.FullName, "migrations.sql"));
            migrationsStream = new System.IO.StreamWriter(migrationsFile);
            var migrationsTestFile = File.Create(Path.Combine(migrationsDir.FullName, "migrations_test.sql"));
            migrationsTestStream = new System.IO.StreamWriter(migrationsTestFile);
        }

        public void BeginTransaction()
        {
        }

        public void Close()
        {
        }

        public void CommitTransaction()
        {
        }

        public void Dispose()
        {
            migrationsStream.Dispose();
        }
        public void ExecuteUpgradeCommand(string versionName, string featureName, string scriptFileName, string sql)
        {
            WriteToMigrationsTestFile(versionName, featureName, scriptFileName, sql);
            WriteToMigrationsFile(versionName, featureName, scriptFileName, sql);
        }

        private void WriteToMigrationsTestFile(string versionName, string featureName, string scriptFileName, string sql)
        {
            var cmd = $"IF NOT EXISTS ({MigratorModelScripts.QueryScript(versionName, featureName, scriptFileName)}) {Environment.NewLine}" +
                $"BEGIN {Environment.NewLine}" +
                $"PRINT 'APPLIED {versionName} {featureName} {scriptFileName}' {Environment.NewLine}" +
                $"END {Environment.NewLine}" +
                $"ELSE {Environment.NewLine}" +
                $"BEGIN {Environment.NewLine}" +
                $"PRINT '{versionName} {featureName} {scriptFileName} ALREADY APPLIED' {Environment.NewLine}" +
                $"END {Environment.NewLine}";
            
            migrationsTestStream.WriteLine(cmd);
            migrationsTestStream.Flush();
        }

        private void WriteToMigrationsFile(string versionName, string featureName, string scriptFileName, string sql)
        {
            var cmd = $"IF NOT EXISTS ({MigratorModelScripts.QueryScript(versionName, featureName, scriptFileName)}) {Environment.NewLine}" +
                $"BEGIN {Environment.NewLine}" +
                sql + $" {Environment.NewLine}" +
                $"PRINT 'APPLIED {versionName} {featureName} {scriptFileName}' {Environment.NewLine}" +
                $"END {Environment.NewLine}" +
                $"ELSE {Environment.NewLine}" +
                $"BEGIN {Environment.NewLine}" +
                $"PRINT '{versionName} {featureName} {scriptFileName} ALREADY APPLIED' {Environment.NewLine}" +
                $"END {Environment.NewLine}";
            
            migrationsStream.WriteLine(cmd);
            migrationsStream.Flush();
        }

        public void ExecuteSingleCommand(string sql)
        {
            migrationsStream.WriteLine(sql);
        }

        public void RollbackTransaction()
        {
        }
    }
}
