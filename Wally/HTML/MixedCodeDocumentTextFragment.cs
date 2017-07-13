namespace Wally.HTML
{
    /// <summary>
    ///     Represents a fragment of text in a mixed code document.
    /// </summary>
    internal class MixedCodeDocumentTextFragment : MixedCodeDocumentFragment
    {
        internal MixedCodeDocumentTextFragment(MixedCodeDocument doc) : base(doc, MixedCodeDocumentFragmentType.Text)
        {
        }

        /// <summary>
        ///     Gets the fragment text.
        /// </summary>
        public string Text
        {
            get { return FragmentText; }
            set { FragmentText = value; }
        }
    }
}