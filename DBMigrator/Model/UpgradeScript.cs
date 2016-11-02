using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Model
{
    public class UpgradeScript : Script
    {
        public UpgradeScript(string fileName, int order, Feature feature) : base(fileName, order, feature)
        {
        }
    }
}
