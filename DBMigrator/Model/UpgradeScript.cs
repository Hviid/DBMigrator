using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Model
{
    public class UpgradeScript : Script
    {
        public string Checksum { get; set; }
        public int ExecutionTime { get; set; }

        public UpgradeScript(string fileName, int order, Feature feature) : base(fileName, order, feature)
        {
        }

        public void AddFileInfo(string sql, string checksum)
        {
            SQL = sql;
            Checksum = checksum;
        }
    }
}
