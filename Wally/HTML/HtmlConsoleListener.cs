using System;
using System.Diagnostics;

namespace Wally.HTML
{
    internal class HtmlConsoleListener : TraceListener
    {
        public override void Write(string Message)
        {
            Write(Message, "");
        }

        public override void Write(string Message, string Category)
        {
            Console.Write(string.Concat("T:", Category, ": ", Message));
        }

        public override void WriteLine(string Message)
        {
            Write(string.Concat(Message, "\n"));
        }

        public override void WriteLine(string Message, string Category)
        {
            Write(string.Concat(Message, "\n"), Category);
        }
    }
}