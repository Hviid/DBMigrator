using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DBMigrator.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            Bootstrapper.ConfigureServices(serviceCollection);

            var test = serviceCollection.BuildServiceProvider();
            var loggerFactory = test.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();

            Migrator migrator;
            switch (args[0])
            {
                case "upgrade":
                    var database = new Database(args[1], args[2], args[3], args[4]);
                    var dbfolder = new DBFolder();
                    var validator = new Validator();
                    var dbVersions = database.GetDBState();
                    validator.ValidateVersions(dbfolder.allVersions, dbVersions);
                    var differ = new VersionDiff();
                    var diff = differ.Diff(dbfolder.GetVersionsUpTo(args[5]), dbVersions);
                    var diffText = differ.DiffText(diff);
                    logger.LogInformation(diffText);
                    System.Console.ReadKey();
                    migrator = new Migrator(database, dbfolder);
                    migrator.Upgrade(diff);
                    System.Console.ReadKey();
                    break;
                case "rollback":
                    var database1 = new Database(args[2], args[3], args[4], args[5]);
                    var dbVersions1 = database1.GetDBState();
                    var dbfolder1 = new DBFolder();
                    var validator1 = new Validator();
                    validator1.ValidateVersions(dbfolder1.allVersions, dbVersions1);
                    var differ1 = new VersionDiff();
                    var diff1 = differ1.Diff(dbVersions1, dbfolder1.GetVersionsUpTo(args[1]));
                    var diffText1 = differ1.DiffText(diff1);
                    logger.LogInformation(diffText1);
                    System.Console.ReadKey();
                    migrator = new Migrator(database1, dbfolder1);
                    migrator.Rollback(diff1);
                    System.Console.ReadKey();
                    break;
                case "validate":
                    var database2 = new Database(args[1], args[2], args[3], args[4]);
                    var dbVersions2 = database2.GetDBState();
                    var dbfolder2 = new DBFolder();
                    var validator2 = new Validator();
                    validator2.ValidateVersions(dbfolder2.allVersions, dbVersions2);
                    System.Console.ReadKey();
                    break;
                default:
                    break;
            }
        }
    }
}
