using System;
using System.IO;
using System.Text;

namespace Wally.HTML
{
    /// <summary>
    ///     Represents a document with mixed code and text. ASP, ASPX, JSP, are good example of such documents.
    /// </summary>
    internal class MixedCodeDocument
    {
        private readonly string TokenTextBlock = "TextBlock({0})";
        private int _c;

        internal MixedCodeDocumentFragmentList _codefragments;

        private MixedCodeDocumentFragment _currentfragment;

        internal MixedCodeDocumentFragmentList _fragments;

        private int _index;

        private int _line;

        private int _lineposition;

        private ParseState _state;

        internal string _text;

        internal MixedCodeDocumentFragmentList _textfragments;

        /// <summary>
        ///     Gets or sets the token representing code end.
        /// </summary>
        public string TokenCodeEnd = "%>";

        /// <summary>
        ///     Gets or sets the token representing code start.
        /// </summary>
        public string TokenCodeStart = "<%";

        /// <summary>
        ///     Gets or sets the token representing code directive.
        /// </summary>
        public string TokenDirective = "@";

        /// <summary>
        ///     Gets or sets the token representing response write directive.
        /// </summary>
        public string TokenResponseWrite = "Response.Write ";

        /// <summary>
        ///     Creates a mixed code document instance.
        /// </summary>
        public MixedCodeDocument()
        {
            _codefragments = new MixedCodeDocumentFragmentList(this);
            _textfragments = new MixedCodeDocumentFragmentList(this);
            _fragments = new MixedCodeDocumentFragmentList(this);
        }

        /// <summary>
        ///     Gets the code represented by the mixed code document seen as a template.
        /// </summary>
        public string Code
        {
            get
            {
                string s = "";
                int i = 0;
                foreach (var frag in _fragments)
                {
                    var mixedCodeDocumentFragmentType = frag._type;
                    if (mixedCodeDocumentFragmentType == MixedCodeDocumentFragmentType.Code)
                    {
                        s = string.Concat(s, ((MixedCodeDocumentCodeFragment) frag).Code, "\n");
                    }
                    else
                    {
                        if (mixedCodeDocumentFragmentType != MixedCodeDocumentFragmentType.Text)
                        {
                            continue;
                        }
                        s = string.Concat(s, TokenResponseWrite, string.Format(TokenTextBlock, i), "\n");
                        i++;
                    }
                }
                return s;
            }
        }

        /// <summary>
        ///     Gets the list of code fragments in the document.
        /// </summary>
        public MixedCodeDocumentFragmentList CodeFragments
        {
            get { return _codefragments; }
        }

        /// <summary>
        ///     Gets the list of all fragments in the document.
        /// </summary>
        public MixedCodeDocumentFragmentList Fragments
        {
            get { return _fragments; }
        }

        /// <summary>
        ///     Gets the encoding of the stream used to read the document.
        /// </summary>
        public Encoding StreamEncoding { get; private set; }

        /// <summary>
        ///     Gets the list of text fragments in the document.
        /// </summary>
        public MixedCodeDocumentFragmentList TextFragments
        {
            get { return _textfragments; }
        }

        /// <summary>
        ///     Create a code fragment instances.
        /// </summary>
        /// <returns>The newly created code fragment instance.</returns>
        public MixedCodeDocumentCodeFragment CreateCodeFragment()
        {
            return (MixedCodeDocumentCodeFragment) CreateFragment(MixedCodeDocumentFragmentType.Code);
        }

        internal MixedCodeDocumentFragment CreateFragment(MixedCodeDocumentFragmentType type)
        {
            if (type == MixedCodeDocumentFragmentType.Code)
            {
                return new MixedCodeDocumentCodeFragment(this);
            }
            if (type != MixedCodeDocumentFragmentType.Text)
            {
                throw new NotSupportedException();
            }
            return new MixedCodeDocumentTextFragment(this);
        }

        /// <summary>
        ///     Create a text fragment instances.
        /// </summary>
        /// <returns>The newly created text fragment instance.</returns>
        public MixedCodeDocumentTextFragment CreateTextFragment()
        {
            return (MixedCodeDocumentTextFragment) CreateFragment(MixedCodeDocumentFragmentType.Text);
        }

        internal Encoding GetOutEncoding()
        {
            if (StreamEncoding == null)
            {
                return Encoding.UTF8;
            }
            return StreamEncoding;
        }

        private void IncrementPosition()
        {
            _index = _index + 1;
            if (_c != 10)
            {
                _lineposition = _lineposition + 1;
                return;
            }
            _lineposition = 1;
            _line = _line + 1;
        }

        /// <summary>
        ///     Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public void Load(Stream stream)
        {
            Load(new StreamReader(stream));
        }

        /// <summary>
        ///     Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public void Load(Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        ///     Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Load(Stream stream, Encoding encoding)
        {
            Load(new StreamReader(stream, encoding));
        }

        /// <summary>
        ///     Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        ///     Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize));
        }

        /// <summary>
        ///     Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public void Load(string path)
        {
            Load(new StreamReader(path));
        }

        /// <summary>
        ///     Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public void Load(string path, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(path, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        ///     Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Load(string path, Encoding encoding)
        {
            Load(new StreamReader(path, encoding));
        }

        /// <summary>
        ///     Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(path, encoding, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        ///     Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">
        ///     Indicates whether to look for byte order marks at the beginning of the
        ///     file.
        /// </param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            Load(new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize));
        }

        /// <summary>
        ///     Loads the mixed code document from the specified TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
        public void Load(TextReader reader)
        {
            _codefragments.Clear();
            _textfragments.Clear();
            var sr = reader as StreamReader;
            if (sr != null)
            {
                StreamEncoding = sr.CurrentEncoding;
            }
            _text = reader.ReadToEnd();
            reader.Close();
            Parse();
        }

        /// <summary>
        ///     Loads a mixed document from a text
        /// </summary>
        /// <param name="html">The text to load.</param>
        public void LoadHtml(string html)
        {
            Load(new StringReader(html));
        }

        private void Parse()
        {
            _state = ParseState.Text;
            _index = 0;
            _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);
            while (_index < _text.Length)
            {
                _c = _text[_index];
                IncrementPosition();
                var parseState = _state;
                if (parseState == ParseState.Text)
                {
                    if (_index + TokenCodeStart.Length >= _text.Length ||
                        !(_text.Substring(_index - 1, TokenCodeStart.Length) == TokenCodeStart))
                    {
                        continue;
                    }
                    _state = ParseState.Code;
                    _currentfragment.Length = _index - 1 - _currentfragment.Index;
                    _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Code);
                    SetPosition();
                }
                else if (parseState == ParseState.Code)
                {
                    if (_index + TokenCodeEnd.Length >= _text.Length ||
                        !(_text.Substring(_index - 1, TokenCodeEnd.Length) == TokenCodeEnd))
                    {
                        continue;
                    }
                    _state = ParseState.Text;
                    _currentfragment.Length = _index + TokenCodeEnd.Length - _currentfragment.Index;
                    _index = _index + TokenCodeEnd.Length;
                    _lineposition = _lineposition + TokenCodeEnd.Length;
                    _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);
                    SetPosition();
                }
            }
            _currentfragment.Length = _index - _currentfragment.Index;
        }

        /// <summary>
        ///     Saves the mixed document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        public void Save(Stream outStream)
        {
            Save(new StreamWriter(outStream, GetOutEncoding()));
        }

        /// <summary>
        ///     Saves the mixed document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Save(Stream outStream, Encoding encoding)
        {
            Save(new StreamWriter(outStream, encoding));
        }

        /// <summary>
        ///     Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        public void Save(string filename)
        {
            Save(new StreamWriter(filename, false, GetOutEncoding()));
        }

        /// <summary>
        ///     Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Save(string filename, Encoding encoding)
        {
            Save(new StreamWriter(filename, false, encoding));
        }

        /// <summary>
        ///     Saves the mixed document to the specified StreamWriter.
        /// </summary>
        /// <param name="writer">The StreamWriter to which you want to save.</param>
        public void Save(StreamWriter writer)
        {
            Save((TextWriter) writer);
        }

        /// <summary>
        ///     Saves the mixed document to the specified TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save.</param>
        public void Save(TextWriter writer)
        {
            writer.Flush();
        }

        private void SetPosition()
        {
            _currentfragment.Line = _line;
            _currentfragment._lineposition = _lineposition;
            _currentfragment.Index = _index - 1;
            _currentfragment.Length = 0;
        }

        private enum ParseState
        {
            Text,
            Code
        }
    }
}