namespace LiteDB.Shell.Commands
{
    internal class CollectionMax : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "max");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            string col = ReadCollection(engine, s);
            string index = s.Scan(FieldPattern).Trim();

            return engine.Max(col, index.Length == 0 ? "_id" : index);
        }
    }
}