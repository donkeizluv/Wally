using System;

namespace Wally.HTML
{
    /// <summary>
    /// Represents an exception thrown by the HtmlWeb utility class.
    /// </summary>
    internal class HtmlWebException : Exception
    {
        /// <summary>
        /// Creates an instance of the HtmlWebException.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public HtmlWebException(string message) : base(message)
        {
        }
    }
}