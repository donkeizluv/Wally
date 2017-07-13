using System;
using System.Text;

namespace Wally.HTML
{
    internal class EncodingFoundException : Exception
    {
        internal EncodingFoundException(Encoding encoding)
        {
            Encoding = encoding;
        }

        internal Encoding Encoding { get; }
    }
}