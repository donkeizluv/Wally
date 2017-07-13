﻿namespace LiteDB.Shell.Commands
{
    internal class FileDelete : BaseFileStorage, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsFileCommand(s, "delete");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var fs = new LiteFileStorage(engine);
            string id = ReadId(s);

            return fs.Delete(id);
        }
    }
}