using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test
{
    [TestClass]
    public class BootstrapTests
    {
        [AssemblyInitialize]
        public static void Bootstrap(TestContext context)
        {
            Bootstrapper.ConfigureServices(new ServiceCollection());
        }
    }
}
