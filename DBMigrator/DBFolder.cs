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

            var sqlScripts = System.IO.Directory.GetFiles(fullpath, "*.sql");

            foreach (var script in sqlScripts)
            {
                var match = Regex.Match(script, Script.MIGRATIONS_UPGRADE_FILENAME_REGEX);

                if (match.Success)
                {
                    Script rollbackScript = null;
                    var rollbackFile = $"{match.Groups[1]}_rollback_{match.Groups[2]}.sql";
                    var order = int.Parse(match.Groups[1].Value);
                    if (File.Exists(rollbackFile))
                    {
                        rollbackScript = new Script(new FileInfo(rollbackFile), order, Script.SQLTYPE.Rollback, feature, null);
                    }
                    result.Add(new Script(new FileInfo(script), order, Script.SQLTYPE.Upgrade, feature, rollbackScript));
                }
                else if (!Regex.IsMatch(script, Script.MIGRATIONS_ROLLBACK_FILENAME_REGEX))
                {
                    throw new Exception($"file {script} aren't a rollback script, and doesn't match the expected regex format: {Script.MIGRATIONS_UPGRADE_FILENAME_REGEX}");
                }
            }
            return result;
        }
    }
}
