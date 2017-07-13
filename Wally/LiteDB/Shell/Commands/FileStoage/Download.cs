namespace LiteDB.Shell.Commands
{
    internal class FileDownload : BaseFileStorage, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsFileCommand(s, "download");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var fs = new LiteFileStorage(engine);
            string id = ReadId(s);
            string filename = s.Scan(@"\s*.*").Trim();

            var file = fs.FindById(id);

            if (file != null)
            {
                file.SaveAs(filename, true);

                return file.AsDocument;
            }
            return false;
        }
    }
}