namespace LiteDB.Shell.Commands
{
    internal class CollectionDropIndex : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "drop[iI]ndex");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            string col = ReadCollection(engine, s);
            string index = s.Scan(FieldPattern).Trim();

            return engine.DropIndex(col, index);
        }
    }
}