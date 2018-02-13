using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using DBMigrator.Model;

namespace DBMigrator.Middleware
{
    public class PrePostMigrationScripts : IMiddleware
    {
        private DirectoryInfo _migrationsDirectory;

        public PrePostMigrationScripts(DirectoryInfo migrationsDirectory)
        {
            _migrationsDirectory = migrationsDirectory;
        }

        public void Init(Middleware middleware)
        {
            var result = FindPreAndPostScripts();
            //TODO should not override
            middleware.PreVersionsUpgradeScripts = result.preVersionsUpgradeScripts;
            middleware.PostVersionsUpgradeScripts = result.postVersionsUpgradeScripts;
        }

        private (List<Script> preVersionsUpgradeScripts, List<Script> postVersionsUpgradeScripts) FindPreAndPostScripts()
        {
            var preVersionsUpgradeScripts = new List<Script>();
            var postVersionsUpgradeScripts = new List<Script>();

            var preVersionsUpgrade = _migrationsDirectory.GetFiles("pre.sql");
            if(preVersionsUpgrade.Length == 1)
            {
                var newScript = new Script(preVersionsUpgrade[0].Name)
                {
                    SQL = GetFileContent(preVersionsUpgrade[0].FullName)
                };
                preVersionsUpgradeScripts.Add(newScript);
            }

            var postVersionsUpgrade = _migrationsDirectory.GetFiles("post.sql");
            if (postVersionsUpgrade.Length == 1)
            {
                var newScript = new Script(postVersionsUpgrade[0].Name)
                {
                    SQL = GetFileContent(postVersionsUpgrade[0].FullName)
                };
                postVersionsUpgradeScripts.Add(newScript);
            }

            return (preVersionsUpgradeScripts, postVersionsUpgradeScripts);
        }

        private string GetFileContent(string filePath)
        {
            return System.IO.File.ReadAllText(filePath);
        }
    }
}
