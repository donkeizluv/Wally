using System;
using System.Collections;
using System.Collections.Generic;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents a combined list and collection of HTML nodes.
    /// </summary>
    internal class HtmlAttributeCollection : IList<HtmlAttribute>, ICollection<HtmlAttribute>,
        IEnumerable<HtmlAttribute>, IEnumerable
    {
        private readonly HtmlNode _ownernode;

        private readonly List<HtmlAttribute> items = new List<HtmlAttribute>();
        internal Dictionary<string, HtmlAttribute> Hashitems = new Dictionary<string, HtmlAttribute>();

        internal HtmlAttributeCollection(HtmlNode ownernode)
        {
            _ownernode = ownernode;
        }

        /// <summary>
        ///     Gets a given attribute from the list using its name.
        /// </summary>
        public HtmlAttribute this[string name]
        {
            get
            {
                HtmlAttribute value;
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                if (!Hashitems.TryGetValue(name.ToLower(), out value))
                {
                    return null;
                }
                return value;
            }
            set { Append(value); }
        }

        /// <summary>
        ///     Gets the number of elements actually contained in the list.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        ///     Gets readonly status of colelction
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the attribute at the specified index.
        /// </summary>
        public HtmlAttribute this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        /// <summary>
        ///     Adds supplied item to collection
        /// </summary>
        /// <param name="item"></param>
        public void Add(HtmlAttribute item)
        {
            Append(item);
        }

        /// <summary>
        ///     Retreives existence of supplied item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(HtmlAttribute item)
        {
            return items.Contains(item);
        }

        /// <summary>
        ///     Copies collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(HtmlAttribute[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Retrieves the index for the supplied item, -1 if not found
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(HtmlAttribute item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        ///     Inserts given item into collection at supplied index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, HtmlAttribute item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            Hashitems[item.Name] = item;
            item._ownernode = _ownernode;
            items.Insert(index, item);
            _ownernode.SetChanged();
        }

        /// <summary>
        ///     Removes the attribute at the specified index.
        /// </summary>
        /// <param name="index">The index of the attribute to remove.</param>
        public void RemoveAt(int index)
        {
            var att = items[index];
            Hashitems.Remove(att.Name);
            items.RemoveAt(index);
            _ownernode.SetChanged();
        }

        void ICollection<HtmlAttribute>.Clear()
        {
            items.Clear();
        }

        bool ICollection<HtmlAttribute>.Remove(HtmlAttribute item)
        {
            return items.Remove(item);
        }

        IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        ///     Explicit non-generic enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        ///     Adds a new attribute to the collection with the given values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            Append(name, value);
        }

        /// <summary>
        ///     Inserts the specified attribute as the last attribute in the collection.
        /// </summary>
        /// <param name="newAttribute">The attribute to insert. May not be null.</param>
        /// <returns>The appended attribute.</returns>
        public HtmlAttribute Append(HtmlAttribute newAttribute)
        {
            if (newAttribute == null)
            {
                throw new ArgumentNullException("newAttribute");
            }
            Hashitems[newAttribute.Name] = newAttribute;
            newAttribute._ownernode = _ownernode;
            items.Add(newAttribute);
            _ownernode.SetChanged();
            return newAttribute;
        }

        /// <summary>
        ///     Creates and inserts a new attribute as the last attribute in the collection.
        /// </summary>
        /// <param name="name">The name of the attribute to insert.</param>
        /// <returns>The appended attribute.</returns>
        public HtmlAttribute Append(string name)
        {
            return Append(_ownernode._ownerdocument.CreateAttribute(name));
        }

        /// <summary>
        ///     Creates and inserts a new attribute as the last attribute in the collection.
        /// </summary>
        /// <param name="name">The name of the attribute to insert.</param>
        /// <param name="value">The value of the attribute to insert.</param>
        /// <returns>The appended attribute.</returns>
        public HtmlAttribute Append(string name, string value)
        {
            var att = _ownernode._ownerdocument.CreateAttribute(name, value);
            return Append(att);
        }

        /// <summary>
        ///     Returns all attributes with specified name. Handles case insentivity
        /// </summary>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<HtmlAttribute> AttributesWithName(string attributeName)
        {
            attributeName = attributeName.ToLower();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name.Equals(attributeName))
                {
                    yield return items[i];
                }
            }
        }

        /// <summary>
        ///     Clears the attribute collection
        /// </summary>
        internal void Clear()
        {
            Hashitems.Clear();
            items.Clear();
        }

        /// <summary>
        ///     Checks for existance of attribute with given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name.Equals(name.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        internal int GetAttributeIndex(HtmlAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == attribute)
                {
                    return i;
                }
            }
            return -1;
        }

        internal int GetAttributeIndex(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            string lname = name.ToLower();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == lname)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Inserts the specified attribute as the first node in the collection.
        /// </summary>
        /// <param name="newAttribute">The attribute to insert. May not be null.</param>
        /// <returns>The prepended attribute.</returns>
        public HtmlAttribute Prepend(HtmlAttribute newAttribute)
        {
            Insert(0, newAttribute);
            return newAttribute;
        }

        /// <summary>
        ///     Removes a given attribute from the list.
        /// </summary>
        /// <param name="attribute">The attribute to remove. May not be null.</param>
        public void Remove(HtmlAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            int index = GetAttributeIndex(attribute);
            if (index == -1)
            {
                throw new IndexOutOfRangeException();
            }
            RemoveAt(index);
        }

        /// <summary>
        ///     Removes an attribute from the list, using its name. If there are more than one attributes with this name, they will
        ///     all be removed.
        /// </summary>
        /// <param name="name">The attribute's name. May not be null.</param>
        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            string lname = name.ToLower();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == lname)
                {
                    RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///     Removes all attributes from the collection
        /// </summary>
        public void Remove()
        {
            foreach (var item in items)
            {
                item.Remove();
            }
        }

        /// <summary>
        ///     Remove all attributes in the list.
        /// </summary>
        public void RemoveAll()
        {
            Hashitems.Clear();
            items.Clear();
            _ownernode.SetChanged();
        }
    }
}