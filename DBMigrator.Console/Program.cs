using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.CommandLineUtils;
using DBMigrator.Model;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace DBMigrator.Console
{
    public class Program
    {
        private static ILogger _logger;

        public static void Main(string[] args)
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
            var commandArg = commandLineApplication.Argument("command <upgrade|downgrade|validatedatabase>", "Command to execute");

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

            IServiceCollection serviceCollection = new ServiceCollection();
            Bootstrapper.ConfigureServices(serviceCollection);

            var test = serviceCollection.BuildServiceProvider();
            var loggerFactory = test.GetRequiredService<ILoggerFactory>();
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
                var database = new Database(servername, databasename, username, password);
                
                switch (commandArg.Value)
                {
                    case "upgrade":
                        if (!noValidationArg.HasValue())
                        {
                            ValidateDatabase(database, migrationDirectory, noPromptArg.HasValue());
                        }
                        Upgrade(versionArg.Value(), database, migrationDirectory, noPromptArg.HasValue());
                        break;
                    case "downgrade":
                        if (!noValidationArg.HasValue())
                        {
                            ValidateDatabase(database, migrationDirectory, noPromptArg.HasValue());
                        }
                        Rollback(versionArg.Value(), database, migrationDirectory, noPromptArg.HasValue());
                        break;
                    case "validatedatabase":
                        ValidateDatabase(database, migrationDirectory, noPromptArg.HasValue());
                        break;
                    default:
                        break;
                }
                if (!noPromptArg.HasValue())
                {
                    _logger.LogInformation("Press any key to exit.");
                    System.Console.ReadKey();
                }
                return 0;
            });
            commandLineApplication.Execute(args);
        }

        public static DirectoryInfo GetExecutingDir()
        {
            return new DirectoryInfo(Path.GetDirectoryName(typeof(VersionValidator).GetTypeInfo().Assembly.Location));
            //new DirectoryInfo(Path.GetDirectoryName(AppContext.BaseDirectory));
            //new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        private static async void Upgrade(string toVersion, Database database, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            var dbfolder = new DBFolder(migrationsDir);
            _logger.LogDebug($"Reading from {migrationsDir.FullName}");
            var dbVersions = database.GetDBState();
            var differ = new VersionDiff();
            _logger.LogInformation("Calculating diff");
            var diff = differ.Diff(dbfolder.GetVersions(toVersion), dbVersions);
            if(diff.Count > 0)
            {
                var diffText = differ.DiffText(diff);
                _logger.LogInformation(diffText);
                if (!noPrompt)
                {
                    _logger.LogInformation("Apply updates? Press any key to continue upgrade.");
                    System.Console.ReadKey();
                }
                var migrator = new Migrator(database, dbfolder);
                var i = 0;

                void callback(object args){
                    System.Console.Write("\r{0} secs", i++);
                }

                var timer = new Timer(callback, null, 0, 1000);
                await Task.Run(() => migrator.Upgrade(diff));
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
            
        }

        private static void Rollback(string toVersion, Database database, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            var dbVersions1 = database.GetDBState();
            var dbfolder1 = new DBFolder(migrationsDir);
            var differ1 = new VersionDiff();
            var diff1 = differ1.Diff(dbVersions1, dbfolder1.GetVersions(toVersion));
            if (diff1.Count > 0)
            {
                dbfolder1.AddRollbacks(diff1);
                var diffText1 = differ1.DiffText(diff1);
                _logger.LogInformation(diffText1);
                if (!noPrompt)
                {
                    _logger.LogInformation("Press any key to continue downgrade.");
                    System.Console.ReadKey();
                }

                var migrator = new Migrator(database, dbfolder1);
                migrator.Rollback(diff1);
            }
            else
            {
                _logger.LogInformation("Database already downgraded.");
            }
        }

        private static void ValidateDatabase(Database database, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
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
    }
}
