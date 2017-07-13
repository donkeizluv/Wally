using System;
using System.Collections;
using System.Collections.Generic;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents a list of mixed code fragments.
    /// </summary>
    internal class MixedCodeDocumentFragmentList : IEnumerable
    {
        private readonly IList<MixedCodeDocumentFragment> _items = new List<MixedCodeDocumentFragment>();

        internal MixedCodeDocumentFragmentList(MixedCodeDocument doc)
        {
            Doc = doc;
        }

        /// <summary>
        ///     Gets the number of fragments contained in the list.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        ///     Gets the Document
        /// </summary>
        public MixedCodeDocument Doc { get; }

        /// <summary>
        ///     Gets a fragment from the list using its index.
        /// </summary>
        public MixedCodeDocumentFragment this[int index]
        {
            get { return _items[index]; }
        }

        /// <summary>
        ///     Gets an enumerator that can iterate through the fragment list.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Appends a fragment to the list of fragments.
        /// </summary>
        /// <param name="newFragment">The fragment to append. May not be null.</param>
        public void Append(MixedCodeDocumentFragment newFragment)
        {
            if (newFragment == null)
            {
                throw new ArgumentNullException("newFragment");
            }
            _items.Add(newFragment);
        }

        internal void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        ///     Gets an enumerator that can iterate through the fragment list.
        /// </summary>
        public MixedCodeDocumentFragmentEnumerator GetEnumerator()
        {
            return new MixedCodeDocumentFragmentEnumerator(_items);
        }

        internal int GetFragmentIndex(MixedCodeDocumentFragment fragment)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException("fragment");
            }
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == fragment)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Prepends a fragment to the list of fragments.
        /// </summary>
        /// <param name="newFragment">The fragment to append. May not be null.</param>
        public void Prepend(MixedCodeDocumentFragment newFragment)
        {
            if (newFragment == null)
            {
                throw new ArgumentNullException("newFragment");
            }
            _items.Insert(0, newFragment);
        }

        /// <summary>
        ///     Remove a fragment from the list of fragments. If this fragment was not in the list, an exception will be raised.
        /// </summary>
        /// <param name="fragment">The fragment to remove. May not be null.</param>
        public void Remove(MixedCodeDocumentFragment fragment)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException("fragment");
            }
            int index = GetFragmentIndex(fragment);
            if (index == -1)
            {
                throw new IndexOutOfRangeException();
            }
            RemoveAt(index);
        }

        /// <summary>
        ///     Remove all fragments from the list.
        /// </summary>
        public void RemoveAll()
        {
            _items.Clear();
        }

        /// <summary>
        ///     Remove a fragment from the list of fragments, using its index in the list.
        /// </summary>
        /// <param name="index">The index of the fragment to remove.</param>
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        /// <summary>
        ///     Represents a fragment enumerator.
        /// </summary>
        internal class MixedCodeDocumentFragmentEnumerator : IEnumerator
        {
            private readonly IList<MixedCodeDocumentFragment> _items;
            private int _index;

            internal MixedCodeDocumentFragmentEnumerator(IList<MixedCodeDocumentFragment> items)
            {
                _items = items;
                _index = -1;
            }

            /// <summary>
            ///     Gets the current element in the collection.
            /// </summary>
            public MixedCodeDocumentFragment Current
            {
                get { return _items[_index]; }
            }

            /// <summary>
            ///     Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the
            ///     end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                _index = _index + 1;
                return _index < _items.Count;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = -1;
            }
        }
    }
}