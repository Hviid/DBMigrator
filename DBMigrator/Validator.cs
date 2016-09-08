using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class Validator
    {
        //validates that everything in target, are correct against source
        public void ValidateVersions(List<DBVersion> source, List<DBVersion> target)
        {
            foreach (var targetVersion in target)
            {
                var sourceVersion = source.SingleOrDefault(t => t.Name == targetVersion.Name);
                if (sourceVersion == null) throw new Exception($"Could not find target version ${targetVersion.Name} in source");

                foreach (var targetFeature in targetVersion.Features)
                {
                    var sourceFeature = sourceVersion.Features.SingleOrDefault(s => s.Name == targetFeature.Name);
                    if(sourceFeature == null) throw new Exception($"Could not find target feature ${targetFeature.Name} in source for version {targetVersion.Name}");

                    foreach (var targetScript in targetFeature.UpgradeScripts)
                    {
                        var sourceScript = sourceFeature.UpgradeScripts.SingleOrDefault(s => s.Name == targetScript.Name);
                        if (sourceScript == null) throw new Exception($"Could not find target script {targetScript.Name} in target feature ${targetFeature.Name} in source for version {targetVersion.Name}");
                        if (sourceScript.Order != targetScript.Order) throw new Exception($"Target script {targetScript.Name} order {targetScript.Order} are not equal to source script {sourceScript.Name} order {sourceScript.Order}");
                        if(sourceScript.Checksum != targetScript.Checksum) throw new Exception($"Target script {targetScript.Name} checksum {targetScript.Checksum} are not equal to source script {sourceScript.Name} checksum {sourceScript.Checksum}");
                    }
                }
            }
        }

        public void ValidateDatabase()
        {

        }

    }
}
