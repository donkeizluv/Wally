using System;

namespace LiteDB
{
    /// <summary>
    ///     Indicate that property will not be persist in Bson serialization
    /// </summary>
    internal class BsonIgnoreAttribute : Attribute
    {
    }
}