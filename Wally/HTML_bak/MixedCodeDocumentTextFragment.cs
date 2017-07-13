namespace Wally.HTML
{
    /// <summary>
    /// Represents a fragment of text in a mixed code document.
    /// </summary>
    internal class MixedCodeDocumentTextFragment : MixedCodeDocumentFragment
    {
        /// <summary>
        /// Gets the fragment text.
        /// </summary>
        public string Text
        {
            get { return FragmentText; }
            set { FragmentText = value; }
        }

        internal MixedCodeDocumentTextFragment(MixedCodeDocument doc) : base(doc, MixedCodeDocumentFragmentType.Text)
        {
        }
    }
}