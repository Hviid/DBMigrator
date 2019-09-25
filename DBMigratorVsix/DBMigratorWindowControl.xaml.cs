namespace DBMigratorVsix
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DBMigratorWindowControl.
    /// </summary>
    public partial class DBMigratorWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBMigratorWindowControl"/> class.
        /// </summary>
        public DBMigratorWindowControl()
        {
            this.InitializeComponent();
        }

        class Data
        {
            public string MigrationsPath { get; set; } = "";
            public string TargetVersion { get; set; } = "";
        }
        private void Upgrade(object sender, RoutedEventArgs e)
        {
            var cmd = BuildMigrationCmd("upgrade");

            System.Diagnostics.Process.Start("CMD.exe", $"/k {cmd}");
        }

        private void Downgrade(object sender, RoutedEventArgs e)
        {
            var cmd = BuildMigrationCmd("downgrade");

            System.Diagnostics.Process.Start("CMD.exe", $"/k {cmd}");
        }

        private string BuildMigrationCmd(string migrationType)
        {
            var username = Username.Text;
            var password = Password.Password;
            var server = Server.Text;
            var database = Database.Text;
            var path = MigrationPath.Text;
            var version = TargetVersion.Text;

            var cmd = $@"C:\dbmigrator\dbmigrator.exe {migrationType} -s {server} -d {database}";

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                cmd += $" -u {username} -p {password}";

            cmd += $"-f {path}";

            if (!string.IsNullOrEmpty(version))
                cmd += $" -v {version}";

            return cmd;
        }

        internal void SetMigrationPath(string folderPath)
        {
            var targetVersion = "";

            try
            {
                var dirName = new DirectoryInfo(folderPath);
                var version = new Version(dirName.Name);
                targetVersion = dirName.Name;
                folderPath = dirName.Parent.FullName;
            }
            catch (Exception)
            {
            }

            this.DataContext = new Data
            {
                MigrationsPath = folderPath,
                TargetVersion = targetVersion
            };
        }
    }
}