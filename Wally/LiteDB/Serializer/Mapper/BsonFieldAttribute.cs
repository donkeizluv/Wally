using System;

namespace LiteDB
{
    /// <summary>
    ///     Set a name to this property in BsonDocument
    /// </summary>
    internal class BsonFieldAttribute : Attribute
    {
        public BsonFieldAttribute(string name)
        {
            Name = name;
        }

        public BsonFieldAttribute()
        {
        }

        public string Name { get; set; }
    }
}