using System;
using System.Collections.Generic;

namespace DBMigrator
{
    public interface IDatabase : IDisposable
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void Close();

        void ExecuteSingleCommand(string sql);
        void ExecuteUpgradeCommand(string versionName, string featureName, string scriptFileName, string sql);
    }
}
