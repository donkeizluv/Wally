namespace Wally.HTML
{
    /// <summary>
    /// Represents a fragment of code in a mixed code document.
    /// </summary>
    internal class MixedCodeDocumentCodeFragment : MixedCodeDocumentFragment
    {
        private string _code;

        /// <summary>
        /// Gets the fragment code text.
        /// </summary>
        public string Code
        {
            get
            {
                if (_code == null)
                {
                    _code =
                        FragmentText.Substring(Doc.TokenCodeStart.Length,
                            FragmentText.Length - Doc.TokenCodeEnd.Length - Doc.TokenCodeStart.Length - 1)
                            .Trim();
                    if (_code.StartsWith("="))
                    {
                        _code = Doc.TokenResponseWrite + _code.Substring(1, _code.Length - 1);
                    }
                }
                return _code;
            }
            set { _code = value; }
        }

        internal MixedCodeDocumentCodeFragment(MixedCodeDocument doc) : base(doc, MixedCodeDocumentFragmentType.Code)
        {
        }
    }
}