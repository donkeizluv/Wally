﻿using System.Collections.Generic;

namespace LiteDB
{
    internal class QueryLess : Query
    {
        private readonly bool _equals;
        private readonly BsonValue _value;

        public QueryLess(string field, BsonValue value, bool equals)
            : base(field)
        {
            _value = value;
            _equals = equals;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            var value = _value.Normalize(index.Options);

            foreach (var node in indexer.FindAll(index, Ascending))
            {
                int diff = node.Key.CompareTo(value);

                if (diff == 1 || (!_equals && diff == 0)) break;

                if (node.IsHeadTail(index)) yield break;

                yield return node;
            }
        }
    }
}