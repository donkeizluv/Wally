using System;

namespace Wally.HTML
{
    internal class HtmlCmdLine
    {
        internal static bool Help;

        static HtmlCmdLine()
        {
            Help = false;
            ParseArgs();
        }

        internal static string GetOption(string name, string def)
        {
            string p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetStringArg(args[i], name, ref p);
            }
            return p;
        }

        internal static string GetOption(int index, string def)
        {
            string p = def;
            string[] args = Environment.GetCommandLineArgs();
            int i = 0;
            for (int j = 1; j < args.Length; j++)
            {
                if (GetStringArg(args[j], ref p))
                {
                    if (index == i)
                    {
                        return p;
                    }
                    p = def;
                    i++;
                }
            }
            return p;
        }

        internal static bool GetOption(string name, bool def)
        {
            bool p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetBoolArg(args[i], name, ref p);
            }
            return p;
        }

        internal static int GetOption(string name, int def)
        {
            int p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetIntArg(args[i], name, ref p);
            }
            return p;
        }

        private static void GetBoolArg(string Arg, string Name, ref bool ArgValue)
        {
            if (Arg.Length < Name.Length + 1)
            {
                return;
            }
            if ('/' != Arg[0] && '-' != Arg[0])
            {
                return;
            }
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
            {
                ArgValue = true;
            }
        }

        private static void GetIntArg(string Arg, string Name, ref int ArgValue)
        {
            if (Arg.Length < Name.Length + 3)
            {
                return;
            }
            if ('/' != Arg[0] && '-' != Arg[0])
            {
                return;
            }
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
            {
                try
                {
                    ArgValue = Convert.ToInt32(Arg.Substring(Name.Length + 2, Arg.Length - Name.Length - 2));
                }
                catch
                {
                }
            }
        }

        private static bool GetStringArg(string Arg, ref string ArgValue)
        {
            if ('/' == Arg[0] || '-' == Arg[0])
            {
                return false;
            }
            ArgValue = Arg;
            return true;
        }

        private static void GetStringArg(string Arg, string Name, ref string ArgValue)
        {
            if (Arg.Length < Name.Length + 3)
            {
                return;
            }
            if ('/' != Arg[0] && '-' != Arg[0])
            {
                return;
            }
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
            {
                ArgValue = Arg.Substring(Name.Length + 2, Arg.Length - Name.Length - 2);
            }
        }

        private static void ParseArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetBoolArg(args[i], "?", ref Help);
                GetBoolArg(args[i], "h", ref Help);
                GetBoolArg(args[i], "help", ref Help);
            }
        }
    }
}