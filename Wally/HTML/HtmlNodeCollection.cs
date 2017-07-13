using System;
using System.Collections;
using System.Collections.Generic;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents a combined list and collection of HTML nodes.
    /// </summary>
    internal class HtmlNodeCollection : IList<HtmlNode>, ICollection<HtmlNode>, IEnumerable<HtmlNode>, IEnumerable
    {
        private readonly List<HtmlNode> _items = new List<HtmlNode>();
        private readonly HtmlNode _parentnode;

        /// <summary>
        ///     Initialize the HtmlNodeCollection with the base parent node
        /// </summary>
        /// <param name="parentnode">The base node of the collection</param>
        public HtmlNodeCollection(HtmlNode parentnode)
        {
            _parentnode = parentnode;
        }

        /// <summary>
        ///     Gets a given node from the list.
        /// </summary>
        public int this[HtmlNode node]
        {
            get
            {
                int nodeIndex = GetNodeIndex(node);
                if (nodeIndex == -1)
                {
                    throw new ArgumentOutOfRangeException("node",
                        string.Concat("Node \"", node.CloneNode(false).OuterHtml, "\" was not found in the collection"));
                }
                return nodeIndex;
            }
        }

        /// <summary>
        ///     Get node with tag name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public HtmlNode this[string nodeName]
        {
            get
            {
                nodeName = nodeName.ToLower();
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i].Name.Equals(nodeName))
                    {
                        return _items[i];
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     Gets the number of elements actually contained in the list.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        ///     Is collection read only
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the node at the specified index.
        /// </summary>
        public HtmlNode this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        /// <summary>
        ///     Add node to the collection
        /// </summary>
        /// <param name="node"></param>
        public void Add(HtmlNode node)
        {
            _items.Add(node);
        }

        /// <summary>
        ///     Clears out the collection of HtmlNodes. Removes each nodes reference to parentnode, nextnode and prevnode
        /// </summary>
        public void Clear()
        {
            foreach (var _item in _items)
            {
                _item.ParentNode = null;
                _item.NextSibling = null;
                _item.PreviousSibling = null;
            }
            _items.Clear();
        }

        /// <summary>
        ///     Gets existence of node in collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(HtmlNode item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        ///     Copy collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(HtmlNode[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Get index of node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(HtmlNode item)
        {
            return _items.IndexOf(item);
        }

        /// <summary>
        ///     Insert node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Insert(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            if (index > 0)
            {
                prev = _items[index - 1];
            }
            if (index < _items.Count)
            {
                next = _items[index];
            }
            _items.Insert(index, node);
            if (prev != null)
            {
                if (node == prev)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                prev._nextnode = node;
            }
            if (next != null)
            {
                next._prevnode = node;
            }
            node._prevnode = prev;
            if (next == node)
            {
                throw new InvalidProgramException("Unexpected error.");
            }
            node._nextnode = next;
            node._parentnode = _parentnode;
        }

        /// <summary>
        ///     Remove node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(HtmlNode item)
        {
            RemoveAt(_items.IndexOf(item));
            return true;
        }

        /// <summary>
        ///     Remove <see cref="T:HtmlAgilityPack.HtmlNode" /> at index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            var item = _items[index];
            if (index > 0)
            {
                prev = _items[index - 1];
            }
            if (index < _items.Count - 1)
            {
                next = _items[index + 1];
            }
            _items.RemoveAt(index);
            if (prev != null)
            {
                if (next == prev)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                prev._nextnode = next;
            }
            if (next != null)
            {
                next._prevnode = prev;
            }
            item._prevnode = null;
            item._nextnode = null;
            item._parentnode = null;
        }

        IEnumerator<HtmlNode> IEnumerable<HtmlNode>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        ///     Get Explicit Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        ///     Add node to the end of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Append(HtmlNode node)
        {
            HtmlNode last = null;
            if (_items.Count > 0)
            {
                last = _items[_items.Count - 1];
            }
            _items.Add(node);
            node._prevnode = last;
            node._nextnode = null;
            node._parentnode = _parentnode;
            if (last == null)
            {
                return;
            }
            if (last == node)
            {
                throw new InvalidProgramException("Unexpected error.");
            }
            last._nextnode = node;
        }

        /// <summary>
        ///     Get all node descended from this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants()
        {
            foreach (var _item in _items)
            {
                foreach (var htmlNode in _item.Descendants(0))
                {
                    yield return htmlNode;
                }
            }
        }

        /// <summary>
        ///     Get all node descended from this collection with matching name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(string name)
        {
            foreach (var _item in _items)
            {
                foreach (var htmlNode in _item.Descendants(name))
                {
                    yield return htmlNode;
                }
            }
        }

        /// <summary>
        ///     Gets all first generation elements in collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements()
        {
            foreach (var _item in _items)
            {
                foreach (var childNode in _item.ChildNodes)
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>
        ///     Gets all first generation elements matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements(string name)
        {
            foreach (var _item in _items)
            {
                foreach (var htmlNode in _item.Elements(name))
                {
                    yield return htmlNode;
                }
            }
        }

        /// <summary>
        ///     Get first instance of node in supplied collection
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HtmlNode FindFirst(HtmlNodeCollection items, string name)
        {
            HtmlNode htmlNode;
            using (var enumerator = ((IEnumerable<HtmlNode>) items).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var node = enumerator.Current;
                    if (!node.Name.ToLower().Contains(name))
                    {
                        if (!node.HasChildNodes)
                        {
                            continue;
                        }
                        var returnNode = FindFirst(node.ChildNodes, name);
                        if (returnNode == null)
                        {
                            continue;
                        }
                        htmlNode = returnNode;
                        return htmlNode;
                    }
                    htmlNode = node;
                    return htmlNode;
                }
                return null;
            }
            return htmlNode;
        }

        /// <summary>
        ///     Get first instance of node with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlNode FindFirst(string name)
        {
            return FindFirst(this, name);
        }

        /// <summary>
        ///     Get index of node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeIndex(HtmlNode node)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (node == _items[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///     All first generation nodes in collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Nodes()
        {
            foreach (var _item in _items)
            {
                foreach (var childNode in _item.ChildNodes)
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>
        ///     Add node to the beginning of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Prepend(HtmlNode node)
        {
            HtmlNode first = null;
            if (_items.Count > 0)
            {
                first = _items[0];
            }
            _items.Insert(0, node);
            if (node == first)
            {
                throw new InvalidProgramException("Unexpected error.");
            }
            node._nextnode = first;
            node._prevnode = null;
            node._parentnode = _parentnode;
            if (first != null)
            {
                first._prevnode = node;
            }
        }

        /// <summary>
        ///     Remove node at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Remove(int index)
        {
            RemoveAt(index);
            return true;
        }

        /// <summary>
        ///     Replace node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Replace(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            var item = _items[index];
            if (index > 0)
            {
                prev = _items[index - 1];
            }
            if (index < _items.Count - 1)
            {
                next = _items[index + 1];
            }
            _items[index] = node;
            if (prev != null)
            {
                if (node == prev)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                prev._nextnode = node;
            }
            if (next != null)
            {
                next._prevnode = node;
            }
            node._prevnode = prev;
            if (next == node)
            {
                throw new InvalidProgramException("Unexpected error.");
            }
            node._nextnode = next;
            node._parentnode = _parentnode;
            item._prevnode = null;
            item._nextnode = null;
            item._parentnode = null;
        }
    }
}