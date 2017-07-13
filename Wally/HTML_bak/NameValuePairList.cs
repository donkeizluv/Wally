using System;
using System.Collections.Generic;

namespace Wally.HTML
{
    internal class NameValuePairList
    {
        internal readonly string Text;
        private List<KeyValuePair<string, string>> _allPairs;
        private Dictionary<string, List<KeyValuePair<string, string>>> _pairsWithName;

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

        internal static string GetNameValuePairsValue(string text, string name)
        {
            NameValuePairList i = new NameValuePairList(text);
            return i.GetNameValuePairValue(name);
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

        internal string GetNameValuePairValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            List<KeyValuePair<string, string>> al = GetNameValuePairs(name);
            if (al.Count == 0)
            {
                return string.Empty;
            }
            return al[0].Value.Trim();
        }

        private void Parse(string text)
        {
            _allPairs.Clear();
            _pairsWithName.Clear();
            if (text == null)
            {
                return;
            }
            string[] p = text.Split(';');
            string[] array = p;
            for (int i = 0; i < array.Length; i++)
            {
                string pv = array[i];
                if (pv.Length != 0)
                {
                    string[] onep = pv.Split(new[]
                    {
                        '='
                    }, 2);
                    if (onep.Length != 0)
                    {
                        KeyValuePair<string, string> nvp = new KeyValuePair<string, string>(onep[0].Trim().ToLower(),
                            onep.Length < 2 ? "" : onep[1]);
                        _allPairs.Add(nvp);
                        List<KeyValuePair<string, string>> al;
                        if (!_pairsWithName.ContainsKey(nvp.Key))
                        {
                            al = new List<KeyValuePair<string, string>>();
                            _pairsWithName[nvp.Key] = al;
                        }
                        else
                        {
                            al = _pairsWithName[nvp.Key];
                        }
                        al.Add(nvp);
                    }
                }
            }
        }
    }
}