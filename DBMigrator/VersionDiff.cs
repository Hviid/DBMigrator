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
                        str += $"----script: {script.FileName} \n";
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
                            diffVersion = new DBVersion(targetVersion.Name);
                            diff.Add(diffVersion);
                        }
                        CopyFeatureToVersion(sourceFeature, diffVersion);
                        continue;
                    }

                    foreach (var sourceScript in sourceFeature.UpgradeScripts)
                    {
                        var targetScript = targetFeature.UpgradeScripts.SingleOrDefault(s => s.FileName == sourceScript.FileName);
                        if (targetScript == null)
                        {
                            var diffVersion = diff.SingleOrDefault(t => t.Name == targetVersion.Name);
                            if (diffVersion == null)
                            {
                                diffVersion = new DBVersion(targetVersion.Name);
                                diff.Add(diffVersion);
                            }
                            
                            var diffFeature = diffVersion.Features.SingleOrDefault(t => t.Name == targetFeature.Name);
                            if (diffFeature == null)
                            {
                                diffVersion.AddFeature(targetFeature.Name);
                            }
                            CopyScriptToFeature(sourceScript, diffFeature);
                        }
                    }
                }
            }
            return diff;
        }

        private void CopyFeatureToVersion(Feature sourceFeature, DBVersion target)
        {
            var feature = target.AddFeature(sourceFeature.Name);
            foreach (var script in sourceFeature.UpgradeScripts)
            {
                CopyScriptToFeature(script, feature);
            }
            
        }

        private void CopyScriptToFeature(Script sourceScript, Feature target)
        {
            var script = new Script(sourceScript.FileName, sourceScript.Order, sourceScript.Type, target);
            target.AddScript(script);
        }

    }
}
