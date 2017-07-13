using System;
using System.Collections.Generic;

namespace LiteDB
{
    internal sealed partial class LiteCollection<T>
        where T : new()
    {
        private readonly DbEngine _engine;
        private readonly List<Action<BsonDocument>> _includes;
        private readonly Logger _log;
        private readonly BsonMapper _mapper;
        private readonly QueryVisitor<T> _visitor;

        internal LiteCollection(string name, DbEngine engine, BsonMapper mapper, Logger log)
        {
            Name = name;
            _engine = engine;
            _mapper = mapper;
            _log = log;
            _visitor = new QueryVisitor<T>(mapper);
            _includes = new List<Action<BsonDocument>>();
        }

        /// <summary>
        ///     Get collection name
        /// </summary>
        public string Name { get; }
    }
}