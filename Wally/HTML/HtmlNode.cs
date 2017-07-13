using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents an HTML node.
    /// </summary>
    [DebuggerDisplay("Name: {OriginalName}")]
    internal class HtmlNode : IXPathNavigable
    {
        internal const string DepthLevelExceptionMessage = "The document is too complex to parse";

        /// <summary>
        ///     Gets the name of a comment node. It is actually defined as '#comment'.
        /// </summary>
        public static readonly string HtmlNodeTypeNameComment;

        /// <summary>
        ///     Gets the name of the document node. It is actually defined as '#document'.
        /// </summary>
        public static readonly string HtmlNodeTypeNameDocument;

        /// <summary>
        ///     Gets the name of a text node. It is actually defined as '#text'.
        /// </summary>
        public static readonly string HtmlNodeTypeNameText;

        /// <summary>
        ///     Gets a collection of flags that define specific behaviors for specific element nodes.
        ///     The table contains a DictionaryEntry list with the lowercase tag name as the Key, and a combination of
        ///     HtmlElementFlags as the Value.
        /// </summary>
        public static Dictionary<string, HtmlElementFlag> ElementsFlags;

        internal HtmlAttributeCollection _attributes;

        private bool _changed;

        internal HtmlNodeCollection _childnodes;

        internal HtmlNode _endnode;

        internal string _innerhtml;

        internal int _innerlength;

        internal int _innerstartindex;

        internal int _line;

        internal int _lineposition;

        internal int _namelength;

        internal int _namestartindex;

        internal HtmlNode _nextnode;

        internal HtmlNodeType _nodetype;

        private string _optimizedName;

        internal string _outerhtml;

        internal int _outerlength;

        internal int _outerstartindex;

        internal HtmlDocument _ownerdocument;

        internal HtmlNode _parentnode;

        internal HtmlNode _prevnode;

        internal HtmlNode _prevwithsamename;

        internal bool _starttag;

        internal int _streamposition;

        /// <summary>
        ///     Initialize HtmlNode. Builds a list of all tags that have special allowances
        /// </summary>
        static HtmlNode()
        {
            HtmlNodeTypeNameComment = "#comment";
            HtmlNodeTypeNameDocument = "#document";
            HtmlNodeTypeNameText = "#text";
            ElementsFlags = new Dictionary<string, HtmlElementFlag>
            {
                {"script", HtmlElementFlag.CData},
                {"style", HtmlElementFlag.CData},
                {"noxhtml", HtmlElementFlag.CData},
                {"base", HtmlElementFlag.Empty},
                {"link", HtmlElementFlag.Empty},
                {"meta", HtmlElementFlag.Empty},
                {"isindex", HtmlElementFlag.Empty},
                {"hr", HtmlElementFlag.Empty},
                {"col", HtmlElementFlag.Empty},
                {"img", HtmlElementFlag.Empty},
                {"param", HtmlElementFlag.Empty},
                {"embed", HtmlElementFlag.Empty},
                {"frame", HtmlElementFlag.Empty},
                {"wbr", HtmlElementFlag.Empty},
                {"bgsound", HtmlElementFlag.Empty},
                {"spacer", HtmlElementFlag.Empty},
                {"keygen", HtmlElementFlag.Empty},
                {"area", HtmlElementFlag.Empty},
                {"input", HtmlElementFlag.Empty},
                {"basefont", HtmlElementFlag.Empty},
                {"form", HtmlElementFlag.Empty | HtmlElementFlag.CanOverlap},
                {"option", HtmlElementFlag.Empty},
                {"br", HtmlElementFlag.Empty | HtmlElementFlag.Closed},
                {"p", HtmlElementFlag.Empty | HtmlElementFlag.Closed}
            };
        }

        /// <summary>
        ///     Initializes HtmlNode, providing type, owner and where it exists in a collection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ownerdocument"></param>
        /// <param name="index"></param>
        public HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
        {
            _nodetype = type;
            _ownerdocument = ownerdocument;
            _outerstartindex = index;
            switch (type)
            {
                case HtmlNodeType.Document:
                {
                    Name = HtmlNodeTypeNameDocument;
                    _endnode = this;
                    if (_ownerdocument.Openednodes != null && !Closed && -1 != index)
                    {
                        _ownerdocument.Openednodes.Add(index, this);
                    }
                    if (-1 != index || type == HtmlNodeType.Comment || type == HtmlNodeType.Text)
                    {
                        return;
                    }
                    SetChanged();
                    return;
                }
                case HtmlNodeType.Element:
                {
                    if (_ownerdocument.Openednodes != null && !Closed && -1 != index)
                    {
                        _ownerdocument.Openednodes.Add(index, this);
                    }
                    if (-1 != index || type == HtmlNodeType.Comment || type == HtmlNodeType.Text)
                    {
                        return;
                    }
                    SetChanged();
                    return;
                }
                case HtmlNodeType.Comment:
                {
                    Name = HtmlNodeTypeNameComment;
                    _endnode = this;
                    if (_ownerdocument.Openednodes != null && !Closed && -1 != index)
                    {
                        _ownerdocument.Openednodes.Add(index, this);
                    }
                    if (-1 != index || type == HtmlNodeType.Comment || type == HtmlNodeType.Text)
                    {
                        return;
                    }
                    SetChanged();
                    return;
                }
                case HtmlNodeType.Text:
                {
                    Name = HtmlNodeTypeNameText;
                    _endnode = this;
                    if (_ownerdocument.Openednodes != null && !Closed && -1 != index)
                    {
                        _ownerdocument.Openednodes.Add(index, this);
                    }
                    if (-1 != index || type == HtmlNodeType.Comment || type == HtmlNodeType.Text)
                    {
                        return;
                    }
                    SetChanged();
                    return;
                }
                default:
                {
                    if (_ownerdocument.Openednodes != null && !Closed && -1 != index)
                    {
                        _ownerdocument.Openednodes.Add(index, this);
                    }
                    if (-1 != index || type == HtmlNodeType.Comment || type == HtmlNodeType.Text)
                    {
                        return;
                    }
                    SetChanged();
                    return;
                }
            }
        }

        /// <summary>
        ///     Gets the collection of HTML attributes for this node. May not be null.
        /// </summary>
        public HtmlAttributeCollection Attributes
        {
            get
            {
                if (!HasAttributes)
                {
                    _attributes = new HtmlAttributeCollection(this);
                }
                return _attributes;
            }
            internal set { _attributes = value; }
        }

        /// <summary>
        ///     Gets all the children of the node.
        /// </summary>
        public HtmlNodeCollection ChildNodes
        {
            get
            {
                var htmlNodeCollections = _childnodes;
                if (htmlNodeCollections == null)
                {
                    var htmlNodeCollections1 = new HtmlNodeCollection(this);
                    var htmlNodeCollections2 = htmlNodeCollections1;
                    _childnodes = htmlNodeCollections1;
                    htmlNodeCollections = htmlNodeCollections2;
                }
                return htmlNodeCollections;
            }
            internal set { _childnodes = value; }
        }

        /// <summary>
        ///     Gets a value indicating if this node has been closed or not.
        /// </summary>
        public bool Closed
        {
            get { return _endnode != null; }
        }

        /// <summary>
        ///     Gets the collection of HTML attributes for the closing tag. May not be null.
        /// </summary>
        public HtmlAttributeCollection ClosingAttributes
        {
            get
            {
                if (!HasClosingAttributes)
                {
                    return new HtmlAttributeCollection(this);
                }
                return _endnode.Attributes;
            }
        }

        internal HtmlNode EndNode
        {
            get { return _endnode; }
        }

        /// <summary>
        ///     Gets the first child of the node.
        /// </summary>
        public HtmlNode FirstChild
        {
            get
            {
                if (!HasChildNodes)
                {
                    return null;
                }
                return _childnodes[0];
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the current node has any attributes.
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                if (_attributes == null)
                {
                    return false;
                }
                if (_attributes.Count <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this node has any child nodes.
        /// </summary>
        public bool HasChildNodes
        {
            get
            {
                if (_childnodes == null)
                {
                    return false;
                }
                if (_childnodes.Count <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the current node has any attributes on the closing tag.
        /// </summary>
        public bool HasClosingAttributes
        {
            get
            {
                if (_endnode == null || _endnode == this)
                {
                    return false;
                }
                if (_endnode._attributes == null)
                {
                    return false;
                }
                if (_endnode._attributes.Count <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        ///     Gets or sets the value of the 'id' HTML attribute. The document must have been parsed using the
        ///     OptionUseIdAttribute set to true.
        /// </summary>
        public string Id
        {
            get
            {
                if (_ownerdocument.Nodesid == null)
                {
                    throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
                }
                return GetId();
            }
            set
            {
                if (_ownerdocument.Nodesid == null)
                {
                    throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                SetId(value);
            }
        }

        /// <summary>
        ///     Gets or Sets the HTML between the start and end tags of the object.
        /// </summary>
        public virtual string InnerHtml
        {
            get
            {
                if (_changed)
                {
                    UpdateHtml();
                    return _innerhtml;
                }
                if (_innerhtml != null)
                {
                    return _innerhtml;
                }
                if (_innerstartindex < 0)
                {
                    return string.Empty;
                }
                return _ownerdocument.Text.Substring(_innerstartindex, _innerlength);
            }
            set
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(value);
                RemoveAllChildren();
                AppendChildren(doc.DocumentNode.ChildNodes);
            }
        }

        /// <summary>
        ///     Gets or Sets the text between the start and end tags of the object.
        /// </summary>
        public virtual string InnerText
        {
            get
            {
                if (_nodetype == HtmlNodeType.Text)
                {
                    return ((HtmlTextNode) this).Text;
                }
                if (_nodetype == HtmlNodeType.Comment)
                {
                    return ((HtmlCommentNode) this).Comment;
                }
                if (!HasChildNodes)
                {
                    return string.Empty;
                }
                string s = null;
                foreach (var node in ChildNodes)
                {
                    s = string.Concat(s, node.InnerText);
                }
                return s;
            }
        }

        /// <summary>
        ///     Gets the last child of the node.
        /// </summary>
        public HtmlNode LastChild
        {
            get
            {
                if (!HasChildNodes)
                {
                    return null;
                }
                return _childnodes[_childnodes.Count - 1];
            }
        }

        /// <summary>
        ///     Gets the line number of this node in the document.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        ///     Gets the column number of this node in the document.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
            internal set { _lineposition = value; }
        }

        /// <summary>
        ///     Gets or sets this node's name.
        /// </summary>
        public string Name
        {
            get
            {
                if (_optimizedName == null)
                {
                    if (OriginalName == null)
                    {
                        Name = _ownerdocument.Text.Substring(_namestartindex, _namelength);
                    }
                    if (OriginalName != null)
                    {
                        _optimizedName = OriginalName.ToLower();
                    }
                    else
                    {
                        _optimizedName = string.Empty;
                    }
                }
                return _optimizedName;
            }
            set
            {
                OriginalName = value;
                _optimizedName = null;
            }
        }

        /// <summary>
        ///     Gets the HTML node immediately following this element.
        /// </summary>
        public HtmlNode NextSibling
        {
            get { return _nextnode; }
            internal set { _nextnode = value; }
        }

        /// <summary>
        ///     Gets the type of this node.
        /// </summary>
        public HtmlNodeType NodeType
        {
            get { return _nodetype; }
            internal set { _nodetype = value; }
        }

        /// <summary>
        ///     The original unaltered name of the tag
        /// </summary>
        public string OriginalName { get; private set; }

        /// <summary>
        ///     Gets or Sets the object and its content in HTML.
        /// </summary>
        public virtual string OuterHtml
        {
            get
            {
                if (_changed)
                {
                    UpdateHtml();
                    return _outerhtml;
                }
                if (_outerhtml != null)
                {
                    return _outerhtml;
                }
                if (_outerstartindex < 0)
                {
                    return string.Empty;
                }
                return _ownerdocument.Text.Substring(_outerstartindex, _outerlength);
            }
        }

        /// <summary>
        ///     Gets the <see cref="T:HtmlAgilityPack.HtmlDocument" /> to which this node belongs.
        /// </summary>
        public HtmlDocument OwnerDocument
        {
            get { return _ownerdocument; }
            internal set { _ownerdocument = value; }
        }

        /// <summary>
        ///     Gets the parent of this node (for nodes that can have parents).
        /// </summary>
        public HtmlNode ParentNode
        {
            get { return _parentnode; }
            internal set { _parentnode = value; }
        }

        /// <summary>
        ///     Gets the node immediately preceding this node.
        /// </summary>
        public HtmlNode PreviousSibling
        {
            get { return _prevnode; }
            internal set { _prevnode = value; }
        }

        /// <summary>
        ///     Gets the stream position of this node in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition
        {
            get { return _streamposition; }
        }

        /// <summary>
        ///     Gets a valid XPath string that points to this node
        /// </summary>
        public string XPath
        {
            get
            {
                return
                    string.Concat(
                        ParentNode == null || ParentNode.NodeType == HtmlNodeType.Document
                            ? "/"
                            : string.Concat(ParentNode.XPath, "/"), GetRelativeXpath());
            }
        }

        /// <summary>
        ///     Creates a new XPathNavigator object for navigating this HTML node.
        /// </summary>
        /// <returns>
        ///     An XPathNavigator object. The XPathNavigator is positioned on the node from which the method was called. It is
        ///     not positioned on the root of the document.
        /// </returns>
        public XPathNavigator CreateNavigator()
        {
            return new HtmlNodeNavigator(OwnerDocument, this);
        }

        /// <summary>
        ///     Returns a collection of all ancestor nodes of this element.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Ancestors()
        {
            var parentNode = ParentNode;
            if (parentNode != null)
            {
                yield return parentNode;
                while (parentNode.ParentNode != null)
                {
                    yield return parentNode.ParentNode;
                    parentNode = parentNode.ParentNode;
                }
            }
        }

        /// <summary>
        ///     Get Ancestors with matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Ancestors(string name)
        {
            HtmlNode i;
            for (i = ParentNode; i != null; i = i.ParentNode)
            {
                if (i.Name == name)
                {
                    yield return i;
                }
            }
            i = null;
        }

        /// <summary>
        ///     Returns a collection of all ancestor nodes of this element.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> AncestorsAndSelf()
        {
            HtmlNode i;
            for (i = this; i != null; i = i.ParentNode)
            {
                yield return i;
            }
            i = null;
        }

        /// <summary>
        ///     Gets all anscestor nodes and the current node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> AncestorsAndSelf(string name)
        {
            HtmlNode i;
            for (i = this; i != null; i = i.ParentNode)
            {
                if (i.Name == name)
                {
                    yield return i;
                }
            }
            i = null;
        }

        /// <summary>
        ///     Adds the specified node to the end of the list of children of this node.
        /// </summary>
        /// <param name="newChild">The node to add. May not be null.</param>
        /// <returns>The node added.</returns>
        public HtmlNode AppendChild(HtmlNode newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            ChildNodes.Append(newChild);
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
            return newChild;
        }

        /// <summary>
        ///     Adds the specified node to the end of the list of children of this node.
        /// </summary>
        /// <param name="newChildren">The node list to add. May not be null.</param>
        public void AppendChildren(HtmlNodeCollection newChildren)
        {
            if (newChildren == null)
            {
                throw new ArgumentNullException("newChildren");
            }
            foreach (var newChild in newChildren)
            {
                AppendChild(newChild);
            }
        }

        /// <summary>
        ///     Determines if an element node can be kept overlapped.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be <c>null</c>.</param>
        /// <returns>true if the name is the name of an element node that can be kept overlapped, <c>false</c> otherwise.</returns>
        public static bool CanOverlapElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!ElementsFlags.ContainsKey(name.ToLower()))
            {
                return false;
            }
            return (int) (ElementsFlags[name.ToLower()] & HtmlElementFlag.CanOverlap) != 0;
        }

        /// <summary>
        ///     Gets all Attributes with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlAttribute> ChildAttributes(string name)
        {
            return Attributes.AttributesWithName(name);
        }

        /// <summary>
        ///     Creates a duplicate of the node
        /// </summary>
        /// <returns></returns>
        public HtmlNode Clone()
        {
            return CloneNode(true);
        }

        /// <summary>
        ///     Creates a duplicate of the node and changes its name at the same time.
        /// </summary>
        /// <param name="newName">The new name of the cloned node. May not be <c>null</c>.</param>
        /// <returns>The cloned node.</returns>
        public HtmlNode CloneNode(string newName)
        {
            return CloneNode(newName, true);
        }

        /// <summary>
        ///     Creates a duplicate of the node and changes its name at the same time.
        /// </summary>
        /// <param name="newName">The new name of the cloned node. May not be null.</param>
        /// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
        /// <returns>The cloned node.</returns>
        public HtmlNode CloneNode(string newName, bool deep)
        {
            if (newName == null)
            {
                throw new ArgumentNullException("newName");
            }
            var htmlNode = CloneNode(deep);
            htmlNode.Name = newName;
            return htmlNode;
        }

        /// <summary>
        ///     Creates a duplicate of the node.
        /// </summary>
        /// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
        /// <returns>The cloned node.</returns>
        public HtmlNode CloneNode(bool deep)
        {
            var node = _ownerdocument.CreateNode(_nodetype);
            node.Name = Name;
            var htmlNodeType = _nodetype;
            if (htmlNodeType == HtmlNodeType.Comment)
            {
                ((HtmlCommentNode) node).Comment = ((HtmlCommentNode) this).Comment;
                return node;
            }
            if (htmlNodeType == HtmlNodeType.Text)
            {
                ((HtmlTextNode) node).Text = ((HtmlTextNode) this).Text;
                return node;
            }
            if (HasAttributes)
            {
                foreach (var _attribute in _attributes)
                {
                    var newatt = _attribute.Clone();
                    node.Attributes.Append(newatt);
                }
            }
            if (HasClosingAttributes)
            {
                node._endnode = _endnode.CloneNode(false);
                foreach (var htmlAttribute in _endnode._attributes)
                {
                    var newatt = htmlAttribute.Clone();
                    node._endnode._attributes.Append(newatt);
                }
            }
            if (!deep)
            {
                return node;
            }
            if (!HasChildNodes)
            {
                return node;
            }
            foreach (var _childnode in _childnodes)
            {
                node.AppendChild(_childnode.Clone());
            }
            return node;
        }

        internal void CloseNode(HtmlNode endnode, int level = 0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException("The document is too complex to parse");
            }
            if (!_ownerdocument.OptionAutoCloseOnEnd && _childnodes != null)
            {
                foreach (var child in _childnodes)
                {
                    if (child.Closed)
                    {
                        continue;
                    }
                    var close = new HtmlNode(NodeType, _ownerdocument, -1);
                    close._endnode = close;
                    child.CloseNode(close, level + 1);
                }
            }
            if (!Closed)
            {
                _endnode = endnode;
                if (_ownerdocument.Openednodes != null)
                {
                    _ownerdocument.Openednodes.Remove(_outerstartindex);
                }
                if (Utilities.GetDictionaryValueOrNull(_ownerdocument.Lastnodes, Name) == this)
                {
                    _ownerdocument.Lastnodes.Remove(Name);
                    _ownerdocument.UpdateLastParentNode();
                }
                if (endnode == this)
                {
                    return;
                }
                _innerstartindex = _outerstartindex + _outerlength;
                _innerlength = endnode._outerstartindex - _innerstartindex;
                _outerlength = endnode._outerstartindex + endnode._outerlength - _outerstartindex;
            }
        }

        /// <summary>
        ///     Creates a duplicate of the node and the subtree under it.
        /// </summary>
        /// <param name="node">The node to duplicate. May not be <c>null</c>.</param>
        public void CopyFrom(HtmlNode node)
        {
            CopyFrom(node, true);
        }

        /// <summary>
        ///     Creates a duplicate of the node.
        /// </summary>
        /// <param name="node">The node to duplicate. May not be <c>null</c>.</param>
        /// <param name="deep">true to recursively clone the subtree under the specified node, false to clone only the node itself.</param>
        public void CopyFrom(HtmlNode node, bool deep)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            Attributes.RemoveAll();
            if (node.HasAttributes)
            {
                foreach (var att in node.Attributes)
                {
                    SetAttributeValue(att.Name, att.Value);
                }
            }
            if (!deep)
            {
                RemoveAllChildren();
                if (node.HasChildNodes)
                {
                    foreach (var child in node.ChildNodes)
                    {
                        AppendChild(child.CloneNode(true));
                    }
                }
            }
        }

        /// <summary>
        ///     Creates an HTML node from a string representing literal HTML.
        /// </summary>
        /// <param name="html">The HTML text.</param>
        /// <returns>The newly created node instance.</returns>
        public static HtmlNode CreateNode(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument.DocumentNode.FirstChild;
        }

        /// <summary>
        ///     Creates an XPathNavigator using the root of this document.
        /// </summary>
        /// <returns></returns>
        public XPathNavigator CreateRootNavigator()
        {
            return new HtmlNodeNavigator(OwnerDocument, OwnerDocument.DocumentNode);
        }

        /// <summary>
        ///     Gets all Descendant nodes for this node and each of child nodes
        /// </summary>
        /// <param name="level">The depth level of the node to parse in the html tree</param>
        /// <returns>the current element as an HtmlNode</returns>
        [Obsolete("Use Descendants() instead, the results of this function will change in a future version")]
        public IEnumerable<HtmlNode> DescendantNodes(int level = 0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException("The document is too complex to parse");
            }
            foreach (var childNode in ChildNodes)
            {
                yield return childNode;
                foreach (var htmlNode in childNode.DescendantNodes(level + 1))
                {
                    yield return htmlNode;
                }
            }
        }

        /// <summary>
        ///     Returns a collection of all descendant nodes of this element, in document order
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use DescendantsAndSelf() instead, the results of this function will change in a future version")]
        public IEnumerable<HtmlNode> DescendantNodesAndSelf()
        {
            return DescendantsAndSelf();
        }

        /// <summary>
        ///     Gets all Descendant nodes in enumerated list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(int level = 0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException("The document is too complex to parse");
            }
            foreach (var childNode in ChildNodes)
            {
                yield return childNode;
                foreach (var htmlNode in childNode.Descendants(level + 1))
                {
                    yield return htmlNode;
                }
            }
        }

        /// <summary>
        ///     Get all descendant nodes with matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(string name)
        {
            name = name.ToLowerInvariant();
            foreach (var htmlNode in Descendants(0))
            {
                if (!htmlNode.Name.Equals(name))
                {
                    continue;
                }
                yield return htmlNode;
            }
        }

        /// <summary>
        ///     Returns a collection of all descendant nodes of this element, in document order
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantsAndSelf()
        {
            yield return this;
            foreach (var htmlNode in Descendants(0))
            {
                if (htmlNode == null)
                {
                    continue;
                }
                yield return htmlNode;
            }
        }

        /// <summary>
        ///     Gets all descendant nodes including this node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantsAndSelf(string name)
        {
            yield return this;
            foreach (var htmlNode in Descendants(0))
            {
                if (htmlNode.Name != name)
                {
                    continue;
                }
                yield return htmlNode;
            }
        }

        /// <summary>
        ///     Gets first generation child node matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlNode Element(string name)
        {
            HtmlNode htmlNode;
            using (var enumerator = ((IEnumerable<HtmlNode>) ChildNodes).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var node = enumerator.Current;
                    if (node.Name != name)
                    {
                        continue;
                    }
                    htmlNode = node;
                    return htmlNode;
                }
                return null;
            }
            return htmlNode;
        }

        /// <summary>
        ///     Gets matching first generation child nodes matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements(string name)
        {
            foreach (var childNode in ChildNodes)
            {
                if (childNode.Name != name)
                {
                    continue;
                }
                yield return childNode;
            }
        }

        /// <summary>
        ///     Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will
        ///     be returned.
        /// </summary>
        /// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
        /// <param name="def">The default value to return if not found.</param>
        /// <returns>The value of the attribute if found, the default value if not found.</returns>
        public string GetAttributeValue(string name, string def)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!HasAttributes)
            {
                return def;
            }
            var att = Attributes[name];
            if (att == null)
            {
                return def;
            }
            return att.Value;
        }

        /// <summary>
        ///     Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will
        ///     be returned.
        /// </summary>
        /// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
        /// <param name="def">The default value to return if not found.</param>
        /// <returns>The value of the attribute if found, the default value if not found.</returns>
        public int GetAttributeValue(string name, int def)
        {
            int num;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!HasAttributes)
            {
                return def;
            }
            var att = Attributes[name];
            if (att == null)
            {
                return def;
            }
            try
            {
                num = Convert.ToInt32(att.Value);
            }
            catch
            {
                num = def;
            }
            return num;
        }

        /// <summary>
        ///     Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will
        ///     be returned.
        /// </summary>
        /// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
        /// <param name="def">The default value to return if not found.</param>
        /// <returns>The value of the attribute if found, the default value if not found.</returns>
        public bool GetAttributeValue(string name, bool def)
        {
            bool flag;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!HasAttributes)
            {
                return def;
            }
            var att = Attributes[name];
            if (att == null)
            {
                return def;
            }
            try
            {
                flag = Convert.ToBoolean(att.Value);
            }
            catch
            {
                flag = def;
            }
            return flag;
        }

        internal string GetId()
        {
            var att = Attributes["id"];
            if (att == null)
            {
                return string.Empty;
            }
            return att.Value;
        }

        private string GetRelativeXpath()
        {
            object[] name;
            if (ParentNode == null)
            {
                return Name;
            }
            if (NodeType == HtmlNodeType.Document)
            {
                return string.Empty;
            }
            int i = 1;
            foreach (var node in ParentNode.ChildNodes)
            {
                if (node.Name != Name)
                {
                    continue;
                }
                if (node != this)
                {
                    i++;
                }
                else
                {
                    name = new object[] {Name, "[", i, "]"};
                    return string.Concat(name);
                }
            }
            name = new object[] {Name, "[", i, "]"};
            return string.Concat(name);
        }

        internal static string GetXmlComment(HtmlCommentNode comment)
        {
            string s = comment.Comment;
            return s.Substring(4, s.Length - 7).Replace("--", " - -");
        }

        /// <summary>
        ///     Inserts the specified node immediately after the specified reference node.
        /// </summary>
        /// <param name="newChild">The node to insert. May not be <c>null</c>.</param>
        /// <param name="refChild">The node that is the reference node. The newNode is placed after the refNode.</param>
        /// <returns>The node being inserted.</returns>
        public HtmlNode InsertAfter(HtmlNode newChild, HtmlNode refChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            if (refChild == null)
            {
                return PrependChild(newChild);
            }
            if (newChild == refChild)
            {
                return newChild;
            }
            int index = -1;
            if (_childnodes != null)
            {
                index = _childnodes[refChild];
            }
            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            if (_childnodes != null)
            {
                _childnodes.Insert(index + 1, newChild);
            }
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
            return newChild;
        }

        /// <summary>
        ///     Inserts the specified node immediately before the specified reference node.
        /// </summary>
        /// <param name="newChild">The node to insert. May not be <c>null</c>.</param>
        /// <param name="refChild">The node that is the reference node. The newChild is placed before this node.</param>
        /// <returns>The node being inserted.</returns>
        public HtmlNode InsertBefore(HtmlNode newChild, HtmlNode refChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            if (refChild == null)
            {
                return AppendChild(newChild);
            }
            if (newChild == refChild)
            {
                return newChild;
            }
            int index = -1;
            if (_childnodes != null)
            {
                index = _childnodes[refChild];
            }
            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            if (_childnodes != null)
            {
                _childnodes.Insert(index, newChild);
            }
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
            return newChild;
        }

        /// <summary>
        ///     Determines if an element node is a CDATA element node.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be null.</param>
        /// <returns>true if the name is the name of a CDATA element node, false otherwise.</returns>
        public static bool IsCDataElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!ElementsFlags.ContainsKey(name.ToLower()))
            {
                return false;
            }
            return (int) (ElementsFlags[name.ToLower()] & HtmlElementFlag.CData) != 0;
        }

        /// <summary>
        ///     Determines if an element node is closed.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be null.</param>
        /// <returns>true if the name is the name of a closed element node, false otherwise.</returns>
        public static bool IsClosedElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!ElementsFlags.ContainsKey(name.ToLower()))
            {
                return false;
            }
            return (int) (ElementsFlags[name.ToLower()] & HtmlElementFlag.Closed) != 0;
        }

        /// <summary>
        ///     Determines if an element node is defined as empty.
        /// </summary>
        /// <param name="name">The name of the element node to check. May not be null.</param>
        /// <returns>true if the name is the name of an empty element node, false otherwise.</returns>
        public static bool IsEmptyElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                return true;
            }
            if (33 == name[0])
            {
                return true;
            }
            if (63 == name[0])
            {
                return true;
            }
            if (!ElementsFlags.ContainsKey(name.ToLower()))
            {
                return false;
            }
            return (int) (ElementsFlags[name.ToLower()] & HtmlElementFlag.Empty) != 0;
        }

        /// <summary>
        ///     Determines if a text corresponds to the closing tag of an node that can be kept overlapped.
        /// </summary>
        /// <param name="text">The text to check. May not be null.</param>
        /// <returns>true or false.</returns>
        public static bool IsOverlappedClosingElement(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (text.Length <= 4)
            {
                return false;
            }
            if (text[0] != '<' || text[text.Length - 1] != '>' || text[1] != '/')
            {
                return false;
            }
            return CanOverlapElement(text.Substring(2, text.Length - 3));
        }

        /// <summary>
        ///     Adds the specified node to the beginning of the list of children of this node.
        /// </summary>
        /// <param name="newChild">The node to add. May not be <c>null</c>.</param>
        /// <returns>The node added.</returns>
        public HtmlNode PrependChild(HtmlNode newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            ChildNodes.Prepend(newChild);
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
            return newChild;
        }

        /// <summary>
        ///     Adds the specified node list to the beginning of the list of children of this node.
        /// </summary>
        /// <param name="newChildren">The node list to add. May not be <c>null</c>.</param>
        public void PrependChildren(HtmlNodeCollection newChildren)
        {
            if (newChildren == null)
            {
                throw new ArgumentNullException("newChildren");
            }
            foreach (var newChild in newChildren)
            {
                PrependChild(newChild);
            }
        }

        /// <summary>
        ///     Removes node from parent collection
        /// </summary>
        public void Remove()
        {
            if (ParentNode != null)
            {
                ParentNode.ChildNodes.Remove(this);
            }
        }

        /// <summary>
        ///     Removes all the children and/or attributes of the current node.
        /// </summary>
        public void RemoveAll()
        {
            RemoveAllChildren();
            if (HasAttributes)
            {
                _attributes.Clear();
            }
            if (_endnode != null && _endnode != this && _endnode._attributes != null)
            {
                _endnode._attributes.Clear();
            }
            SetChanged();
        }

        /// <summary>
        ///     Removes all the children of the current node.
        /// </summary>
        public void RemoveAllChildren()
        {
            if (!HasChildNodes)
            {
                return;
            }
            if (_ownerdocument.OptionUseIdAttribute)
            {
                foreach (var node in _childnodes)
                {
                    _ownerdocument.SetIdForNode(null, node.GetId());
                }
            }
            _childnodes.Clear();
            SetChanged();
        }

        /// <summary>
        ///     Removes the specified child node.
        /// </summary>
        /// <param name="oldChild">The node being removed. May not be <c>null</c>.</param>
        /// <returns>The node removed.</returns>
        public HtmlNode RemoveChild(HtmlNode oldChild)
        {
            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }
            int index = -1;
            if (_childnodes != null)
            {
                index = _childnodes[oldChild];
            }
            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            if (_childnodes != null)
            {
                _childnodes.Remove(index);
            }
            _ownerdocument.SetIdForNode(null, oldChild.GetId());
            SetChanged();
            return oldChild;
        }

        /// <summary>
        ///     Removes the specified child node.
        /// </summary>
        /// <param name="oldChild">The node being removed. May not be <c>null</c>.</param>
        /// <param name="keepGrandChildren">true to keep grand children of the node, false otherwise.</param>
        /// <returns>The node removed.</returns>
        public HtmlNode RemoveChild(HtmlNode oldChild, bool keepGrandChildren)
        {
            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }
            if (oldChild._childnodes != null & keepGrandChildren)
            {
                var prev = oldChild.PreviousSibling;
                foreach (var grandchild in oldChild._childnodes)
                {
                    InsertAfter(grandchild, prev);
                }
            }
            RemoveChild(oldChild);
            SetChanged();
            return oldChild;
        }

        /// <summary>
        ///     Replaces the child node oldChild with newChild node.
        /// </summary>
        /// <param name="newChild">The new node to put in the child list.</param>
        /// <param name="oldChild">The node being replaced in the list.</param>
        /// <returns>The node replaced.</returns>
        public HtmlNode ReplaceChild(HtmlNode newChild, HtmlNode oldChild)
        {
            if (newChild == null)
            {
                return RemoveChild(oldChild);
            }
            if (oldChild == null)
            {
                return AppendChild(newChild);
            }
            int index = -1;
            if (_childnodes != null)
            {
                index = _childnodes[oldChild];
            }
            if (index == -1)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            if (_childnodes != null)
            {
                _childnodes.Replace(index, newChild);
            }
            _ownerdocument.SetIdForNode(null, oldChild.GetId());
            _ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
            return newChild;
        }

        /// <summary>
        ///     Selects a list of nodes matching the <see cref="P:HtmlAgilityPack.HtmlNode.XPath" /> expression.
        /// </summary>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>
        ///     An <see cref="T:HtmlAgilityPack.HtmlNodeCollection" /> containing a collection of nodes matching the
        ///     <see cref="P:HtmlAgilityPack.HtmlNode.XPath" /> query, or <c>null</c> if no node matched the XPath expression.
        /// </returns>
        public HtmlNodeCollection SelectNodes(string xpath)
        {
            var list = new HtmlNodeCollection(null);
            var it = new HtmlNodeNavigator(OwnerDocument, this).Select(xpath);
            while (it.MoveNext())
            {
                list.Add(((HtmlNodeNavigator) it.Current).CurrentNode);
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list;
        }

        /// <summary>
        ///     Selects the first XmlNode that matches the XPath expression.
        /// </summary>
        /// <param name="xpath">The XPath expression. May not be null.</param>
        /// <returns>
        ///     The first <see cref="T:HtmlAgilityPack.HtmlNode" /> that matches the XPath query or a null reference if no
        ///     matching node was found.
        /// </returns>
        public HtmlNode SelectSingleNode(string xpath)
        {
            if (xpath == null)
            {
                throw new ArgumentNullException("xpath");
            }
            var it = new HtmlNodeNavigator(OwnerDocument, this).Select(xpath);
            if (!it.MoveNext())
            {
                return null;
            }
            return ((HtmlNodeNavigator) it.Current).CurrentNode;
        }

        /// <summary>
        ///     Helper method to set the value of an attribute of this node. If the attribute is not found, it will be created
        ///     automatically.
        /// </summary>
        /// <param name="name">The name of the attribute to set. May not be null.</param>
        /// <param name="value">The value for the attribute.</param>
        /// <returns>The corresponding attribute instance.</returns>
        public HtmlAttribute SetAttributeValue(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            var att = Attributes[name];
            if (att != null)
            {
                att.Value = value;
                return att;
            }
            return Attributes.Append(_ownerdocument.CreateAttribute(name, value));
        }

        internal void SetChanged()
        {
            _changed = true;
            if (ParentNode != null)
            {
                ParentNode.SetChanged();
            }
        }

        internal void SetId(string id)
        {
            var att = Attributes["id"] ?? _ownerdocument.CreateAttribute("id");
            att.Value = id;
            _ownerdocument.SetIdForNode(this, att.Value);
            SetChanged();
        }

        private void UpdateHtml()
        {
            _innerhtml = WriteContentTo();
            _outerhtml = WriteTo();
            _changed = false;
        }

        internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
        {
            string name;
            string quote = att.QuoteType == AttributeValueQuote.DoubleQuote ? "\"" : "'";
            if (_ownerdocument.OptionOutputAsXml)
            {
                name = _ownerdocument.OptionOutputUpperCase ? att.XmlName.ToUpper() : att.XmlName;
                if (_ownerdocument.OptionOutputOriginalCase)
                {
                    name = att.OriginalName;
                }
                outText.Write(string.Concat(" ", name, "=", quote, HtmlDocument.HtmlEncode(att.XmlValue), quote));
                return;
            }
            name = _ownerdocument.OptionOutputUpperCase ? att.Name.ToUpper() : att.Name;
            if (_ownerdocument.OptionOutputOriginalCase)
            {
                name = att.OriginalName;
            }
            if (att.Name.Length >= 4 && att.Name[0] == '<' && att.Name[1] == '%' && att.Name[att.Name.Length - 1] == '>' &&
                att.Name[att.Name.Length - 2] == '%')
            {
                outText.Write(string.Concat(" ", name));
                return;
            }
            if (!_ownerdocument.OptionOutputOptimizeAttributeValues)
            {
                outText.Write(string.Concat(" ", name, "=", quote, att.Value, quote));
                return;
            }
            if (att.Value.IndexOfAny(new[] {'\n', '\r', '\t', ' '}) < 0)
            {
                outText.Write(string.Concat(" ", name, "=", att.Value));
                return;
            }
            outText.Write(string.Concat(" ", name, "=", quote, att.Value, quote));
        }

        internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
        {
            if (!node.HasAttributes)
            {
                return;
            }
            foreach (var att in node.Attributes.Hashitems.Values)
            {
                writer.WriteAttributeString(att.XmlName, att.Value);
            }
        }

        internal void WriteAttributes(TextWriter outText, bool closing)
        {
            bool closed;
            int count;
            if (_ownerdocument.OptionOutputAsXml)
            {
                if (_attributes == null)
                {
                    return;
                }
                foreach (var att in _attributes.Hashitems.Values)
                {
                    WriteAttribute(outText, att);
                }
                return;
            }
            if (closing)
            {
                if (_endnode == null || _endnode._attributes == null || _endnode == this)
                {
                    return;
                }
                foreach (var att in _endnode._attributes)
                {
                    WriteAttribute(outText, att);
                }
                if (!_ownerdocument.OptionAddDebuggingAttributes)
                {
                    return;
                }
                var htmlDocument = _ownerdocument;
                closed = Closed;
                WriteAttribute(outText, htmlDocument.CreateAttribute("_closed", closed.ToString()));
                var htmlDocument1 = _ownerdocument;
                count = ChildNodes.Count;
                WriteAttribute(outText, htmlDocument1.CreateAttribute("_children", count.ToString()));
            }
            else
            {
                if (_attributes != null)
                {
                    foreach (var att in _attributes)
                    {
                        WriteAttribute(outText, att);
                    }
                }
                if (!_ownerdocument.OptionAddDebuggingAttributes)
                {
                    return;
                }
                var htmlDocument2 = _ownerdocument;
                closed = Closed;
                WriteAttribute(outText, htmlDocument2.CreateAttribute("_closed", closed.ToString()));
                var htmlDocument3 = _ownerdocument;
                count = ChildNodes.Count;
                WriteAttribute(outText, htmlDocument3.CreateAttribute("_children", count.ToString()));
                int i = 0;
                foreach (var n in ChildNodes)
                {
                    WriteAttribute(outText, _ownerdocument.CreateAttribute(string.Concat("_child_", i), n.Name));
                    i++;
                }
            }
        }

        /// <summary>
        ///     Saves all the children of the node to the specified TextWriter.
        /// </summary>
        /// <param name="outText">The TextWriter to which you want to save.</param>
        /// <param name="level">Identifies the level we are in starting at root with 0</param>
        public void WriteContentTo(TextWriter outText, int level = 0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException("The document is too complex to parse");
            }
            if (_childnodes == null)
            {
                return;
            }
            foreach (var _childnode in _childnodes)
            {
                _childnode.WriteTo(outText, level + 1);
            }
        }

        /// <summary>
        ///     Saves all the children of the node to a string.
        /// </summary>
        /// <returns>The saved string.</returns>
        public string WriteContentTo()
        {
            var sw = new StringWriter();
            WriteContentTo(sw, 0);
            sw.Flush();
            return sw.ToString();
        }

        /// <summary>
        ///     Saves the current node to the specified TextWriter.
        /// </summary>
        /// <param name="outText">The TextWriter to which you want to save.</param>
        /// <param name="level">identifies the level we are in starting at root with 0</param>
        public void WriteTo(TextWriter outText, int level = 0)
        {
            string html;
            switch (_nodetype)
            {
                case HtmlNodeType.Document:
                {
                    if (_ownerdocument.OptionOutputAsXml)
                    {
                        outText.Write(string.Concat("<?xml version=\"1.0\" encoding=\"",
                            _ownerdocument.GetOutEncoding().BodyName, "\"?>"));
                        if (_ownerdocument.DocumentNode.HasChildNodes)
                        {
                            int rootnodes = _ownerdocument.DocumentNode._childnodes.Count;
                            if (rootnodes > 0)
                            {
                                if (_ownerdocument.GetXmlDeclaration() != null)
                                {
                                    rootnodes--;
                                }
                                if (rootnodes > 1)
                                {
                                    if (_ownerdocument.OptionOutputUpperCase)
                                    {
                                        outText.Write("<SPAN>");
                                        WriteContentTo(outText, level);
                                        outText.Write("</SPAN>");
                                        return;
                                    }
                                    outText.Write("<span>");
                                    WriteContentTo(outText, level);
                                    outText.Write("</span>");
                                    return;
                                }
                            }
                        }
                    }
                    WriteContentTo(outText, level);
                    return;
                }
                case HtmlNodeType.Element:
                {
                    string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpper() : Name;
                    if (_ownerdocument.OptionOutputOriginalCase)
                    {
                        name = OriginalName;
                    }
                    if (_ownerdocument.OptionOutputAsXml)
                    {
                        if (name.Length <= 0 || name[0] == '?' || name.Trim().Length == 0)
                        {
                            return;
                        }
                        name = HtmlDocument.GetXmlName(name);
                    }
                    outText.Write(string.Concat("<", name));
                    WriteAttributes(outText, false);
                    if (HasChildNodes)
                    {
                        outText.Write(">");
                        bool cdata = false;
                        if (_ownerdocument.OptionOutputAsXml && IsCDataElement(Name))
                        {
                            cdata = true;
                            outText.Write("\r\n//<![CDATA[\r\n");
                        }
                        if (!cdata)
                        {
                            WriteContentTo(outText, level);
                        }
                        else
                        {
                            if (HasChildNodes)
                            {
                                ChildNodes[0].WriteTo(outText, level);
                            }
                            outText.Write("\r\n//]]>//\r\n");
                        }
                        outText.Write(string.Concat("</", name));
                        if (!_ownerdocument.OptionOutputAsXml)
                        {
                            WriteAttributes(outText, true);
                        }
                        outText.Write(">");
                        return;
                    }
                    if (IsEmptyElement(Name))
                    {
                        if (_ownerdocument.OptionWriteEmptyNodes || _ownerdocument.OptionOutputAsXml)
                        {
                            outText.Write(" />");
                            return;
                        }
                        if (Name.Length > 0 && Name[0] == '?')
                        {
                            outText.Write("?");
                        }
                        outText.Write(">");
                        return;
                    }
                    outText.Write(string.Concat("></", name, ">"));
                    return;
                }
                case HtmlNodeType.Comment:
                {
                    html = ((HtmlCommentNode) this).Comment;
                    if (!_ownerdocument.OptionOutputAsXml)
                    {
                        outText.Write(html);
                        return;
                    }
                    outText.Write(string.Concat("<!--", GetXmlComment((HtmlCommentNode) this), " -->"));
                    return;
                }
                case HtmlNodeType.Text:
                {
                    html = ((HtmlTextNode) this).Text;
                    outText.Write(_ownerdocument.OptionOutputAsXml ? HtmlDocument.HtmlEncode(html) : html);
                    return;
                }
                default:
                {
                    return;
                }
            }
        }

        /// <summary>
        ///     Saves the current node to the specified XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to which you want to save.</param>
        public void WriteTo(XmlWriter writer)
        {
            switch (_nodetype)
            {
                case HtmlNodeType.Document:
                {
                    writer.WriteProcessingInstruction("xml",
                        string.Concat("version=\"1.0\" encoding=\"", _ownerdocument.GetOutEncoding().BodyName, "\""));
                    if (!HasChildNodes)
                    {
                        break;
                    }
                    using (var enumerator = ((IEnumerable<HtmlNode>) ChildNodes).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.WriteTo(writer);
                        }
                    }
                    break;
                }
                case HtmlNodeType.Element:
                {
                    string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpper() : Name;
                    if (_ownerdocument.OptionOutputOriginalCase)
                    {
                        name = OriginalName;
                    }
                    writer.WriteStartElement(name);
                    WriteAttributes(writer, this);
                    if (HasChildNodes)
                    {
                        foreach (var childNode in ChildNodes)
                        {
                            childNode.WriteTo(writer);
                        }
                    }
                    writer.WriteEndElement();
                    break;
                }
                case HtmlNodeType.Comment:
                {
                    writer.WriteComment(GetXmlComment((HtmlCommentNode) this));
                    return;
                }
                case HtmlNodeType.Text:
                {
                    writer.WriteString(((HtmlTextNode) this).Text);
                    return;
                }
                default:
                {
                    return;
                }
            }
        }

        /// <summary>
        ///     Saves the current node to a string.
        /// </summary>
        /// <returns>The saved string.</returns>
        public string WriteTo()
        {
            string str;
            using (var sw = new StringWriter())
            {
                WriteTo(sw, 0);
                sw.Flush();
                str = sw.ToString();
            }
            return str;
        }
    }
}