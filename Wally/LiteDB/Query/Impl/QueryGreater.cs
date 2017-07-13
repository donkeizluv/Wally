﻿using System.Collections.Generic;

namespace LiteDB
{
    internal class QueryGreater : Query
    {
        private readonly bool _equals;
        private readonly BsonValue _value;

        public QueryGreater(string field, BsonValue value, bool equals)
            : base(field)
        {
            _value = value;
            _equals = equals;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            // find first indexNode
            var value = _value.Normalize(index.Options);
            var node = indexer.Find(index, value, true, Ascending);

            if (node == null) yield break;

            // move until next is last
            while (node != null)
            {
                int diff = node.Key.CompareTo(value);

                if (diff == 1 || (_equals && diff == 0))
                {
                    if (node.IsHeadTail(index)) yield break;

                    yield return node;
                }

                node = indexer.GetNode(node.Next[0]);
            }
        }
    }
}