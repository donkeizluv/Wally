using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents an HTML navigator on an HTML document seen as a data store.
    /// </summary>
    internal class HtmlNodeNavigator : XPathNavigator
    {
        private readonly HtmlNameTable _nametable = new HtmlNameTable();
        private int _attindex;

        internal bool Trace;

        internal HtmlNodeNavigator()
        {
            Reset();
        }

        internal HtmlNodeNavigator(HtmlDocument doc, HtmlNode currentNode)
        {
            if (currentNode == null)
            {
                throw new ArgumentNullException("currentNode");
            }
            if (currentNode.OwnerDocument != doc)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            CurrentDocument = doc;
            Reset();
            CurrentNode = currentNode;
        }

        private HtmlNodeNavigator(HtmlNodeNavigator nav)
        {
            if (nav == null)
            {
                throw new ArgumentNullException("nav");
            }
            CurrentDocument = nav.CurrentDocument;
            CurrentNode = nav.CurrentNode;
            _attindex = nav._attindex;
            _nametable = nav._nametable;
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public HtmlNodeNavigator(Stream stream)
        {
            CurrentDocument.Load(stream);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     stream.
        /// </param>
        public HtmlNodeNavigator(Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            CurrentDocument.Load(stream, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public HtmlNodeNavigator(Stream stream, Encoding encoding)
        {
            CurrentDocument.Load(stream, encoding);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     stream.
        /// </param>
        public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            CurrentDocument.Load(stream, encoding, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     stream.
        /// </param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            CurrentDocument.Load(stream, encoding, detectEncodingFromByteOrderMarks, buffersize);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
        public HtmlNodeNavigator(TextReader reader)
        {
            CurrentDocument.Load(reader);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public HtmlNodeNavigator(string path)
        {
            CurrentDocument.Load(path);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public HtmlNodeNavigator(string path, bool detectEncodingFromByteOrderMarks)
        {
            CurrentDocument.Load(path, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public HtmlNodeNavigator(string path, Encoding encoding)
        {
            CurrentDocument.Load(path, encoding);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            CurrentDocument.Load(path, encoding, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            CurrentDocument.Load(path, encoding, detectEncodingFromByteOrderMarks, buffersize);
            Reset();
        }

        /// <summary>
        ///     Gets the base URI for the current node.
        ///     Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string BaseURI
        {
            get { return _nametable.GetOrAdd(string.Empty); }
        }

        /// <summary>
        ///     Gets the current HTML document.
        /// </summary>
        public HtmlDocument CurrentDocument { get; } = new HtmlDocument();

        /// <summary>
        ///     Gets the current HTML node.
        /// </summary>
        public HtmlNode CurrentNode { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the current node has child nodes.
        /// </summary>
        public override bool HasAttributes
        {
            get { return CurrentNode.Attributes.Count > 0; }
        }

        /// <summary>
        ///     Gets a value indicating whether the current node has child nodes.
        /// </summary>
        public override bool HasChildren
        {
            get { return CurrentNode.ChildNodes.Count > 0; }
        }

        /// <summary>
        ///     Gets a value indicating whether the current node is an empty element.
        /// </summary>
        public override bool IsEmptyElement
        {
            get { return !HasChildren; }
        }

        /// <summary>
        ///     Gets the name of the current HTML node without the namespace prefix.
        /// </summary>
        public override string LocalName
        {
            get
            {
                if (_attindex == -1)
                {
                    return _nametable.GetOrAdd(CurrentNode.Name);
                }
                return _nametable.GetOrAdd(CurrentNode.Attributes[_attindex].Name);
            }
        }

        /// <summary>
        ///     Gets the qualified name of the current node.
        /// </summary>
        public override string Name
        {
            get { return _nametable.GetOrAdd(CurrentNode.Name); }
        }

        /// <summary>
        ///     Gets the namespace URI (as defined in the W3C Namespace Specification) of the current node.
        ///     Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string NamespaceURI
        {
            get { return _nametable.GetOrAdd(string.Empty); }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.
        /// </summary>
        public override XmlNameTable NameTable
        {
            get { return _nametable; }
        }

        /// <summary>
        ///     Gets the type of the current node.
        /// </summary>
        public override XPathNodeType NodeType
        {
            get
            {
                switch (CurrentNode.NodeType)
                {
                    case HtmlNodeType.Document:
                    {
                        return XPathNodeType.Root;
                    }
                    case HtmlNodeType.Element:
                    {
                        if (_attindex != -1)
                        {
                            return XPathNodeType.Attribute;
                        }
                        return XPathNodeType.Element;
                    }
                    case HtmlNodeType.Comment:
                    {
                        return XPathNodeType.Comment;
                    }
                    case HtmlNodeType.Text:
                    {
                        return XPathNodeType.Text;
                    }
                }
                throw new NotImplementedException(string.Concat("Internal error: Unhandled HtmlNodeType: ",
                    CurrentNode.NodeType));
            }
        }

        /// <summary>
        ///     Gets the prefix associated with the current node.
        ///     Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string Prefix
        {
            get { return _nametable.GetOrAdd(string.Empty); }
        }

        /// <summary>
        ///     Gets the text value of the current node.
        /// </summary>
        public override string Value
        {
            get
            {
                switch (CurrentNode.NodeType)
                {
                    case HtmlNodeType.Document:
                    {
                        return "";
                    }
                    case HtmlNodeType.Element:
                    {
                        if (_attindex == -1)
                        {
                            return CurrentNode.InnerText;
                        }
                        return CurrentNode.Attributes[_attindex].Value;
                    }
                    case HtmlNodeType.Comment:
                    {
                        return ((HtmlCommentNode) CurrentNode).Comment;
                    }
                    case HtmlNodeType.Text:
                    {
                        return ((HtmlTextNode) CurrentNode).Text;
                    }
                }
                throw new NotImplementedException(string.Concat("Internal error: Unhandled HtmlNodeType: ",
                    CurrentNode.NodeType));
            }
        }

        /// <summary>
        ///     Gets the xml:lang scope for the current node.
        ///     Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string XmlLang
        {
            get { return _nametable.GetOrAdd(string.Empty); }
        }

        /// <summary>
        ///     Creates a new HtmlNavigator positioned at the same node as this HtmlNavigator.
        /// </summary>
        /// <returns>A new HtmlNavigator object positioned at the same node as the original HtmlNavigator.</returns>
        public override XPathNavigator Clone()
        {
            return new HtmlNodeNavigator(this);
        }

        /// <summary>
        ///     Gets the value of the HTML attribute with the specified LocalName and NamespaceURI.
        /// </summary>
        /// <param name="localName">The local name of the HTML attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute. Unsupported with the HtmlNavigator implementation.</param>
        /// <returns>
        ///     The value of the specified HTML attribute. String.Empty or null if a matching attribute is not found or if the
        ///     navigator is not positioned on an element node.
        /// </returns>
        public override string GetAttribute(string localName, string namespaceURI)
        {
            var att = CurrentNode.Attributes[localName];
            if (att == null)
            {
                return null;
            }
            return att.Value;
        }

        /// <summary>
        ///     Returns the value of the namespace node corresponding to the specified local name.
        ///     Always returns string.Empty for the HtmlNavigator implementation.
        /// </summary>
        /// <param name="name">The local name of the namespace node.</param>
        /// <returns>Always returns string.Empty for the HtmlNavigator implementation.</returns>
        public override string GetNamespace(string name)
        {
            return string.Empty;
        }

        [Conditional("TRACE")]
        internal void InternalTrace(object traceValue)
        {
            string nodevalue;
            if (!Trace)
            {
                return;
            }
            string name = new StackFrame(1).GetMethod().Name;
            string nodename = CurrentNode == null ? "(null)" : CurrentNode.Name;
            if (CurrentNode != null)
            {
                switch (CurrentNode.NodeType)
                {
                    case HtmlNodeType.Document:
                    {
                        nodevalue = "";
                        break;
                    }
                    case HtmlNodeType.Element:
                    {
                        nodevalue = CurrentNode.CloneNode(false).OuterHtml;
                        break;
                    }
                    case HtmlNodeType.Comment:
                    {
                        nodevalue = ((HtmlCommentNode) CurrentNode).Comment;
                        break;
                    }
                    case HtmlNodeType.Text:
                    {
                        nodevalue = ((HtmlTextNode) CurrentNode).Text;
                        break;
                    }
                    default:
                    {
                        goto case HtmlNodeType.Element;
                    }
                }
            }
            else
            {
                nodevalue = "(null)";
            }
            HTML.Trace.WriteLine(
                string.Format("oid={0},n={1},a={2},v={3},{4}", GetHashCode(), nodename, _attindex, nodevalue, traceValue),
                string.Concat("N!", name));
        }

        /// <summary>
        ///     Determines whether the current HtmlNavigator is at the same position as the specified HtmlNavigator.
        /// </summary>
        /// <param name="other">The HtmlNavigator that you want to compare against.</param>
        /// <returns>true if the two navigators have the same position, otherwise, false.</returns>
        public override bool IsSamePosition(XPathNavigator other)
        {
            var nav = other as HtmlNodeNavigator;
            if (nav == null)
            {
                return false;
            }
            return nav.CurrentNode == CurrentNode;
        }

        /// <summary>
        ///     Moves to the same position as the specified HtmlNavigator.
        /// </summary>
        /// <param name="other">The HtmlNavigator positioned on the node that you want to move to.</param>
        /// <returns>true if successful, otherwise false. If false, the position of the navigator is unchanged.</returns>
        public override bool MoveTo(XPathNavigator other)
        {
            var nav = other as HtmlNodeNavigator;
            if (nav == null)
            {
                return false;
            }
            if (nav.CurrentDocument != CurrentDocument)
            {
                return false;
            }
            CurrentNode = nav.CurrentNode;
            _attindex = nav._attindex;
            return true;
        }

        /// <summary>
        ///     Moves to the HTML attribute with matching LocalName and NamespaceURI.
        /// </summary>
        /// <param name="localName">The local name of the HTML attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute. Unsupported with the HtmlNavigator implementation.</param>
        /// <returns>
        ///     true if the HTML attribute is found, otherwise, false. If false, the position of the navigator does not
        ///     change.
        /// </returns>
        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            int index = CurrentNode.Attributes.GetAttributeIndex(localName);
            if (index == -1)
            {
                return false;
            }
            _attindex = index;
            return true;
        }

        /// <summary>
        ///     Moves to the first sibling of the current node.
        /// </summary>
        /// <returns>
        ///     true if the navigator is successful moving to the first sibling node, false if there is no first sibling or if
        ///     the navigator is currently positioned on an attribute node.
        /// </returns>
        public override bool MoveToFirst()
        {
            if (CurrentNode.ParentNode == null)
            {
                return false;
            }
            if (CurrentNode.ParentNode.FirstChild == null)
            {
                return false;
            }
            CurrentNode = CurrentNode.ParentNode.FirstChild;
            return true;
        }

        /// <summary>
        ///     Moves to the first HTML attribute.
        /// </summary>
        /// <returns>true if the navigator is successful moving to the first HTML attribute, otherwise, false.</returns>
        public override bool MoveToFirstAttribute()
        {
            if (!HasAttributes)
            {
                return false;
            }
            _attindex = 0;
            return true;
        }

        /// <summary>
        ///     Moves to the first child of the current node.
        /// </summary>
        /// <returns>true if there is a first child node, otherwise false.</returns>
        public override bool MoveToFirstChild()
        {
            if (!CurrentNode.HasChildNodes)
            {
                return false;
            }
            CurrentNode = CurrentNode.ChildNodes[0];
            return true;
        }

        /// <summary>
        ///     Moves the XPathNavigator to the first namespace node of the current element.
        ///     Always returns false for the HtmlNavigator implementation.
        /// </summary>
        /// <param name="scope">An XPathNamespaceScope value describing the namespace scope.</param>
        /// <returns>Always returns false for the HtmlNavigator implementation.</returns>
        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            return false;
        }

        /// <summary>
        ///     Moves to the node that has an attribute of type ID whose value matches the specified string.
        /// </summary>
        /// <param name="id">
        ///     A string representing the ID value of the node to which you want to move. This argument does not need
        ///     to be atomized.
        /// </param>
        /// <returns>true if the move was successful, otherwise false. If false, the position of the navigator is unchanged.</returns>
        public override bool MoveToId(string id)
        {
            var node = CurrentDocument.GetElementbyId(id);
            if (node == null)
            {
                return false;
            }
            CurrentNode = node;
            return true;
        }

        /// <summary>
        ///     Moves the XPathNavigator to the namespace node with the specified local name.
        ///     Always returns false for the HtmlNavigator implementation.
        /// </summary>
        /// <param name="name">The local name of the namespace node.</param>
        /// <returns>Always returns false for the HtmlNavigator implementation.</returns>
        public override bool MoveToNamespace(string name)
        {
            return false;
        }

        /// <summary>
        ///     Moves to the next sibling of the current node.
        /// </summary>
        /// <returns>
        ///     true if the navigator is successful moving to the next sibling node, false if there are no more siblings or if
        ///     the navigator is currently positioned on an attribute node. If false, the position of the navigator is unchanged.
        /// </returns>
        public override bool MoveToNext()
        {
            if (CurrentNode.NextSibling == null)
            {
                return false;
            }
            CurrentNode = CurrentNode.NextSibling;
            return true;
        }

        /// <summary>
        ///     Moves to the next HTML attribute.
        /// </summary>
        /// <returns></returns>
        public override bool MoveToNextAttribute()
        {
            if (_attindex >= CurrentNode.Attributes.Count - 1)
            {
                return false;
            }
            _attindex = _attindex + 1;
            return true;
        }

        /// <summary>
        ///     Moves the XPathNavigator to the next namespace node.
        ///     Always returns falsefor the HtmlNavigator implementation.
        /// </summary>
        /// <param name="scope">An XPathNamespaceScope value describing the namespace scope.</param>
        /// <returns>Always returns false for the HtmlNavigator implementation.</returns>
        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            return false;
        }

        /// <summary>
        ///     Moves to the parent of the current node.
        /// </summary>
        /// <returns>true if there is a parent node, otherwise false.</returns>
        public override bool MoveToParent()
        {
            if (CurrentNode.ParentNode == null)
            {
                return false;
            }
            CurrentNode = CurrentNode.ParentNode;
            return true;
        }

        /// <summary>
        ///     Moves to the previous sibling of the current node.
        /// </summary>
        /// <returns>
        ///     true if the navigator is successful moving to the previous sibling node, false if there is no previous sibling
        ///     or if the navigator is currently positioned on an attribute node.
        /// </returns>
        public override bool MoveToPrevious()
        {
            if (CurrentNode.PreviousSibling == null)
            {
                return false;
            }
            CurrentNode = CurrentNode.PreviousSibling;
            return true;
        }

        /// <summary>
        ///     Moves to the root node to which the current node belongs.
        /// </summary>
        public override void MoveToRoot()
        {
            CurrentNode = CurrentDocument.DocumentNode;
        }

        private void Reset()
        {
            CurrentNode = CurrentDocument.DocumentNode;
            _attindex = -1;
        }
    }
}