﻿using System;
using System.Text;

namespace LiteDB
{
    /// <summary>
    ///     Internal class to deserialize a byte[] into a BsonDocument using BSON data format
    /// </summary>
    internal class BsonReader
    {
        // use byte array buffer for CString (key-only)
        private readonly byte[] _strBuffer = new byte[1000];

        /// <summary>
        ///     Main method - deserialize using ByteReader helper
        /// </summary>
        public BsonDocument Deserialize(byte[] bson)
        {
            return ReadDocument(new ByteReader(bson));
        }

        /// <summary>
        ///     Read a BsonDocument from reader
        /// </summary>
        public BsonDocument ReadDocument(ByteReader reader)
        {
            int length = reader.ReadInt32();
            int end = reader.Position + length - 5;
            var obj = new BsonDocument();

            while (reader.Position < end)
            {
                string name;
                var value = ReadElement(reader, out name);
                obj.RawValue[name] = value;
            }

            reader.ReadByte(); // zero

            return obj;
        }

        /// <summary>
        ///     Read an BsonArray from reader
        /// </summary>
        public BsonArray ReadArray(ByteReader reader)
        {
            int length = reader.ReadInt32();
            int end = reader.Position + length - 5;
            var arr = new BsonArray();

            while (reader.Position < end)
            {
                string name;
                var value = ReadElement(reader, out name);
                arr.Add(value);
            }

            reader.ReadByte(); // zero

            return arr;
        }

        /// <summary>
        ///     Reads an element (key-value) from an reader
        /// </summary>
        private BsonValue ReadElement(ByteReader reader, out string name)
        {
            byte type = reader.ReadByte();
            name = ReadCString(reader);

            if (type == 0x01) // Double
            {
                return reader.ReadDouble();
            }
            if (type == 0x02) // String
            {
                return ReadString(reader);
            }
            if (type == 0x03) // Document
            {
                return ReadDocument(reader);
            }
            if (type == 0x04) // Array
            {
                return ReadArray(reader);
            }
            if (type == 0x05) // Binary
            {
                int length = reader.ReadInt32();
                byte subType = reader.ReadByte();
                var bytes = reader.ReadBytes(length);

                switch (subType)
                {
                    case 0x00:
                        return bytes;
                    case 0x04:
                        return new Guid(bytes);
                }
            }
            else if (type == 0x07) // ObjectId
            {
                return new ObjectId(reader.ReadBytes(12));
            }
            else if (type == 0x08) // Boolean
            {
                return reader.ReadBoolean();
            }
            else if (type == 0x09) // DateTime
            {
                long ts = reader.ReadInt64();

                // catch specific values for MaxValue / MinValue #19
                if (ts == 253402300800000) return DateTime.MaxValue;
                if (ts == -62135596800000) return DateTime.MinValue;

                return BsonValue.UnixEpoch.AddMilliseconds(ts).ToLocalTime();
            }
            else if (type == 0x0A) // Null
            {
                return BsonValue.Null;
            }
            else if (type == 0x10) // Int32
            {
                return reader.ReadInt32();
            }
            else if (type == 0x12) // Int64
            {
                return reader.ReadInt64();
            }
            else if (type == 0xFF) // MinKey
            {
                return BsonValue.MinValue;
            }
            else if (type == 0x7F) // MaxKey
            {
                return BsonValue.MaxValue;
            }

            throw new NotSupportedException("BSON type not supported");
        }

        private string ReadString(ByteReader reader)
        {
            int length = reader.ReadInt32();
            var bytes = reader.ReadBytes(length - 1);
            reader.ReadByte(); // discard \x00
            return Encoding.UTF8.GetString(bytes);
        }

        private string ReadCString(ByteReader reader)
        {
            int pos = 0;

            while (true)
            {
                byte buf = reader.ReadByte();
                if (buf == 0x00) break;
                _strBuffer[pos++] = buf;
            }

            return Encoding.UTF8.GetString(_strBuffer, 0, pos);
        }
    }
}