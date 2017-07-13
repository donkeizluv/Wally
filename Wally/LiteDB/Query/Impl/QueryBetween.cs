﻿using System.Collections.Generic;

namespace LiteDB
{
    internal class QueryBetween : Query
    {
        private readonly BsonValue _end;
        private readonly BsonValue _start;

        public QueryBetween(string field, BsonValue start, BsonValue end)
            : base(field)
        {
            _start = start;
            _end = end;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            var start = _start.Normalize(index.Options);
            var end = _end.Normalize(index.Options);

            // define order
            int order = start.CompareTo(end) <= 0 ? Ascending : Descending;

            // find first indexNode
            var node = indexer.Find(index, start, true, order);

            // navigate using next[0] do next node - if less or equals returns
            while (node != null)
            {
                int diff = node.Key.CompareTo(end);

                if (diff == 0 || diff != order)
                {
                    yield return node;
                }
                else
                {
                    break;
                }

                node = indexer.GetNode(node.NextPrev(0, order));
            }
        }
    }
}