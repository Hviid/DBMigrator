using DBMigrator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test
{
    [TestClass]
    public class ValidatorTest
    {
        private List<DBVersion> Baseline()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            var script = feature.AddScript("TestScript.sql", 1, Script.SQLTYPE.Upgrade);
            script.Checksum = "A";

            return new List<DBVersion> { version };
        }

        private List<DBVersion> Same()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            var script = feature.AddScript("TestScript.sql", 1, Script.SQLTYPE.Upgrade);
            script.Checksum = "A";

            return new List<DBVersion> { version };
        }

        private List<DBVersion> DiffChecksum()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            var script = feature.AddScript("TestScript.sql", 1, Script.SQLTYPE.Upgrade);
            script.Checksum = "B";

            return new List<DBVersion> { version };
        }

        private List<DBVersion> DiffVersion()
        {
            var version = new DBVersion("1.0.1");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            var script = feature.AddScript("TestScript.sql", 1, Script.SQLTYPE.Upgrade);
            script.Checksum = "A";

            return new List<DBVersion> { version };
        }

        [TestMethod]
        public void Test_nodiff()
        {
            var validator = new Validator();

            validator.ValidateVersions(Baseline(), Same());
        }

        [TestMethod]
        public void Test_Diff_checksum()
        {
            var validator = new Validator();

            Assert.ThrowsException<Exception>(() => validator.ValidateVersions(Baseline(), DiffChecksum()));
        }

        [TestMethod]
        public void Test_Diff_version()
        {
            var validator = new Validator();

            Assert.ThrowsException<Exception>(() => validator.ValidateVersions(Baseline(), DiffVersion()));
        }

        [TestMethod]
        public void Test_Diff_feature()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Diff_script()
        {
            throw new NotImplementedException();
        }
    }
}
