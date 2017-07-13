using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents a complete HTML document.
    /// </summary>
    internal class HtmlDocument : IXPathNavigable
    {
        /// <summary>
        ///     Defines the max level we would go deep into the html document
        /// </summary>
        private static int _maxDepthLevel;

        internal static readonly string HtmlExceptionRefNotChild;

        internal static readonly string HtmlExceptionUseIdAttributeFalse;

        private int _c;

        private Crc32 _crc32;

        private HtmlAttribute _currentattribute;

        private HtmlNode _currentnode;

        private bool _fullcomment;

        private int _index;

        private HtmlNode _lastparentnode;

        private int _line;

        private int _lineposition;

        private int _maxlineposition;

        private ParseState _oldstate;

        private bool _onlyDetectEncoding;

        private List<HtmlParseError> _parseerrors = new List<HtmlParseError>();

        private ParseState _state;

        internal Dictionary<string, HtmlNode> Lastnodes = new Dictionary<string, HtmlNode>();

        internal Dictionary<string, HtmlNode> Nodesid;

        internal Dictionary<int, HtmlNode> Openednodes;

        /// <summary>
        ///     Adds Debugging attributes to node. Default is false.
        /// </summary>
        public bool OptionAddDebuggingAttributes;

        /// <summary>
        ///     Defines if closing for non closed nodes must be done at the end or directly in the document.
        ///     Setting this to true can actually change how browsers render the page. Default is false.
        /// </summary>
        public bool OptionAutoCloseOnEnd;

        /// <summary>
        ///     Defines if non closed nodes will be checked at the end of parsing. Default is true.
        /// </summary>
        public bool OptionCheckSyntax = true;

        /// <summary>
        ///     Defines if a checksum must be computed for the document while parsing. Default is false.
        /// </summary>
        public bool OptionComputeChecksum;

        /// <summary>
        ///     Defines the default stream encoding to use. Default is System.Text.Encoding.Default.
        /// </summary>
        public Encoding OptionDefaultStreamEncoding;

        /// <summary>
        ///     Defines if source text must be extracted while parsing errors.
        ///     If the document has a lot of errors, or cascading errors, parsing performance can be dramatically affected if set
        ///     to true.
        ///     Default is false.
        /// </summary>
        public bool OptionExtractErrorSourceText;

        /// <summary>
        ///     Defines the maximum length of source text or parse errors. Default is 100.
        /// </summary>
        public int OptionExtractErrorSourceTextMaxLength = 100;

        /// <summary>
        ///     Defines if LI, TR, TH, TD tags must be partially fixed when nesting errors are detected. Default is false.
        /// </summary>
        public bool OptionFixNestedTags;

        /// <summary>
        ///     Defines if output must conform to XML, instead of HTML.
        /// </summary>
        public bool OptionOutputAsXml;

        /// <summary>
        ///     Defines if attribute value output must be optimized (not bound with double quotes if it is possible). Default is
        ///     false.
        /// </summary>
        public bool OptionOutputOptimizeAttributeValues;

        /// <summary>
        ///     Defines if name must be output with it's original case. Useful for asp.net tags and attributes
        /// </summary>
        public bool OptionOutputOriginalCase;

        /// <summary>
        ///     Defines if name must be output in uppercase. Default is false.
        /// </summary>
        public bool OptionOutputUpperCase;

        /// <summary>
        ///     Defines if declared encoding must be read from the document.
        ///     Declared encoding is determined using the meta http-equiv="content-type" content="text/html;charset=XXXXX" html
        ///     node.
        ///     Default is true.
        /// </summary>
        public bool OptionReadEncoding = true;

        /// <summary>
        ///     Defines the name of a node that will throw the StopperNodeException when found as an end node. Default is null.
        /// </summary>
        public string OptionStopperNodeName;

        /// <summary>
        ///     Defines if the 'id' attribute must be specifically used. Default is true.
        /// </summary>
        public bool OptionUseIdAttribute = true;

        /// <summary>
        ///     Defines if empty nodes must be written as closed during output. Default is false.
        /// </summary>
        public bool OptionWriteEmptyNodes;

        internal string Text;

        static HtmlDocument()
        {
            _maxDepthLevel = 2147483647;
            HtmlExceptionRefNotChild = "Reference node must be a child of this node";
            HtmlExceptionUseIdAttributeFalse = "You need to set UseIdAttribute property to true to enable this feature";
        }

        /// <summary>
        ///     Creates an instance of an HTML document.
        /// </summary>
        public HtmlDocument()
        {
            DocumentNode = CreateNode(HtmlNodeType.Document, 0);
            OptionDefaultStreamEncoding = Encoding.Default;
        }

        /// <summary>
        ///     Gets the document CRC32 checksum if OptionComputeChecksum was set to true before parsing, 0 otherwise.
        /// </summary>
        public int CheckSum
        {
            get
            {
                if (_crc32 == null)
                {
                    return 0;
                }
                return (int) _crc32.CheckSum;
            }
        }

        /// <summary>
        ///     Gets the document's declared encoding.
        ///     Declared encoding is determined using the meta http-equiv="content-type" content="text/html;charset=XXXXX" html
        ///     node.
        /// </summary>
        public Encoding DeclaredEncoding { get; private set; }

        /// <summary>
        ///     Gets the root node of the document.
        /// </summary>
        public HtmlNode DocumentNode { get; private set; }

        /// <summary>
        ///     Gets the document's output encoding.
        /// </summary>
        public Encoding Encoding
        {
            get { return GetOutEncoding(); }
        }

        /// <summary>
        ///     Defines the max level we would go deep into the html document. If this depth level is exceeded, and exception is
        ///     thrown.
        /// </summary>
        public static int MaxDepthLevel
        {
            get { return _maxDepthLevel; }
            set { _maxDepthLevel = value; }
        }

        /// <summary>
        ///     Gets a list of parse errors found in the document.
        /// </summary>
        public IEnumerable<HtmlParseError> ParseErrors
        {
            get { return _parseerrors; }
        }

        /// <summary>
        ///     Gets the remaining text.
        ///     Will always be null if OptionStopperNodeName is null.
        /// </summary>
        public string Remainder { get; private set; }

        /// <summary>
        ///     Gets the offset of Remainder in the original Html text.
        ///     If OptionStopperNodeName is null, this will return the length of the original Html text.
        /// </summary>
        public int RemainderOffset { get; private set; }

        /// <summary>
        ///     Gets the document's stream encoding.
        /// </summary>
        public Encoding StreamEncoding { get; private set; }

        /// <summary>
        ///     Creates a new XPathNavigator object for navigating this HTML document.
        /// </summary>
        /// <returns>An XPathNavigator object. The XPathNavigator is positioned on the root of the document.</returns>
        public XPathNavigator CreateNavigator()
        {
            return new HtmlNodeNavigator(this, DocumentNode);
        }

        private void AddError(HtmlParseErrorCode code, int line, int linePosition, int streamPosition, string sourceText,
            string reason)
        {
            var err = new HtmlParseError(code, line, linePosition, streamPosition, sourceText, reason);
            _parseerrors.Add(err);
        }

        private void CloseCurrentNode()
        {
            if (_currentnode.Closed)
            {
                return;
            }
            bool error = false;
            var prev = Utilities.GetDictionaryValueOrNull(Lastnodes, _currentnode.Name);
            if (prev != null)
            {
                if (OptionFixNestedTags && FindResetterNodes(prev, GetResetters(_currentnode.Name)))
                {
                    AddError(HtmlParseErrorCode.EndTagInvalidHere, _currentnode._line, _currentnode._lineposition,
                        _currentnode._streamposition, _currentnode.OuterHtml,
                        string.Concat("End tag </", _currentnode.Name, "> invalid here"));
                    error = true;
                }
                if (!error)
                {
                    Lastnodes[_currentnode.Name] = prev._prevwithsamename;
                    prev.CloseNode(_currentnode, 0);
                }
            }
            else if (HtmlNode.IsClosedElement(_currentnode.Name))
            {
                _currentnode.CloseNode(_currentnode, 0);
                if (_lastparentnode != null)
                {
                    HtmlNode foundNode = null;
                    var futureChild = new Stack<HtmlNode>();
                    var node = _lastparentnode.LastChild;
                    while (node != null)
                    {
                        if (!(node.Name == _currentnode.Name) || node.HasChildNodes)
                        {
                            futureChild.Push(node);
                            node = node.PreviousSibling;
                        }
                        else
                        {
                            foundNode = node;
                            break;
                        }
                    }
                    if (foundNode == null)
                    {
                        _lastparentnode.AppendChild(_currentnode);
                    }
                    else
                    {
                        while (futureChild.Count != 0)
                        {
                            var node2 = futureChild.Pop();
                            _lastparentnode.RemoveChild(node2);
                            foundNode.AppendChild(node2);
                        }
                    }
                }
            }
            else if (HtmlNode.CanOverlapElement(_currentnode.Name))
            {
                var closenode = CreateNode(HtmlNodeType.Text, _currentnode._outerstartindex);
                closenode._outerlength = _currentnode._outerlength;
                ((HtmlTextNode) closenode).Text = ((HtmlTextNode) closenode).Text.ToLower();
                if (_lastparentnode != null)
                {
                    _lastparentnode.AppendChild(closenode);
                }
            }
            else if (!HtmlNode.IsEmptyElement(_currentnode.Name))
            {
                AddError(HtmlParseErrorCode.TagNotOpened, _currentnode._line, _currentnode._lineposition,
                    _currentnode._streamposition, _currentnode.OuterHtml,
                    string.Concat("Start tag <", _currentnode.Name, "> was not found"));
                error = true;
            }
            else
            {
                AddError(HtmlParseErrorCode.EndTagNotRequired, _currentnode._line, _currentnode._lineposition,
                    _currentnode._streamposition, _currentnode.OuterHtml,
                    string.Concat("End tag </", _currentnode.Name, "> is not required"));
            }
            if (!error && _lastparentnode != null &&
                (!HtmlNode.IsClosedElement(_currentnode.Name) || _currentnode._starttag))
            {
                UpdateLastParentNode();
            }
        }

        /// <summary>
        ///     Creates an HTML attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the attribute. May not be null.</param>
        /// <returns>The new HTML attribute.</returns>
        public HtmlAttribute CreateAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            var htmlAttribute = CreateAttribute();
            htmlAttribute.Name = name;
            return htmlAttribute;
        }

        /// <summary>
        ///     Creates an HTML attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the attribute. May not be null.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>The new HTML attribute.</returns>
        public HtmlAttribute CreateAttribute(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            var htmlAttribute = CreateAttribute(name);
            htmlAttribute.Value = value;
            return htmlAttribute;
        }

        internal HtmlAttribute CreateAttribute()
        {
            return new HtmlAttribute(this);
        }

        /// <summary>
        ///     Creates an HTML comment node.
        /// </summary>
        /// <returns>The new HTML comment node.</returns>
        public HtmlCommentNode CreateComment()
        {
            return (HtmlCommentNode) CreateNode(HtmlNodeType.Comment);
        }

        /// <summary>
        ///     Creates an HTML comment node with the specified comment text.
        /// </summary>
        /// <param name="comment">The comment text. May not be null.</param>
        /// <returns>The new HTML comment node.</returns>
        public HtmlCommentNode CreateComment(string comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException("comment");
            }
            var htmlCommentNode = CreateComment();
            htmlCommentNode.Comment = comment;
            return htmlCommentNode;
        }

        /// <summary>
        ///     Creates an HTML element node with the specified name.
        /// </summary>
        /// <param name="name">The qualified name of the element. May not be null.</param>
        /// <returns>The new HTML node.</returns>
        public HtmlNode CreateElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            var htmlNode = CreateNode(HtmlNodeType.Element);
            htmlNode.Name = name;
            return htmlNode;
        }

        internal HtmlNode CreateNode(HtmlNodeType type)
        {
            return CreateNode(type, -1);
        }

        internal HtmlNode CreateNode(HtmlNodeType type, int index)
        {
            if (type == HtmlNodeType.Comment)
            {
                return new HtmlCommentNode(this, index);
            }
            if (type == HtmlNodeType.Text)
            {
                return new HtmlTextNode(this, index);
            }
            return new HtmlNode(type, this, index);
        }

        /// <summary>
        ///     Creates an HTML text node.
        /// </summary>
        /// <returns>The new HTML text node.</returns>
        public HtmlTextNode CreateTextNode()
        {
            return (HtmlTextNode) CreateNode(HtmlNodeType.Text);
        }

        /// <summary>
        ///     Creates an HTML text node with the specified text.
        /// </summary>
        /// <param name="text">The text of the node. May not be null.</param>
        /// <returns>The new HTML text node.</returns>
        public HtmlTextNode CreateTextNode(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            var htmlTextNode = CreateTextNode();
            htmlTextNode.Text = text;
            return htmlTextNode;
        }

        private string CurrentNodeName()
        {
            return Text.Substring(_currentnode._namestartindex, _currentnode._namelength);
        }

        private void DecrementPosition()
        {
            _index = _index - 1;
            if (_lineposition != 1)
            {
                _lineposition = _lineposition - 1;
                return;
            }
            _lineposition = _maxlineposition;
            _line = _line - 1;
        }

        /// <summary>
        ///     Detects the encoding of an HTML file.
        /// </summary>
        /// <param name="path">Path for the file containing the HTML document to detect. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(string path)
        {
            Encoding encoding;
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            using (var sr = new StreamReader(path, OptionDefaultStreamEncoding))
            {
                encoding = DetectEncoding(sr);
            }
            return encoding;
        }

        /// <summary>
        ///     Detects the encoding of an HTML stream.
        /// </summary>
        /// <param name="stream">The input stream. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return DetectEncoding(new StreamReader(stream));
        }

        /// <summary>
        ///     Detects the encoding of an HTML text provided on a TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(TextReader reader)
        {
            Encoding encoding;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _onlyDetectEncoding = true;
            if (!OptionCheckSyntax)
            {
                Openednodes = null;
            }
            else
            {
                Openednodes = new Dictionary<int, HtmlNode>();
            }
            if (!OptionUseIdAttribute)
            {
                Nodesid = null;
            }
            else
            {
                Nodesid = new Dictionary<string, HtmlNode>();
            }
            var sr = reader as StreamReader;
            if (sr == null)
            {
                StreamEncoding = null;
            }
            else
            {
                StreamEncoding = sr.CurrentEncoding;
            }
            DeclaredEncoding = null;
            Text = reader.ReadToEnd();
            DocumentNode = CreateNode(HtmlNodeType.Document, 0);
            try
            {
                Parse();
                return null;
            }
            catch (EncodingFoundException encodingFoundException)
            {
                encoding = encodingFoundException.Encoding;
            }
            return encoding;
        }

        /// <summary>
        ///     Detects the encoding of an HTML document from a file first, and then loads the file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public void DetectEncodingAndLoad(string path)
        {
            DetectEncodingAndLoad(path, true);
        }

        /// <summary>
        ///     Detects the encoding of an HTML document from a file first, and then loads the file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="detectEncoding">true to detect encoding, false otherwise.</param>
        public void DetectEncodingAndLoad(string path, bool detectEncoding)
        {
            Encoding enc;
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (!detectEncoding)
            {
                enc = null;
            }
            else
            {
                enc = DetectEncoding(path);
            }
            if (enc == null)
            {
                Load(path);
                return;
            }
            Load(path, enc);
        }

        /// <summary>
        ///     Detects the encoding of an HTML text.
        /// </summary>
        /// <param name="html">The input html text. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncodingHtml(string html)
        {
            Encoding encoding;
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
            using (var sr = new StringReader(html))
            {
                encoding = DetectEncoding(sr);
            }
            return encoding;
        }

        private HtmlNode FindResetterNode(HtmlNode node, string name)
        {
            var resetter = Utilities.GetDictionaryValueOrNull(Lastnodes, name);
            if (resetter == null)
            {
                return null;
            }
            if (resetter.Closed)
            {
                return null;
            }
            if (resetter._streamposition < node._streamposition)
            {
                return null;
            }
            return resetter;
        }

        private bool FindResetterNodes(HtmlNode node, string[] names)
        {
            if (names == null)
            {
                return false;
            }
            for (int i = 0; i < names.Length; i++)
            {
                if (FindResetterNode(node, names[i]) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void FixNestedTag(string name, string[] resetters)
        {
            if (resetters == null)
            {
                return;
            }
            var prev = Utilities.GetDictionaryValueOrNull(Lastnodes, _currentnode.Name);
            if (prev == null || Lastnodes[name].Closed)
            {
                return;
            }
            if (FindResetterNodes(prev, resetters))
            {
                return;
            }
            var close = new HtmlNode(prev.NodeType, this, -1);
            close._endnode = close;
            prev.CloseNode(close, 0);
        }

        private void FixNestedTags()
        {
            if (!_currentnode._starttag)
            {
                return;
            }
            string name = CurrentNodeName();
            FixNestedTag(name, GetResetters(name));
        }

        /// <summary>
        ///     Gets the HTML node with the specified 'id' attribute value.
        /// </summary>
        /// <param name="id">The attribute id to match. May not be null.</param>
        /// <returns>The HTML node with the matching id or null if not found.</returns>
        public HtmlNode GetElementbyId(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (Nodesid == null)
            {
                throw new Exception(HtmlExceptionUseIdAttributeFalse);
            }
            if (!Nodesid.ContainsKey(id.ToLower()))
            {
                return null;
            }
            return Nodesid[id.ToLower()];
        }

        internal Encoding GetOutEncoding()
        {
            return DeclaredEncoding ?? (StreamEncoding ?? OptionDefaultStreamEncoding);
        }

        private string[] GetResetters(string name)
        {
            if (name == "li")
            {
                return new[] {"ul"};
            }
            if (name == "tr")
            {
                return new[] {"table"};
            }
            if (!(name == "th") && !(name == "td"))
            {
                return null;
            }
            return new[] {"tr", "table"};
        }

        internal HtmlNode GetXmlDeclaration()
        {
            HtmlNode htmlNode;
            if (!DocumentNode.HasChildNodes)
            {
                return null;
            }
            using (var enumerator = ((IEnumerable<HtmlNode>) DocumentNode._childnodes).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var node = enumerator.Current;
                    if (node.Name != "?xml")
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
        ///     Gets a valid XML name.
        /// </summary>
        /// <param name="name">Any text.</param>
        /// <returns>A string that is a valid XML name.</returns>
        public static string GetXmlName(string name)
        {
            string xmlname = string.Empty;
            bool nameisok = true;
            for (int i = 0; i < name.Length; i++)
            {
                if ((name[i] < 'a' || name[i] > 'z') && (name[i] < '0' || name[i] > '9') && name[i] != '\u005F' &&
                    name[i] != '-' && name[i] != '.')
                {
                    nameisok = false;
                    var bytes = Encoding.UTF8.GetBytes(new[] {name[i]});
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        xmlname = string.Concat(xmlname, bytes[j].ToString("x2"));
                    }
                    xmlname = string.Concat(xmlname, "_");
                }
                else
                {
                    char chr = name[i];
                    xmlname = string.Concat(xmlname, chr.ToString());
                }
            }
            if (nameisok)
            {
                return xmlname;
            }
            return string.Concat("_", xmlname);
        }

        /// <summary>
        ///     Applies HTML encoding to a specified string.
        /// </summary>
        /// <param name="html">The input string to encode. May not be null.</param>
        /// <returns>The encoded string.</returns>
        public static string HtmlEncode(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
            return
                new Regex("&(?!(amp;)|(lt;)|(gt;)|(quot;))", RegexOptions.IgnoreCase).Replace(html, "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;");
        }

        private void IncrementPosition()
        {
            if (_crc32 != null)
            {
                _crc32.AddToCRC32(_c);
            }
            _index = _index + 1;
            _maxlineposition = _lineposition;
            if (_c != 10)
            {
                _lineposition = _lineposition + 1;
                return;
            }
            _lineposition = 1;
            _line = _line + 1;
        }

        /// <summary>
        ///     Determines if the specified character is considered as a whitespace character.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>true if if the specified character is considered as a whitespace character.</returns>
        public static bool IsWhiteSpace(int c)
        {
            if (c != 10 && c != 13 && c != 32 && c != 9)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        public void Load(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            using (var sr = new StreamReader(path, OptionDefaultStreamEncoding))
            {
                Load(sr);
            }
        }

        /// <summary>
        ///     Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public void Load(string path, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            using (var sr = new StreamReader(path, detectEncodingFromByteOrderMarks))
            {
                Load(sr);
            }
        }

        /// <summary>
        ///     Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Load(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            using (var sr = new StreamReader(path, encoding))
            {
                Load(sr);
            }
        }

        /// <summary>
        ///     Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            using (var sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks))
            {
                Load(sr);
            }
        }

        /// <summary>
        ///     Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            using (var sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize))
            {
                Load(sr);
            }
        }

        /// <summary>
        ///     Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public void Load(Stream stream)
        {
            Load(new StreamReader(stream, OptionDefaultStreamEncoding));
        }

        /// <summary>
        ///     Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     stream.
        /// </param>
        public void Load(Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        ///     Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Load(Stream stream, Encoding encoding)
        {
            Load(new StreamReader(stream, encoding));
        }

        /// <summary>
        ///     Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     stream.
        /// </param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        ///     Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     stream.
        /// </param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize));
        }

        /// <summary>
        ///     Loads the HTML document from the specified TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML data into the document. May not be null.</param>
        public void Load(TextReader reader)
        {
            string html;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _onlyDetectEncoding = false;
            if (!OptionCheckSyntax)
            {
                Openednodes = null;
            }
            else
            {
                Openednodes = new Dictionary<int, HtmlNode>();
            }
            if (!OptionUseIdAttribute)
            {
                Nodesid = null;
            }
            else
            {
                Nodesid = new Dictionary<string, HtmlNode>();
            }
            var sr = reader as StreamReader;
            if (sr == null)
            {
                StreamEncoding = null;
            }
            else
            {
                try
                {
                    sr.Peek();
                }
                catch (Exception exception)
                {
                }
                StreamEncoding = sr.CurrentEncoding;
            }
            DeclaredEncoding = null;
            Text = reader.ReadToEnd();
            DocumentNode = CreateNode(HtmlNodeType.Document, 0);
            Parse();
            if (!OptionCheckSyntax || Openednodes == null)
            {
                return;
            }
            foreach (var node in Openednodes.Values)
            {
                if (!node._starttag)
                {
                    continue;
                }
                if (!OptionExtractErrorSourceText)
                {
                    html = string.Empty;
                }
                else
                {
                    html = node.OuterHtml;
                    if (html.Length > OptionExtractErrorSourceTextMaxLength)
                    {
                        html = html.Substring(0, OptionExtractErrorSourceTextMaxLength);
                    }
                }
                AddError(HtmlParseErrorCode.TagNotClosed, node._line, node._lineposition, node._streamposition, html,
                    string.Concat("End tag </", node.Name, "> was not found"));
            }
            Openednodes.Clear();
        }

        /// <summary>
        ///     Loads the HTML document from the specified string.
        /// </summary>
        /// <param name="html">String containing the HTML document to load. May not be null.</param>
        public void LoadHtml(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
            using (var sr = new StringReader(html))
            {
                Load(sr);
            }
        }

        private bool NewCheck()
        {
            if (_c != 60)
            {
                return false;
            }
            if (_index < Text.Length && Text[_index] == '%')
            {
                var parseState = _state;
                if (parseState == ParseState.WhichTag)
                {
                    PushNodeNameStart(true, _index - 1);
                    _state = ParseState.Tag;
                }
                else if (parseState == ParseState.BetweenAttributes)
                {
                    PushAttributeNameStart(_index - 1);
                }
                else if (parseState == ParseState.AttributeAfterEquals)
                {
                    PushAttributeValueStart(_index - 1);
                }
                _oldstate = _state;
                _state = ParseState.ServerSideCode;
                return true;
            }
            if (!PushNodeEnd(_index - 1, true))
            {
                _index = Text.Length;
                return true;
            }
            _state = ParseState.WhichTag;
            if (_index - 1 > Text.Length - 2 || Text[_index] != '!')
            {
                PushNodeStart(HtmlNodeType.Element, _index - 1);
                return true;
            }
            PushNodeStart(HtmlNodeType.Comment, _index - 1);
            PushNodeNameStart(true, _index);
            PushNodeNameEnd(_index + 1);
            _state = ParseState.Comment;
            if (_index < Text.Length - 2)
            {
                if (Text[_index + 1] != '-' || Text[_index + 2] != '-')
                {
                    _fullcomment = false;
                }
                else
                {
                    _fullcomment = true;
                }
            }
            return true;
        }

        private void Parse()
        {
            int lastquote = 0;
            if (OptionComputeChecksum)
            {
                _crc32 = new Crc32();
            }
            Lastnodes = new Dictionary<string, HtmlNode>();
            _c = 0;
            _fullcomment = false;
            _parseerrors = new List<HtmlParseError>();
            _line = 1;
            _lineposition = 1;
            _maxlineposition = 1;
            _state = ParseState.Text;
            _oldstate = _state;
            DocumentNode._innerlength = Text.Length;
            DocumentNode._outerlength = Text.Length;
            RemainderOffset = Text.Length;
            _lastparentnode = DocumentNode;
            _currentnode = CreateNode(HtmlNodeType.Text, 0);
            _currentattribute = null;
            _index = 0;
            PushNodeStart(HtmlNodeType.Text, 0);
            while (_index < Text.Length)
            {
                _c = Text[_index];
                IncrementPosition();
                var parseState = _state;
                switch (parseState)
                {
                    case ParseState.Text:
                    {
                        if (!NewCheck())
                        {
                        }
                        continue;
                    }
                    case ParseState.WhichTag:
                    {
                        if (NewCheck())
                        {
                            continue;
                        }
                        if (_c != 47)
                        {
                            PushNodeNameStart(true, _index - 1);
                            DecrementPosition();
                        }
                        else
                        {
                            PushNodeNameStart(false, _index);
                        }
                        _state = ParseState.Tag;
                        continue;
                    }
                    case ParseState.Tag:
                    {
                        if (NewCheck())
                        {
                            continue;
                        }
                        if (IsWhiteSpace(_c))
                        {
                            PushNodeNameEnd(_index - 1);
                            if (_state != ParseState.Tag)
                            {
                                continue;
                            }
                            _state = ParseState.BetweenAttributes;
                            continue;
                        }
                        if (_c != 47)
                        {
                            if (_c != 62)
                            {
                                continue;
                            }
                            PushNodeNameEnd(_index - 1);
                            if (_state != ParseState.Tag)
                            {
                                continue;
                            }
                            if (PushNodeEnd(_index, false))
                            {
                                if (_state != ParseState.Tag)
                                {
                                    continue;
                                }
                                _state = ParseState.Text;
                                PushNodeStart(HtmlNodeType.Text, _index);
                                continue;
                            }
                            _index = Text.Length;
                            continue;
                        }
                        PushNodeNameEnd(_index - 1);
                        if (_state != ParseState.Tag)
                        {
                            continue;
                        }
                        _state = ParseState.EmptyTag;
                        continue;
                    }
                    case ParseState.BetweenAttributes:
                    {
                        if (NewCheck() || IsWhiteSpace(_c))
                        {
                            continue;
                        }
                        if (_c == 47 || _c == 63)
                        {
                            _state = ParseState.EmptyTag;
                            continue;
                        }
                        if (_c != 62)
                        {
                            PushAttributeNameStart(_index - 1);
                            _state = ParseState.AttributeName;
                            continue;
                        }
                        if (PushNodeEnd(_index, false))
                        {
                            if (_state != ParseState.BetweenAttributes)
                            {
                                continue;
                            }
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        _index = Text.Length;
                        continue;
                    }
                    case ParseState.EmptyTag:
                    {
                        if (NewCheck())
                        {
                            continue;
                        }
                        if (_c != 62)
                        {
                            _state = ParseState.BetweenAttributes;
                            continue;
                        }
                        if (PushNodeEnd(_index, true))
                        {
                            if (_state != ParseState.EmptyTag)
                            {
                                continue;
                            }
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        _index = Text.Length;
                        continue;
                    }
                    case ParseState.AttributeName:
                    {
                        if (NewCheck())
                        {
                            continue;
                        }
                        if (IsWhiteSpace(_c))
                        {
                            PushAttributeNameEnd(_index - 1);
                            _state = ParseState.AttributeBeforeEquals;
                            continue;
                        }
                        if (_c != 61)
                        {
                            if (_c != 62)
                            {
                                continue;
                            }
                            PushAttributeNameEnd(_index - 1);
                            if (PushNodeEnd(_index, false))
                            {
                                if (_state != ParseState.AttributeName)
                                {
                                    continue;
                                }
                                _state = ParseState.Text;
                                PushNodeStart(HtmlNodeType.Text, _index);
                                continue;
                            }
                            _index = Text.Length;
                            continue;
                        }
                        PushAttributeNameEnd(_index - 1);
                        _state = ParseState.AttributeAfterEquals;
                        continue;
                    }
                    case ParseState.AttributeBeforeEquals:
                    {
                        if (NewCheck() || IsWhiteSpace(_c))
                        {
                            continue;
                        }
                        if (_c == 62)
                        {
                            if (PushNodeEnd(_index, false))
                            {
                                if (_state != ParseState.AttributeBeforeEquals)
                                {
                                    continue;
                                }
                                _state = ParseState.Text;
                                PushNodeStart(HtmlNodeType.Text, _index);
                                continue;
                            }
                            _index = Text.Length;
                            continue;
                        }
                        if (_c != 61)
                        {
                            _state = ParseState.BetweenAttributes;
                            DecrementPosition();
                            continue;
                        }
                        _state = ParseState.AttributeAfterEquals;
                        continue;
                    }
                    case ParseState.AttributeAfterEquals:
                    {
                        if (NewCheck() || IsWhiteSpace(_c))
                        {
                            continue;
                        }
                        if (_c == 39 || _c == 34)
                        {
                            _state = ParseState.QuotedAttributeValue;
                            PushAttributeValueStart(_index, _c);
                            lastquote = _c;
                            continue;
                        }
                        if (_c != 62)
                        {
                            PushAttributeValueStart(_index - 1);
                            _state = ParseState.AttributeValue;
                            continue;
                        }
                        if (PushNodeEnd(_index, false))
                        {
                            if (_state != ParseState.AttributeAfterEquals)
                            {
                                continue;
                            }
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        _index = Text.Length;
                        continue;
                    }
                    case ParseState.AttributeValue:
                    {
                        if (NewCheck())
                        {
                            continue;
                        }
                        if (!IsWhiteSpace(_c))
                        {
                            if (_c != 62)
                            {
                                continue;
                            }
                            PushAttributeValueEnd(_index - 1);
                            if (PushNodeEnd(_index, false))
                            {
                                if (_state != ParseState.AttributeValue)
                                {
                                    continue;
                                }
                                _state = ParseState.Text;
                                PushNodeStart(HtmlNodeType.Text, _index);
                                continue;
                            }
                            _index = Text.Length;
                            continue;
                        }
                        PushAttributeValueEnd(_index - 1);
                        _state = ParseState.BetweenAttributes;
                        continue;
                    }
                    case ParseState.Comment:
                    {
                        if (_c != 62 || _fullcomment && (Text[_index - 2] != '-' || Text[_index - 3] != '-'))
                        {
                            continue;
                        }
                        if (PushNodeEnd(_index, false))
                        {
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        _index = Text.Length;
                        continue;
                    }
                    case ParseState.QuotedAttributeValue:
                    {
                        if (_c != lastquote)
                        {
                            if (_c != 60 || _index >= Text.Length || Text[_index] != '%')
                            {
                                continue;
                            }
                            _oldstate = _state;
                            _state = ParseState.ServerSideCode;
                            continue;
                        }
                        PushAttributeValueEnd(_index - 1);
                        _state = ParseState.BetweenAttributes;
                        continue;
                    }
                    case ParseState.ServerSideCode:
                    {
                        if (_c != 37 || _index >= Text.Length || Text[_index] != '>')
                        {
                            continue;
                        }
                        parseState = _oldstate;
                        if (parseState == ParseState.BetweenAttributes)
                        {
                            PushAttributeNameEnd(_index + 1);
                            _state = ParseState.BetweenAttributes;
                        }
                        else if (parseState != ParseState.AttributeAfterEquals)
                        {
                            _state = _oldstate;
                        }
                        else
                        {
                            _state = ParseState.AttributeValue;
                        }
                        IncrementPosition();
                        continue;
                    }
                    case ParseState.PcData:
                    {
                        if (_currentnode._namelength + 3 > Text.Length - (_index - 1) ||
                            string.Compare(Text.Substring(_index - 1, _currentnode._namelength + 2),
                                string.Concat("</", _currentnode.Name), StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        int c = Text[_index - 1 + 2 + _currentnode.Name.Length];
                        if (c != 62 && !IsWhiteSpace(c))
                        {
                            continue;
                        }
                        var script = CreateNode(HtmlNodeType.Text,
                            _currentnode._outerstartindex + _currentnode._outerlength);
                        script._outerlength = _index - 1 - script._outerstartindex;
                        _currentnode.AppendChild(script);
                        PushNodeStart(HtmlNodeType.Element, _index - 1);
                        PushNodeNameStart(false, _index - 1 + 2);
                        _state = ParseState.Tag;
                        IncrementPosition();
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
            }
            if (_currentnode._namestartindex > 0)
            {
                PushNodeNameEnd(_index);
            }
            PushNodeEnd(_index, false);
            Lastnodes.Clear();
        }

        private void PushAttributeNameEnd(int index)
        {
            _currentattribute._namelength = index - _currentattribute._namestartindex;
            _currentnode.Attributes.Append(_currentattribute);
        }

        private void PushAttributeNameStart(int index)
        {
            _currentattribute = CreateAttribute();
            _currentattribute._namestartindex = index;
            _currentattribute.Line = _line;
            _currentattribute._lineposition = _lineposition;
            _currentattribute._streamposition = index;
        }

        private void PushAttributeValueEnd(int index)
        {
            _currentattribute._valuelength = index - _currentattribute._valuestartindex;
        }

        private void PushAttributeValueStart(int index)
        {
            PushAttributeValueStart(index, 0);
        }

        private void PushAttributeValueStart(int index, int quote)
        {
            _currentattribute._valuestartindex = index;
            if (quote == 39)
            {
                _currentattribute.QuoteType = AttributeValueQuote.SingleQuote;
            }
        }

        private bool PushNodeEnd(int index, bool close)
        {
            _currentnode._outerlength = index - _currentnode._outerstartindex;
            if (_currentnode._nodetype != HtmlNodeType.Text && _currentnode._nodetype != HtmlNodeType.Comment)
            {
                if (_currentnode._starttag && _lastparentnode != _currentnode)
                {
                    if (_lastparentnode != null)
                    {
                        _lastparentnode.AppendChild(_currentnode);
                    }
                    ReadDocumentEncoding(_currentnode);
                    var prev = Utilities.GetDictionaryValueOrNull(Lastnodes, _currentnode.Name);
                    _currentnode._prevwithsamename = prev;
                    Lastnodes[_currentnode.Name] = _currentnode;
                    if (_currentnode.NodeType == HtmlNodeType.Document || _currentnode.NodeType == HtmlNodeType.Element)
                    {
                        _lastparentnode = _currentnode;
                    }
                    if (HtmlNode.IsCDataElement(CurrentNodeName()))
                    {
                        _state = ParseState.PcData;
                        return true;
                    }
                    if (HtmlNode.IsClosedElement(_currentnode.Name) || HtmlNode.IsEmptyElement(_currentnode.Name))
                    {
                        close = true;
                    }
                }
            }
            else if (_currentnode._outerlength > 0)
            {
                _currentnode._innerlength = _currentnode._outerlength;
                _currentnode._innerstartindex = _currentnode._outerstartindex;
                if (_lastparentnode != null)
                {
                    _lastparentnode.AppendChild(_currentnode);
                }
            }
            if (close || !_currentnode._starttag)
            {
                if (OptionStopperNodeName != null && Remainder == null &&
                    string.Compare(_currentnode.Name, OptionStopperNodeName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    RemainderOffset = index;
                    Remainder = Text.Substring(RemainderOffset);
                    CloseCurrentNode();
                    return false;
                }
                CloseCurrentNode();
            }
            return true;
        }

        private void PushNodeNameEnd(int index)
        {
            _currentnode._namelength = index - _currentnode._namestartindex;
            if (OptionFixNestedTags)
            {
                FixNestedTags();
            }
        }

        private void PushNodeNameStart(bool starttag, int index)
        {
            _currentnode._starttag = starttag;
            _currentnode._namestartindex = index;
        }

        private void PushNodeStart(HtmlNodeType type, int index)
        {
            _currentnode = CreateNode(type, index);
            _currentnode._line = _line;
            _currentnode._lineposition = _lineposition;
            if (type == HtmlNodeType.Element)
            {
                var htmlNode = _currentnode;
                htmlNode._lineposition = htmlNode._lineposition - 1;
            }
            _currentnode._streamposition = index;
        }

        private void ReadDocumentEncoding(HtmlNode node)
        {
            if (!OptionReadEncoding)
            {
                return;
            }
            if (node._namelength != 4)
            {
                return;
            }
            if (node.Name != "meta")
            {
                return;
            }
            var att = node.Attributes["http-equiv"];
            if (att == null)
            {
                return;
            }
            if (string.Compare(att.Value, "content-type", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return;
            }
            var content = node.Attributes["content"];
            if (content != null)
            {
                string charset = NameValuePairList.GetNameValuePairsValue(content.Value, "charset");
                if (!string.IsNullOrEmpty(charset))
                {
                    if (string.Equals(charset, "utf8", StringComparison.OrdinalIgnoreCase))
                    {
                        charset = "utf-8";
                    }
                    try
                    {
                        DeclaredEncoding = Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException argumentException)
                    {
                        DeclaredEncoding = null;
                    }
                    if (_onlyDetectEncoding)
                    {
                        throw new EncodingFoundException(DeclaredEncoding);
                    }
                    if (StreamEncoding != null && DeclaredEncoding != null &&
                        DeclaredEncoding.WindowsCodePage != StreamEncoding.WindowsCodePage)
                    {
                        AddError(HtmlParseErrorCode.CharsetMismatch, _line, _lineposition, _index, node.OuterHtml,
                            string.Concat("Encoding mismatch between StreamEncoding: ", StreamEncoding.WebName,
                                " and DeclaredEncoding: ", DeclaredEncoding.WebName));
                    }
                }
            }
        }

        /// <summary>
        ///     Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        public void Save(string filename)
        {
            using (var sw = new StreamWriter(filename, false, GetOutEncoding()))
            {
                Save(sw);
            }
        }

        /// <summary>
        ///     Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Save(string filename, Encoding encoding)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            using (var sw = new StreamWriter(filename, false, encoding))
            {
                Save(sw);
            }
        }

        /// <summary>
        ///     Saves the HTML document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        public void Save(Stream outStream)
        {
            Save(new StreamWriter(outStream, GetOutEncoding()));
        }

        /// <summary>
        ///     Saves the HTML document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Save(Stream outStream, Encoding encoding)
        {
            if (outStream == null)
            {
                throw new ArgumentNullException("outStream");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            Save(new StreamWriter(outStream, encoding));
        }

        /// <summary>
        ///     Saves the HTML document to the specified StreamWriter.
        /// </summary>
        /// <param name="writer">The StreamWriter to which you want to save.</param>
        public void Save(StreamWriter writer)
        {
            Save((TextWriter) writer);
        }

        /// <summary>
        ///     Saves the HTML document to the specified TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save. May not be null.</param>
        public void Save(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            DocumentNode.WriteTo(writer, 0);
            writer.Flush();
        }

        /// <summary>
        ///     Saves the HTML document to the specified XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to which you want to save.</param>
        public void Save(XmlWriter writer)
        {
            DocumentNode.WriteTo(writer);
            writer.Flush();
        }

        internal void SetIdForNode(HtmlNode node, string id)
        {
            if (!OptionUseIdAttribute)
            {
                return;
            }
            if (Nodesid == null || id == null)
            {
                return;
            }
            if (node == null)
            {
                Nodesid.Remove(id.ToLower());
                return;
            }
            Nodesid[id.ToLower()] = node;
        }

        internal void UpdateLastParentNode()
        {
            do
            {
                if (!_lastparentnode.Closed)
                {
                    continue;
                }
                _lastparentnode = _lastparentnode.ParentNode;
            } while (_lastparentnode != null && _lastparentnode.Closed);
            if (_lastparentnode == null)
            {
                _lastparentnode = DocumentNode;
            }
        }

        private enum ParseState
        {
            Text,
            WhichTag,
            Tag,
            BetweenAttributes,
            EmptyTag,
            AttributeName,
            AttributeBeforeEquals,
            AttributeAfterEquals,
            AttributeValue,
            Comment,
            QuotedAttributeValue,
            ServerSideCode,
            PcData
        }
    }
}