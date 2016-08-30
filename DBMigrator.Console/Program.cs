using Microsoft.Extensions.Logging;

namespace DBMigrator.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            //Hack use DI instead
            var logger = loggerFactory.CreateLogger<Migrator>();
            new Logger(logger);

            Migrator migrator;
            switch (args[0])
            {
                case "upgrade":
                    var database = new Database(args[1], args[2], args[3], args[4]);
                    var dbfolder = new DBFolder();
                    var validator = new Validator();
                    validator.ValidateVersions(dbfolder.allVersions, database.allVersions);
                    var differ = new VersionDiff();
                    var diff = differ.Diff(dbfolder.GetVersionsUpTo(args[5]), database.allVersions);
                    var diffText = differ.DiffText(diff);
                    logger.LogInformation(diffText);
                    System.Console.ReadKey();
                    migrator = new Migrator(database, dbfolder);
                    migrator.Upgrade(diff);
                    System.Console.ReadKey();
                    break;
                case "rollback":
                    var database1 = new Database(args[2], args[3], args[4], args[5]);
                    var dbfolder1 = new DBFolder();
                    var validator1 = new Validator();
                    validator1.ValidateVersions(dbfolder1.allVersions, database1.allVersions);
                    var differ1 = new VersionDiff();
                    var diff1 = differ1.Diff(database1.allVersions, dbfolder1.GetVersionsUpTo(args[1]));
                    var diffText1 = differ1.DiffText(diff1);
                    logger.LogInformation(diffText1);
                    System.Console.ReadKey();
                    migrator = new Migrator(database1, dbfolder1);
                    migrator.Rollback(diff1);
                    System.Console.ReadKey();
                    break;
                case "validate":
                    var database2 = new Database(args[1], args[2], args[3], args[4]);
                    var dbfolder2 = new DBFolder();
                    var validator2 = new Validator();
                    validator2.ValidateVersions(dbfolder2.allVersions, database2.allVersions);
                    System.Console.ReadKey();
                    break;
                default:
                    break;
            }
        }
    }
}
