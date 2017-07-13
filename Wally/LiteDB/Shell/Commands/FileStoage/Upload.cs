using System.IO;

namespace LiteDB.Shell.Commands
{
    internal class FileUpload : BaseFileStorage, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsFileCommand(s, "upload");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var fs = new LiteFileStorage(engine);
            string id = ReadId(s);

            string filename = Path.GetFullPath(s.Scan(@"\s*.*").Trim());

            if (!File.Exists(filename)) throw new IOException("File " + filename + " not found");

            var file = fs.Upload(id, filename);

            return file.AsDocument;
        }
    }
}