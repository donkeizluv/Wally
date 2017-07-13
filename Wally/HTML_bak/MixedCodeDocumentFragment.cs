namespace Wally.HTML
{
    /// <summary>
    /// Represents a base class for fragments in a mixed code document.
    /// </summary>
    internal abstract class MixedCodeDocumentFragment
    {
        internal MixedCodeDocument Doc;
        private string _fragmentText;
        internal int Index;
        internal int Length;
        internal int _lineposition;
        internal MixedCodeDocumentFragmentType _type;

        /// <summary>
        /// Gets the fragement text.
        /// </summary>
        public string FragmentText
        {
            get
            {
                if (_fragmentText == null)
                {
                    _fragmentText = Doc._text.Substring(Index, Length);
                }
                return FragmentText;
            }
            internal set { _fragmentText = value; }
        }

        /// <summary>
        /// Gets the type of fragment.
        /// </summary>
        public MixedCodeDocumentFragmentType FragmentType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the line number of the fragment.
        /// </summary>
        public int Line { get; internal set; }

        /// <summary>
        /// Gets the line position (column) of the fragment.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
        }

        /// <summary>
        /// Gets the fragment position in the document's stream.
        /// </summary>
        public int StreamPosition
        {
            get { return Index; }
        }

        internal MixedCodeDocumentFragment(MixedCodeDocument doc, MixedCodeDocumentFragmentType type)
        {
            Doc = doc;
            _type = type;
            switch (type)
            {
                case MixedCodeDocumentFragmentType.Code:
                    Doc._codefragments.Append(this);
                    break;
                case MixedCodeDocumentFragmentType.Text:
                    Doc._textfragments.Append(this);
                    break;
            }
            Doc._fragments.Append(this);
        }
    }
}