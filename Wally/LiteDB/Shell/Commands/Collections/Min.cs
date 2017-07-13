namespace LiteDB.Shell.Commands
{
    internal class CollectionMin : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "min");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            string col = ReadCollection(engine, s);
            string index = s.Scan(FieldPattern).Trim();

            return engine.Min(col, index.Length == 0 ? "_id" : index);
        }
    }
}