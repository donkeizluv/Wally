using System;
using System.Collections.Generic;

namespace Wally.HTML
{
    internal class NameValuePairList
    {
        private readonly List<KeyValuePair<string, string>> _allPairs;

        private readonly Dictionary<string, List<KeyValuePair<string, string>>> _pairsWithName;
        internal readonly string Text;

        internal NameValuePairList() : this(null)
        {
        }

        internal NameValuePairList(string text)
        {
            Text = text;
            _allPairs = new List<KeyValuePair<string, string>>();
            _pairsWithName = new Dictionary<string, List<KeyValuePair<string, string>>>();
            Parse(text);
        }

        internal List<KeyValuePair<string, string>> GetNameValuePairs(string name)
        {
            if (name == null)
            {
                return _allPairs;
            }
            if (!_pairsWithName.ContainsKey(name))
            {
                return new List<KeyValuePair<string, string>>();
            }
            return _pairsWithName[name];
        }

        internal static string GetNameValuePairsValue(string text, string name)
        {
            return new NameValuePairList(text).GetNameValuePairValue(name);
        }

        internal string GetNameValuePairValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            var al = GetNameValuePairs(name);
            if (al.Count == 0)
            {
                return string.Empty;
            }
            return al[0].Value.Trim();
        }

        private void Parse(string text)
        {
            List<KeyValuePair<string, string>> al;
            _allPairs.Clear();
            _pairsWithName.Clear();
            if (text == null)
            {
                return;
            }
            var strArrays = text.Split(';');
            for (int i = 0; i < strArrays.Length; i++)
            {
                string pv = strArrays[i];
                if (pv.Length != 0)
                {
                    var onep = pv.Split(new[] {'='}, 2);
                    if (onep.Length != 0)
                    {
                        var nvp = new KeyValuePair<string, string>(onep[0].Trim().ToLower(),
                            onep.Length < 2 ? "" : onep[1]);
                        _allPairs.Add(nvp);
                        if (_pairsWithName.ContainsKey(nvp.Key))
                        {
                            al = _pairsWithName[nvp.Key];
                        }
                        else
                        {
                            al = new List<KeyValuePair<string, string>>();
                            _pairsWithName[nvp.Key] = al;
                        }
                        al.Add(nvp);
                    }
                }
            }
        }
    }
}