using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class VersionDiff
    {
        private string executingPath;
        public VersionDiff()
        {
            executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        public string DiffText(List<DBVersion> diff)
        {
            var str = "";
            foreach (var version in diff)
            {
                str += $"version: {version.Name} \n";
                foreach (var feature in version.Features)
                {
                    str += $"--feature: {feature.Name} \n";
                    foreach (var script in feature.UpgradeScripts)
                    {
                        str += $"----script: {script.Name} \n";
                    }
                }
            }
            return str;
        }

        public List<DBVersion> Diff(List<DBVersion> source, List<DBVersion> target)
        {
            var diff = new List<DBVersion>();
            foreach (var sourceVersion in source)
            {
                var targetVersion = target.SingleOrDefault(t => t.Name == sourceVersion.Name);
                if (targetVersion == null) {
                    diff.Add(sourceVersion);
                    continue;
                } 

                foreach (var sourceFeature in sourceVersion.Features)
                {
                    var targetFeature = targetVersion.Features.SingleOrDefault(s => s.Name == sourceFeature.Name);
                    if (targetFeature == null)
                    {
                        var diffVersion = diff.SingleOrDefault(t => t.Name == targetVersion.Name);
                        if(diffVersion == null)
                        {
                            diffVersion = new DBVersion(targetVersion.Directory);
                        }
                        diffVersion.Features.Add(sourceFeature);
                        diff.Add(diffVersion);
                        continue;
                    }

                    foreach (var sourceScript in sourceFeature.UpgradeScripts)
                    {
                        var targetScript = targetFeature.UpgradeScripts.SingleOrDefault(s => s.Name == sourceScript.Name);
                        if (targetScript == null)
                        {
                            var diffVersion = diff.SingleOrDefault(t => t.Name == targetVersion.Name);
                            if (diffVersion == null)
                            {
                                diffVersion = new DBVersion(targetVersion.Directory);
                            }
                            diff.Add(diffVersion);
                            var diffFeature = diffVersion.Features.SingleOrDefault(t => t.Name == targetFeature.Name);
                            if (diffFeature == null)
                            {
                                diffFeature = new Feature(targetFeature.Directory, diffVersion);
                            }
                            diffVersion.Features.Add(diffFeature);
                            var diffScript = diffFeature.UpgradeScripts.SingleOrDefault(t => t.Name == targetScript.Name);
                            if (diffScript == null)
                            {
                                diffScript = new Script(sourceScript.File, sourceScript.Order, sourceScript.Type, diffFeature, sourceScript.RollbackScript);
                            }
                            diffFeature.AddScript(diffScript);
                        }
                    }
                }
            }
            return diff;
        }

    }
}
