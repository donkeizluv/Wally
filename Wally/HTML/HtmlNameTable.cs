using System.Xml;

namespace Wally.HTML
{
    internal class HtmlNameTable : XmlNameTable
    {
        private readonly NameTable _nametable = new NameTable();

        public override string Add(string array)
        {
            return _nametable.Add(array);
        }

        public override string Add(char[] array, int offset, int length)
        {
            return _nametable.Add(array, offset, length);
        }

        public override string Get(string array)
        {
            return _nametable.Get(array);
        }

        public override string Get(char[] array, int offset, int length)
        {
            return _nametable.Get(array, offset, length);
        }

        internal string GetOrAdd(string array)
        {
            string s = Get(array);
            if (s != null)
            {
                return s;
            }
            return Add(array);
        }
    }
}