using System;

namespace LiteDB
{
    /// <summary>
    ///     Indicate that property will be used as BsonDocument Id
    /// </summary>
    internal class BsonIdAttribute : Attribute
    {
        public BsonIdAttribute()
        {
            AutoId = true;
        }

        public BsonIdAttribute(bool autoId)
        {
            AutoId = autoId;
        }

        public bool AutoId { get; private set; }
    }
}