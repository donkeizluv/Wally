﻿using System.IO;
using System.Linq;
using System.Text;

namespace LiteDB.Shell.Commands
{
    internal class CollectionBulk : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "bulk");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            string col = ReadCollection(engine, s);
            string filename = s.Scan(@".*");

            using (var sr = new StreamReader(filename, Encoding.UTF8))
            {
                var docs = JsonSerializer.DeserializeArray(sr);

                return engine.Insert(col, docs.Select(x => x.AsDocument));
            }
        }
    }
}