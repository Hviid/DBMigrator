using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.CommandLineUtils;
using DBMigrator.Model;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            var commandArg = commandLineApplication.Argument("command <upgrade|downgrade|validatedatabase|validateupgrade>", "Command to execute");

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

                switch (commandArg.Value)
                {
                    case "upgrade":
                        Upgrade(versionArg.Value(), serveraddressArg.Value(), databasenameArg.Value(), usernameArg.Value(), passwordArg.Value(), migrationDirectory, noPromptArg.HasValue());
                        break;
                    case "downgrade":
                        Rollback(versionArg.Value(), serveraddressArg.Value(), databasenameArg.Value(), usernameArg.Value(), passwordArg.Value(), migrationDirectory, noPromptArg.HasValue());
                        break;
                    case "validatedatabase":
                        ValidateDatabase(serveraddressArg.Value(), databasenameArg.Value(), usernameArg.Value(), passwordArg.Value(), migrationDirectory, noPromptArg.HasValue());
                        break;
                    case "validateupgrade":
                        ValidateUpgrade(versionArg.Value(), serveraddressArg.Value(), databasenameArg.Value(), usernameArg.Value(), passwordArg.Value(), migrationDirectory, noPromptArg.HasValue());
                        break;
                    default:
                        break;
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

        private static void Upgrade(string toVersion, string servername, string databasename, string username, string password, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            var database = new Database(servername, databasename, username, password);
            var dbfolder = new DBFolder(migrationsDir);
            _logger.LogDebug($"Reading from {migrationsDir.FullName}");
            var validator = new VersionValidator();
            var dbVersions = database.GetDBState();
            validator.ValidateVersions(dbfolder.allVersions, dbVersions);
            var differ = new VersionDiff();

            var diff = differ.Diff(dbfolder.GetVersions(toVersion), dbVersions);

            var diffText = differ.DiffText(diff);
            _logger.LogInformation(diffText);
            if(!noPrompt)
                System.Console.ReadKey();
            var migrator = new Migrator(database, dbfolder);
            migrator.Upgrade(diff);
            if (!noPrompt)
                System.Console.ReadKey();
        }

        private static void Rollback(string toVersion, string servername, string databasename, string username, string password, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            var database1 = new Database(servername, databasename, username, password);
            var dbVersions1 = database1.GetDBState();
            var dbfolder1 = new DBFolder(migrationsDir);
            var validator1 = new VersionValidator();
            validator1.ValidateVersions(dbfolder1.allVersions, dbVersions1);
            var differ1 = new VersionDiff();
            var diff1 = differ1.Diff(dbVersions1, dbfolder1.GetVersions(toVersion));
            dbfolder1.AddRollbacks(diff1);
            var diffText1 = differ1.DiffText(diff1);
            _logger.LogInformation(diffText1);
            if (!noPrompt)
                System.Console.ReadKey();
            var migrator = new Migrator(database1, dbfolder1);
            migrator.Rollback(diff1);
            if (!noPrompt)
                System.Console.ReadKey();
        }

        private static void ValidateDatabase(string servername, string databasename, string username, string password, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            var database2 = new Database(servername, databasename, username, password);
            var dbVersions2 = database2.GetDBState();
            var dbfolder2 = new DBFolder(migrationsDir);
            var validator2 = new VersionValidator();
            var DBValidator = new DatabaseValidator(database2);
            validator2.ValidateVersions(dbfolder2.allVersions, dbVersions2);
            DBValidator.Validate();
            if (!noPrompt)
            System.Console.ReadKey();
        }

        private static void ValidateUpgrade(string toVersion, string servername, string databasename, string username, string password, DirectoryInfo migrationsDir, bool noPrompt = false)
        {
            var database2 = new Database(servername, databasename, username, password);
            var dbVersions2 = database2.GetDBState();
            var dbfolder2 = new DBFolder(migrationsDir);
            var validator2 = new VersionValidator();
            validator2.ValidateVersions(dbfolder2.allVersions, dbVersions2);
            if (!noPrompt)
                System.Console.ReadKey();
        }
    }
}
