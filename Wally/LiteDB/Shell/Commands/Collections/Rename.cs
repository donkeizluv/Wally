namespace LiteDB.Shell.Commands
{
    internal class CollectionRename : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "rename");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            string col = ReadCollection(engine, s);
            string newName = s.Scan(@"[\w-]+").ThrowIfEmpty("Invalid new collection name");

            return engine.RenameCollection(col, newName);
        }
    }
}