using System;
using System.Text;

namespace Wally.HTML
{
    internal class EncodingFoundException : Exception
    {
        internal Encoding Encoding { get; }

        internal EncodingFoundException(Encoding encoding)
        {
            Encoding = encoding;
        }
    }
}