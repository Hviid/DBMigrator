﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.CommandLineUtils;
using DBMigrator.Model;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading;
using System;
using DBMigrator.Middleware;
#nullable enable

namespace DBMigrator.Console
{
    public class Program
    {
        private static ILogger _logger;

        public static int Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            commandLineApplication.HelpOption("-? | -h | --help");
            //CommandArgument names = null;
            //var test2 = commandLineApplication.Command("name",
            //    (target) =>
            //    names = target.Argument(
            //        "fullname",
            //        "Enter the full name of the person to be greeted.",
            //        multipleValues: true));
            //test2.HelpOption("-? | -h | --help");
            var commandArg = commandLineApplication.Argument("command <upgrade|downgrade|validatedatabase|upgrade_file>", "Command to execute");

            CommandOption versionArg = commandLineApplication.Option(
                "-v |--version <version>",
                "The target version of the migration. If left blank targets latest version",
                CommandOptionType.SingleValue);

            CommandOption serveraddressArg = commandLineApplication.Option(
                "-s |--serveraddress <serveraddress>",
                "The address of the database server instance",
                CommandOptionType.SingleValue);

            CommandOption databasenameArg = commandLineApplication.Option(
                "-d |--databasename <databasename>",
                "The databasename of the database to migrate",
                CommandOptionType.SingleValue);

            CommandOption usernameArg = commandLineApplication.Option(
                "-u |--username <username>",
                "The username of user to auth against the database",
                CommandOptionType.SingleValue);

            CommandOption passwordArg = commandLineApplication.Option(
                "-p |--password <password>",
                "The password for the to auth against the database",
                CommandOptionType.SingleValue);

            CommandOption migrationsPathArg = commandLineApplication.Option(
                "-f |--folderPath <path>",
                "The path for the folder where migrations are located",
                CommandOptionType.SingleValue);

            CommandOption noPromptArg = commandLineApplication.Option(
                "--noprompt",
                "Runs command without required user interaction",
                CommandOptionType.NoValue);

            CommandOption noValidationArg = commandLineApplication.Option(
                "--novalidation",
                "Runs command without Database validation first",
                CommandOptionType.NoValue);

            CommandOption dryRynArg = commandLineApplication.Option(
                "--dry-run",
                "Runs command as a dry run without committing to the database",
                CommandOptionType.NoValue);

            CommandOption optionsArg = commandLineApplication.Option(
                "--options <options>",
                "Extra options appended to the connection string, ie --options Encrypt=False;TrustServerCertificate=False;",
                CommandOptionType.SingleValue);

            IServiceCollection serviceCollection = new ServiceCollection();
            Bootstrapper.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<Program>();

            DirectoryInfo migrationDirectory;

            commandLineApplication.OnExecute(() =>
            {
                if (migrationsPathArg.HasValue())
                {
                    migrationDirectory = new DirectoryInfo(migrationsPathArg.Value());
                }
                else
                {
                    migrationDirectory = GetExecutingDir();
                }

                var servername = serveraddressArg.Value();
                var username = usernameArg.Value();
                var password = passwordArg.Value();
                var databasename = databasenameArg.Value();
                var options = optionsArg.Value();
                var database = new Database(servername, databasename, username, password, options);

                try
                {
                    switch (commandArg.Value)
                    {
                        case "upgrade":
                            if (!noValidationArg.HasValue())
                            {
                                ValidateDatabase(database, migrationDirectory, noPromptArg.HasValue());
                            }
                            Upgrade(versionArg.Value(), database, migrationDirectory, noPromptArg.HasValue(), dryRynArg.HasValue());
                            break;
                        case "downgrade":
                            if (!noValidationArg.HasValue())
                            {
                                ValidateDatabase(database, migrationDirectory, noPromptArg.HasValue());
                            }
                            Rollback(versionArg.Value(), database, migrationDirectory, noPromptArg.HasValue(), dryRynArg.HasValue());
                            break;
                        case "validatedatabase":
                            ValidateDatabase(database, migrationDirectory, noPromptArg.HasValue());
                            break;
                        case "fixchecksums":
                            FixChecksums(database, migrationDirectory);
                            break;
                        case "upgrade_file":
                            UpgradeFile(migrationDirectory, noPromptArg.HasValue());
                            break;
                        default:
                            _logger.LogInformation("No command type specified");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    return 1;
                }

                if (!noPromptArg.HasValue())
                {
                    _logger.LogInformation("Press any key to exit.");
                    System.Console.ReadKey();
                }

                return 0;
            });
            var temp = commandLineApplication.Execute(args);
            return temp;
        }

        public static DirectoryInfo GetExecutingDir()
        {
            return new DirectoryInfo(Path.GetDirectoryName(typeof(VersionValidator).GetTypeInfo().Assembly.Location));
            //new DirectoryInfo(Path.GetDirectoryName(AppContext.BaseDirectory));
            //new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        private static void UpgradeFile(DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            _logger.LogInformation("Starting file upgrade");
            var dbfolder = new DBFolder(migrationsDir);
            _logger.LogInformation($"Reading from {migrationsDir.FullName}");
            var middleware = new Middleware.Middleware();
            middleware.RegisterMiddleware(new PrePostMigrationScripts(migrationsDir));
            
            var migrator = new Migrator(new TextFileDB(migrationsDir), dbfolder, middleware);
            var i = 0;

            void callback(object args)
            {
                System.Console.Write("\r{0} secs", i++);
            }

            var timer = new Timer(callback, null, 0, 1000);
            migrator.Upgrade(dbfolder.allVersions);
            timer.Dispose();

            _logger.LogInformation("Upgrade finished");
        }

        private static void Upgrade(string toVersion, Database database, DirectoryInfo migrationsDir, bool noPrompt = false, bool dryRun = false)
        {
            _logger.LogInformation("Starting upgrade");
            var dbfolder = new DBFolder(migrationsDir);
            _logger.LogInformation($"Reading from {migrationsDir.FullName}");
            var dbVersions = database.GetDBState();
            var differ = new VersionDiff();
            var middleware = new Middleware.Middleware();
            middleware.RegisterMiddleware(new PrePostMigrationScripts(migrationsDir));
            _logger.LogInformation("Calculating diff");
            var diff = differ.Diff(dbfolder.GetVersions(toVersion), dbVersions).ToList();
            if(diff.Count > 0)
            {
                var diffText = differ.UpgradeDiffText(diff);
                _logger.LogInformation(diffText);
                if (!noPrompt)
                {
                    _logger.LogInformation("Apply updates? Press any key to continue upgrade.");
                    System.Console.ReadKey();
                }
                var migrator = new Migrator(database, dbfolder, middleware);
                var i = 0;

                void callback(object args){
                    System.Console.Write("\r{0} secs", i++);
                }

                var timer = new Timer(callback, null, 0, 1000);
                migrator.Upgrade(diff, dryRun);
                timer.Dispose();

                if (!noPrompt)
                {
                    _logger.LogInformation("Upgrade completed.");
                }
            }
            else
            {
                _logger.LogInformation("Database is up to date.");
            }
            _logger.LogInformation("Upgrade finished");
        }

        private static void Rollback(string toVersion, Database database, DirectoryInfo migrationsDir, bool noPrompt = false, bool dryRun = false)
        {
            _logger.LogInformation("Starting downgrade");
            var dbVersions1 = database.GetDBState();
            var dbfolder1 = new DBFolder(migrationsDir);
            var differ1 = new VersionDiff();
            var middleware = new Middleware.Middleware();
            middleware.RegisterMiddleware(new PrePostMigrationScripts(migrationsDir));
            var diff1 = differ1.Diff(dbVersions1, dbfolder1.GetVersions(toVersion)).ToList();
            if (diff1.Count > 0)
            {
                dbfolder1.AddRollbacks(diff1);
                differ1.LogDownGradeDiffText(diff1, _logger);
                if (!noPrompt)
                {
                    _logger.LogInformation("Press any key to continue downgrade.");
                    System.Console.ReadKey();
                }

                var migrator = new Migrator(database, dbfolder1, middleware);
                migrator.Rollback(diff1, dryRun);
            }
            else
            {
                _logger.LogInformation("Database already downgraded.");
            }
            _logger.LogInformation("Downgrade finished");
        }

        private static void ValidateDatabase(Database database, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            _logger.LogInformation("Validating database");
            var dbVersions2 = database.GetDBState();
            var dbfolder2 = new DBFolder(migrationsDir);
            var validator2 = new VersionValidator();
            var DBValidator = new DatabaseValidator(database);
            _logger.LogInformation("Validating DB script state");
            validator2.ValidateVersions(dbfolder2.allVersions, dbVersions2);
            _logger.LogInformation("DB script state validation passed");
            _logger.LogInformation("Validating DB integrity");
            DBValidator.Validate();
            _logger.LogInformation("DB integrity validation passed");
        }
        private static void FixChecksums(Database database, DirectoryInfo migrationsDir)
        {
            _logger.LogInformation("Fix checksums");
            var target = database.GetDBState();
            var source = new DBFolder(migrationsDir).allVersions;
            foreach (var targetVersion in target)
            {
                var sourceVersion = source.SingleOrDefault(t => t.Name == targetVersion.Name);
                if (sourceVersion == null) throw new Exception($"Could not find target version {targetVersion.Name} in source");

                foreach (var targetFeature in targetVersion.Features)
                {
                    var sourceFeature = sourceVersion.Features.SingleOrDefault(s => s.Name == targetFeature.Name);
                    if (sourceFeature == null) throw new Exception($"Could not find target feature {targetFeature.Name} in source for version {targetVersion.Name}");

                    foreach (var targetScript in targetFeature.UpgradeScripts)
                    {
                        var sourceScript = sourceFeature.UpgradeScripts.SingleOrDefault(s => s.FileName == targetScript.FileName);
                        if (sourceScript == null) throw new Exception($"Could not find target script {targetScript.FileName} in target feature {targetFeature.Name} in source for version {targetVersion.Name}");
                        if (sourceScript.Order != targetScript.Order) throw new Exception($"Target script {targetScript.FileName} order {targetScript.Order} are not equal to source script {sourceScript.FileName} order {sourceScript.Order}");
                        if (sourceScript.Checksum != targetScript.Checksum)
                        {
                            database.FixChecksum(sourceScript);
                        }
                    }
                }
            }
        }
    }
}
