using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Model
{
    public class DataScript : Script
    {
        public DataScript(string fileName, int order, Feature feature) : base(fileName, order, feature)
        {
        }
    }
}
