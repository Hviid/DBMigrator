using DBMigrator.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DBMigrator.Test.Comparers
{
    public class DBVersionComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xx = (DBVersion)x;
            var yy = (DBVersion)y;

            var result = xx.Version.CompareTo(yy.Version);

            result += (xx.Features.SequenceEqual(yy.Features, new FeatureComparer()) ? 0 : 1);

            return result;
        }
    }
}
