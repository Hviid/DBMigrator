using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class Util
    {
        public static void CreateNewFeatureFile(string path)
        {
            File.WriteAllText(Path.Combine(path, "NewFeature.ps1"), @"
            Param(
	            Parameter(Mandatory=True)
	            string newfeaturename
            ) 

            mkdir newfeaturename
            cd newfeaturename
            mkdir ""Upgrade""
            cd ""Upgrade""
            mkdir ""PreMigrate""
            mkdir ""Migrate""
            mkdir ""PostMigrate""

            mkdir ""StoredProcedures""
            mkdir ""Functions""
            mkdir ""Views""
            mkdir ""Triggers");
        }

    }
}
