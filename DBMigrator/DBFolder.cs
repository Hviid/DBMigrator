using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class DBFolder
    {
        private string executingPath;
        public List<DBVersion> allVersions;

        public DBFolder()
        {
            executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            allVersions = GetFolderState();
        }

        public List<DBVersion> GetVersionsUpTo(string version)
        {
            var upToVersion = new Version(version);
            return allVersions.Where(v => v.Version <= upToVersion).ToList();
        }

        private List<DBVersion> GetFolderState()
        {
            var allVersions = Directory.GetDirectories(executingPath).Select(d => new DBVersion((new DirectoryInfo(d)))).ToList();

            foreach (var version in allVersions)
            {
                version.Features = version.Directory.GetDirectories().Select(d => new Feature(new DirectoryInfo(d.FullName), version)).ToList();
                foreach (var feature in version.Features)
                {
                    feature.UpgradeScripts = FindMigrationsForFeature(feature);
                }
            }

            return allVersions;
        }

        private List<Script> FindMigrationsForFeature(Feature feature)
        {
            var result = new List<Script>();
            var fullpath = Path.Combine(feature.Directory.FullName, "Migrations");

            var sqlScriptsNames = System.IO.Directory.GetFiles(fullpath, "*.sql");

            foreach (var scriptName in sqlScriptsNames)
            {
                var match = Regex.Match(scriptName, Script.MIGRATIONS_UPGRADE_FILENAME_REGEX);

                if (match.Success)
                {
                    var order = int.Parse(match.Groups[1].Value);
                    
                    result.Add(new Script(new FileInfo(scriptName), order, Script.SQLTYPE.Upgrade, feature));
                }
                else if (!Regex.IsMatch(scriptName, Script.MIGRATIONS_ROLLBACK_FILENAME_REGEX))
                {
                    throw new Exception($"file {scriptName} aren't a rollback script, and doesn't match the expected regex format: {Script.MIGRATIONS_UPGRADE_FILENAME_REGEX}");
                }
            }
            return result;
        }
    }
}
