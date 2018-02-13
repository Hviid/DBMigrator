using DBMigrator.Model;
using System.Collections.Generic;

namespace DBMigrator.Middleware
{
    public class Middleware
    {
        public List<Script> PreVersionsUpgradeScripts { get; internal set; }
        public List<Script> PostVersionsUpgradeScripts { get; internal set; }

        public void RegisterMiddleware(IMiddleware middleware)
        {
            middleware.Init(this);
        }
    }
}