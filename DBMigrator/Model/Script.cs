namespace DBMigrator.Model
{
    public class Script
    {
        public Script(string filename)
        {
            FileName = filename;
        }

        public string FileName { get; }
        public string SQL { get; set; }
    }
}