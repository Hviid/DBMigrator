﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DBMigrator
{
    public class Bootstrapper
    {
        private static IServiceCollection _serviceCollection;

        static public IServiceProvider GetConfiguredServiceProvider()
        {
            if (_serviceCollection == null) throw new Exception($"{nameof(ConfigureServices)} hasn't been called");
            return _serviceCollection.BuildServiceProvider();
        }

        static public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(builder => builder.AddConsole().AddDebug());

            _serviceCollection = serviceCollection;
        }


    }
}
